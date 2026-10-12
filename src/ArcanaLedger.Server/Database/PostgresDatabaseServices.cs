using ArcanaLedger.Server.Dtos.Accounts;
using ArcanaLedger.Server.Dtos.Activity;
using ArcanaLedger.Server.Dtos.Auth;
using ArcanaLedger.Server.Dtos.Dashboard;
using ArcanaLedger.Server.Dtos.Instruments;
using ArcanaLedger.Server.Dtos.Portfolios;
using ArcanaLedger.Server.Dtos.Settings;
using ArcanaLedger.Server.Dtos.Snapshots;
using ArcanaLedger.Server.Dtos.Transactions;
using ArcanaLedger.Server.Dtos.Trades;
using ArcanaLedger.Server.Dtos.TradeSetups;
using ArcanaLedger.Server.Dtos.Valuations;
using ArcanaLedger.Server.Dtos.Watchlist;
using ArcanaLedger.Server.Services;
using Npgsql;

namespace ArcanaLedger.Server.Database;

internal abstract class PostgresService(NpgsqlDataSource dataSource)
{
    protected ValueTask<NpgsqlConnection> OpenAsync(CancellationToken cancellationToken)
        => dataSource.OpenConnectionAsync(cancellationToken);

    protected static NpgsqlCommand Command(NpgsqlConnection connection, string sql)
        => new(sql, connection);

    protected static NpgsqlParameter Add(NpgsqlCommand command, string name, object? value)
        => command.Parameters.AddWithValue(name, value ?? DBNull.Value);

    protected static async Task EnsurePortfolioOwnedAsync(
        NpgsqlConnection connection,
        Guid userId,
        Guid portfolioId,
        CancellationToken cancellationToken)
    {
        await using var command = Command(connection, "SELECT user_id FROM portfolios WHERE portfolio_id = @portfolio_id");
        Add(command, "portfolio_id", portfolioId);
        var owner = await command.ExecuteScalarAsync(cancellationToken);
        if (owner is null)
        {
            throw ApiException.NotFound("Portfolio", portfolioId);
        }

        if ((Guid)owner != userId)
        {
            throw ApiException.Forbidden("You do not have access to this portfolio.");
        }
    }

    protected static async Task EnsureOwnedAsync(
        NpgsqlConnection connection,
        string table,
        string idColumn,
        string resource,
        Guid userId,
        Guid resourceId,
        CancellationToken cancellationToken)
    {
        var sql = $"SELECT user_id FROM {table} WHERE {idColumn} = @resource_id";
        await using var command = Command(connection, sql);
        Add(command, "resource_id", resourceId);
        var owner = await command.ExecuteScalarAsync(cancellationToken);
        if (owner is null)
        {
            throw ApiException.NotFound(resource, resourceId);
        }

        if ((Guid)owner != userId)
        {
            throw ApiException.Forbidden($"You do not have access to this {resource.ToLowerInvariant()}.");
        }
    }

    protected static DateTimeOffset Timestamp(NpgsqlDataReader reader, string name)
        => reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal(name));

    protected static async Task<NpgsqlDataReader> ExecuteReaderAsync(
        NpgsqlCommand command,
        CancellationToken cancellationToken)
        => await command.ExecuteReaderAsync(cancellationToken);

    protected static bool IsUniqueViolation(PostgresException exception)
        => exception.SqlState == PostgresErrorCodes.UniqueViolation;

}

internal sealed class PostgresAuthService(
    NpgsqlDataSource dataSource,
    IJwtTokenService tokenService) : PostgresService(dataSource), IAuthService
{
    public async Task<RegisteredUserResponse> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, """
            INSERT INTO app_users (email, display_name, password_hash)
            VALUES (@email::citext, @display_name, @password_hash)
            RETURNING user_id, email::text, display_name, created_at
            """);
        Add(command, "email", request.Email);
        Add(command, "display_name", request.DisplayName);
        Add(command, "password_hash", PasswordHasher.Hash(request.Password));

        try
        {
            await using var reader = await ExecuteReaderAsync(command, cancellationToken);
            await reader.ReadAsync(cancellationToken);
            return new RegisteredUserResponse(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                Timestamp(reader, "created_at"));
        }
        catch (PostgresException exception) when (IsUniqueViolation(exception))
        {
            throw ApiException.Conflict(
                "email_taken",
                "Email is already registered.",
                $"An account with email '{request.Email}' already exists.");
        }
    }

    public async Task<LoginResponse> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, """
            SELECT user_id, email::text, display_name, password_hash
            FROM app_users
            WHERE email = @email::citext
            """);
        Add(command, "email", request.Email);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)
            || !PasswordHasher.Verify(request.Password, reader.GetString(3)))
        {
            throw ApiException.Unauthorized("Email or password is incorrect.");
        }

        var userId = reader.GetGuid(0);
        var email = reader.GetString(1);
        var displayName = reader.GetString(2);
        var issued = tokenService.IssueToken(userId, email);
        return new LoginResponse(issued.AccessToken, issued.ExpiresAt, new LoginUserSummary(userId, email, displayName));
    }

    public Task Logout(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public async Task<AuthenticatedUserResponse> GetCurrentUser(Guid userId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, """
            SELECT user_id, email::text, display_name, created_at, updated_at
            FROM app_users
            WHERE user_id = @user_id
            """);
        Add(command, "user_id", userId);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw ApiException.NotFound("User", userId);
        }

        return new AuthenticatedUserResponse(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            Timestamp(reader, "created_at"),
            Timestamp(reader, "updated_at"));
    }
}

internal sealed class PostgresPortfolioService(NpgsqlDataSource dataSource)
    : PostgresService(dataSource), IPortfolioService
{
    public async Task<IReadOnlyList<PortfolioResponse>> List(Guid userId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, """
            SELECT portfolio_id, user_id, name, base_currency, is_default, created_at, updated_at
            FROM portfolios
            WHERE user_id = @user_id
            ORDER BY created_at
            """);
        Add(command, "user_id", userId);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        var results = new List<PortfolioResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(ReadPortfolio(reader));
        }

        return results;
    }

    public async Task<PortfolioResponse> Create(Guid userId, CreatePortfolioRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var command = Command(connection, """
            INSERT INTO portfolios (user_id, name, base_currency, is_default)
            VALUES (@user_id, @name, @base_currency, false)
            RETURNING portfolio_id, user_id, name, base_currency, is_default, created_at, updated_at
            """);
        command.Transaction = transaction;
        Add(command, "user_id", userId);
        Add(command, "name", request.Name);
        Add(command, "base_currency", request.BaseCurrency);
        PortfolioResponse result;
        await using (var reader = await ExecuteReaderAsync(command, cancellationToken))
        {
            await reader.ReadAsync(cancellationToken);
            result = ReadPortfolio(reader);
        }

        if (request.IsDefault)
        {
            await using var setDefault = Command(connection, "CALL set_default_portfolio(@user_id, @portfolio_id)");
            setDefault.Transaction = transaction;
            Add(setDefault, "user_id", userId);
            Add(setDefault, "portfolio_id", result.Id);
            await setDefault.ExecuteNonQueryAsync(cancellationToken);
            result = result with { IsDefault = true, UpdatedAt = DateTimeOffset.UtcNow };
        }

        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task<PortfolioResponse> Get(Guid userId, Guid portfolioId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        return await GetAsync(connection, userId, portfolioId, cancellationToken);
    }

    public async Task<PortfolioResponse> Update(Guid userId, Guid portfolioId, UpdatePortfolioRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);

        if (request.IsDefault is true)
        {
            await using var setDefault = Command(connection, "CALL set_default_portfolio(@user_id, @portfolio_id)");
            setDefault.Transaction = transaction;
            Add(setDefault, "user_id", userId);
            Add(setDefault, "portfolio_id", portfolioId);
            await setDefault.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var command = Command(connection, """
            UPDATE portfolios
            SET name = COALESCE(@name, name),
                base_currency = COALESCE(@base_currency, base_currency),
                is_default = COALESCE(@is_default, is_default),
                updated_at = now()
            WHERE user_id = @user_id AND portfolio_id = @portfolio_id
            RETURNING portfolio_id, user_id, name, base_currency, is_default, created_at, updated_at
            """);
        command.Transaction = transaction;
        Add(command, "name", request.Name);
        Add(command, "base_currency", request.BaseCurrency);
        Add(command, "is_default", request.IsDefault);
        Add(command, "user_id", userId);
        Add(command, "portfolio_id", portfolioId);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        await reader.ReadAsync(cancellationToken);
        var result = ReadPortfolio(reader);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task Delete(Guid userId, Guid portfolioId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
        await using var command = Command(connection, "DELETE FROM portfolios WHERE portfolio_id = @portfolio_id");
        Add(command, "portfolio_id", portfolioId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task EnsureOwned(Guid userId, Guid portfolioId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
    }

    internal static async Task<PortfolioResponse> GetAsync(
        NpgsqlConnection connection,
        Guid userId,
        Guid portfolioId,
        CancellationToken cancellationToken)
    {
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
        await using var command = Command(connection, """
            SELECT portfolio_id, user_id, name, base_currency, is_default, created_at, updated_at
            FROM portfolios
            WHERE user_id = @user_id AND portfolio_id = @portfolio_id
            """);
        Add(command, "user_id", userId);
        Add(command, "portfolio_id", portfolioId);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw ApiException.NotFound("Portfolio", portfolioId);
        }

        return ReadPortfolio(reader);
    }

    private static PortfolioResponse ReadPortfolio(NpgsqlDataReader reader)
        => new(
            reader.GetGuid(0),
            reader.GetGuid(1),
            reader.GetString(2),
            reader.GetString(3).Trim(),
            reader.GetBoolean(4),
            Timestamp(reader, "created_at"),
            Timestamp(reader, "updated_at"));
}

internal sealed class PostgresBrokerageAccountService(NpgsqlDataSource dataSource)
    : PostgresService(dataSource), IBrokerageAccountService
{
    public async Task<IReadOnlyList<BrokerageAccountResponse>> List(Guid userId, Guid portfolioId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
        await using var command = Command(connection, """
            SELECT account_id, portfolio_id, name, COALESCE(broker_name, ''), currency, is_active, created_at
            FROM brokerage_accounts
            WHERE portfolio_id = @portfolio_id
            ORDER BY created_at
            """);
        Add(command, "portfolio_id", portfolioId);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        var results = new List<BrokerageAccountResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(ReadAccount(reader));
        }

        return results;
    }

    public async Task<BrokerageAccountResponse> Create(Guid userId, Guid portfolioId, CreateAccountRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
        await using var command = Command(connection, """
            INSERT INTO brokerage_accounts (portfolio_id, name, broker_name, currency)
            VALUES (@portfolio_id, @name, @broker_name, @currency)
            RETURNING account_id, portfolio_id, name, broker_name, currency, is_active, created_at
            """);
        Add(command, "portfolio_id", portfolioId);
        Add(command, "name", request.Name);
        Add(command, "broker_name", request.BrokerName);
        Add(command, "currency", request.Currency);

        try
        {
            await using var reader = await ExecuteReaderAsync(command, cancellationToken);
            await reader.ReadAsync(cancellationToken);
            return ReadAccount(reader);
        }
        catch (PostgresException exception) when (IsUniqueViolation(exception))
        {
            throw ApiException.Conflict("account_name_exists", "An account with that name already exists in this portfolio.");
        }
    }

    public async Task<BrokerageAccountResponse> Update(Guid userId, Guid portfolioId, Guid accountId, UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
        await using var command = Command(connection, """
            UPDATE brokerage_accounts
            SET name = COALESCE(@name, name),
                currency = COALESCE(@currency, currency),
                is_active = COALESCE(@is_active, is_active)
            WHERE portfolio_id = @portfolio_id AND account_id = @account_id
            RETURNING account_id, portfolio_id, name, broker_name, currency, is_active, created_at
            """);
        Add(command, "name", request.Name);
        Add(command, "currency", request.Currency);
        Add(command, "is_active", request.IsActive);
        Add(command, "portfolio_id", portfolioId);
        Add(command, "account_id", accountId);
        try
        {
            await using var reader = await ExecuteReaderAsync(command, cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                throw ApiException.NotFound("Account", accountId);
            }

            return ReadAccount(reader);
        }
        catch (PostgresException exception) when (IsUniqueViolation(exception))
        {
            throw ApiException.Conflict("account_name_exists", "An account with that name already exists in this portfolio.");
        }
    }

    public async Task Delete(Guid userId, Guid portfolioId, Guid accountId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
        await using var command = Command(connection, "DELETE FROM brokerage_accounts WHERE account_id = @account_id AND portfolio_id = @portfolio_id");
        Add(command, "account_id", accountId);
        Add(command, "portfolio_id", portfolioId);
        try
        {
            if (await command.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                throw ApiException.NotFound("Account", accountId);
            }
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw ApiException.Conflict("account_in_use", "Account cannot be deleted while it is used by a trade.");
        }
    }

    private static BrokerageAccountResponse ReadAccount(NpgsqlDataReader reader)
        => new(
            reader.GetGuid(0),
            reader.GetGuid(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? "" : reader.GetString(3),
            reader.GetString(4).Trim(),
            reader.GetBoolean(5),
            Timestamp(reader, "created_at"));
}

internal sealed class PostgresInstrumentService(NpgsqlDataSource dataSource)
    : PostgresService(dataSource), IInstrumentService
{
    public async Task<IReadOnlyList<InstrumentResponse>> Search(string? search, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, """
            SELECT i.instrument_id, i.symbol, i.name, i.sector, i.asset_type, i.currency, i.is_active,
                   q.last_price, q.previous_close, q.change_amount, q.change_percent, q.tone, q.quoted_at
            FROM instruments i
            LEFT JOIN instrument_quotes q ON q.instrument_id = i.instrument_id
            WHERE @search IS NULL
               OR position(lower(@search) in lower(i.symbol)) > 0
               OR position(lower(@search) in lower(i.name)) > 0
            ORDER BY i.symbol
            """);
        Add(command, "search", string.IsNullOrWhiteSpace(search) ? null : search.Trim());
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        var results = new List<InstrumentResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(ReadInstrument(reader));
        }

        return results;
    }

    public async Task<InstrumentResponse> Get(Guid instrumentId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, """
            SELECT i.instrument_id, i.symbol, i.name, i.sector, i.asset_type, i.currency, i.is_active,
                   q.last_price, q.previous_close, q.change_amount, q.change_percent, q.tone, q.quoted_at
            FROM instruments i
            LEFT JOIN instrument_quotes q ON q.instrument_id = i.instrument_id
            WHERE i.instrument_id = @instrument_id
            """);
        Add(command, "instrument_id", instrumentId);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw ApiException.NotFound("Instrument", instrumentId);
        }

        return ReadInstrument(reader);
    }

    private static InstrumentResponse ReadInstrument(NpgsqlDataReader reader)
    {
        QuoteResponse? quote = reader.IsDBNull(7)
            ? null
            : new QuoteResponse(
                reader.GetDecimal(7),
                reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                reader.IsDBNull(9) ? null : reader.GetDecimal(9),
                reader.IsDBNull(10) ? null : reader.GetDecimal(10),
                reader.GetString(11),
                Timestamp(reader, "quoted_at"));

        return new InstrumentResponse(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5).Trim(),
            reader.GetBoolean(6),
            quote);
    }
}

internal sealed class PostgresWatchlistService(NpgsqlDataSource dataSource)
    : PostgresService(dataSource), IWatchlistService
{
    private const string SelectSql = """
        SELECT w.portfolio_id, i.instrument_id, i.symbol, i.name, i.sector,
               q.last_price, q.change_amount, q.change_percent, q.tone,
               w.display_order, w.added_at
        FROM watchlist_items w
        JOIN instruments i ON i.instrument_id = w.instrument_id
        LEFT JOIN instrument_quotes q ON q.instrument_id = i.instrument_id
        """;

    public async Task<IReadOnlyList<WatchlistItemResponse>> List(Guid userId, Guid portfolioId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
        await using var command = Command(connection, SelectSql + """
            WHERE w.portfolio_id = @portfolio_id
            ORDER BY w.display_order, w.added_at
            """);
        Add(command, "portfolio_id", portfolioId);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        var items = new List<WatchlistItemResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(ReadWatchlistItem(reader));
        }

        return items;
    }

    public async Task<WatchlistItemResponse> Add(Guid userId, Guid portfolioId, AddWatchlistItemRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
        await using var command = Command(connection, """
            INSERT INTO watchlist_items (portfolio_id, instrument_id, display_order)
            VALUES (@portfolio_id, @instrument_id, @display_order)
            RETURNING added_at
            """);
        Add(command, "portfolio_id", portfolioId);
        Add(command, "instrument_id", request.InstrumentId);
        Add(command, "display_order", request.DisplayOrder);

        DateTimeOffset addedAt;
        try
        {
            await using (var reader = await ExecuteReaderAsync(command, cancellationToken))
            {
                if (!await reader.ReadAsync(cancellationToken))
                {
                    throw ApiException.NotFound("Instrument", request.InstrumentId);
                }

                addedAt = Timestamp(reader, "added_at");
            }

            return await GetItemAsync(connection, portfolioId, request.InstrumentId, request.DisplayOrder, addedAt, cancellationToken);
        }
        catch (PostgresException exception) when (IsUniqueViolation(exception))
        {
            throw ApiException.Conflict("watchlist_item_exists", "Instrument is already on the watchlist.");
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw ApiException.NotFound("Instrument", request.InstrumentId);
        }
    }

    public async Task<WatchlistItemResponse> Reorder(Guid userId, Guid portfolioId, Guid instrumentId, UpdateWatchlistItemRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
        await using var command = Command(connection, """
            UPDATE watchlist_items SET display_order = @display_order
            WHERE portfolio_id = @portfolio_id AND instrument_id = @instrument_id
            RETURNING added_at
            """);
        Add(command, "display_order", request.DisplayOrder);
        Add(command, "portfolio_id", portfolioId);
        Add(command, "instrument_id", instrumentId);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw ApiException.NotFound("Watchlist item", instrumentId);
        }

        var addedAt = Timestamp(reader, "added_at");
        await reader.DisposeAsync();
        return await GetItemAsync(connection, portfolioId, instrumentId, request.DisplayOrder, addedAt, cancellationToken);
    }

    public async Task Remove(Guid userId, Guid portfolioId, Guid instrumentId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
        await using var command = Command(connection, """
            DELETE FROM watchlist_items
            WHERE portfolio_id = @portfolio_id AND instrument_id = @instrument_id
            """);
        Add(command, "portfolio_id", portfolioId);
        Add(command, "instrument_id", instrumentId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0)
        {
            throw ApiException.NotFound("Watchlist item", instrumentId);
        }
    }

    private static async Task<WatchlistItemResponse> GetItemAsync(
        NpgsqlConnection connection,
        Guid portfolioId,
        Guid instrumentId,
        int displayOrder,
        DateTimeOffset addedAt,
        CancellationToken cancellationToken)
    {
        await using var command = Command(connection, """
            SELECT i.instrument_id, i.symbol, i.name, i.sector,
                   q.last_price, q.change_amount, q.change_percent, q.tone
            FROM instruments i
            LEFT JOIN instrument_quotes q ON q.instrument_id = i.instrument_id
            WHERE i.instrument_id = @instrument_id
            """);
        Add(command, "instrument_id", instrumentId);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw ApiException.NotFound("Instrument", instrumentId);
        }

        WatchlistQuoteSummary? quote = reader.IsDBNull(4)
            ? null
            : new WatchlistQuoteSummary(
                reader.GetDecimal(4),
                reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                reader.GetString(7));
        return new WatchlistItemResponse(
            portfolioId,
            new WatchlistInstrumentSummary(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3)),
            quote,
            displayOrder,
            addedAt);
    }

    private static WatchlistItemResponse ReadWatchlistItem(NpgsqlDataReader reader)
    {
        WatchlistQuoteSummary? quote = reader.IsDBNull(5)
            ? null
            : new WatchlistQuoteSummary(
                reader.GetDecimal(5),
                reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                reader.IsDBNull(7) ? null : reader.GetDecimal(7),
                reader.GetString(8));
        return new WatchlistItemResponse(
            reader.GetGuid(0),
            new WatchlistInstrumentSummary(
                reader.GetGuid(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4)),
            quote,
            reader.GetInt32(9),
            Timestamp(reader, "added_at"));
    }
}

internal sealed class PostgresTradeSetupService(NpgsqlDataSource dataSource)
    : PostgresService(dataSource), ITradeSetupService
{
    private const string SelectColumns = """
        setup_id, user_id, code, name, family, entry_rule, exit_rule, trend,
        uses_tranches, tranche_count, var_multiplier, usage_notes, created_at, updated_at
        """;

    public async Task<IReadOnlyList<TradeSetupResponse>> List(Guid userId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, $"SELECT {SelectColumns} FROM trade_setups WHERE user_id = @user_id ORDER BY created_at");
        Add(command, "user_id", userId);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        var results = new List<TradeSetupResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(ReadSetup(reader));
        }

        return results;
    }

    public async Task<TradeSetupResponse> Create(Guid userId, CreateTradeSetupRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, """
            INSERT INTO trade_setups
                (user_id, code, name, family, entry_rule, exit_rule, trend, uses_tranches, tranche_count, var_multiplier, usage_notes)
            VALUES
                (@user_id, @code, @name, @family, @entry_rule, @exit_rule, @trend, @uses_tranches, @tranche_count, @var_multiplier, @usage_notes)
            RETURNING setup_id, user_id, code, name, family, entry_rule, exit_rule, trend,
                      uses_tranches, tranche_count, var_multiplier, usage_notes, created_at, updated_at
            """);
        Add(command, "user_id", userId);
        Add(command, "code", request.Code);
        Add(command, "name", request.Name);
        Add(command, "family", request.Family);
        Add(command, "entry_rule", request.EntryRule);
        Add(command, "exit_rule", request.ExitRule);
        Add(command, "trend", request.Trend);
        Add(command, "uses_tranches", request.UsesTranches);
        Add(command, "tranche_count", request.TrancheCount);
        Add(command, "var_multiplier", request.VarMultiplier);
        Add(command, "usage_notes", request.UsageNotes);
        try
        {
            await using var reader = await ExecuteReaderAsync(command, cancellationToken);
            await reader.ReadAsync(cancellationToken);
            return ReadSetup(reader);
        }
        catch (PostgresException exception) when (IsUniqueViolation(exception))
        {
            throw ApiException.Conflict("trade_setup_code_exists", "A trade setup with that code already exists.");
        }
    }

    public async Task<TradeSetupResponse> Get(Guid userId, Guid setupId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        return await GetSetupAsync(connection, userId, setupId, cancellationToken);
    }

    public async Task<TradeSetupResponse> Update(Guid userId, Guid setupId, UpdateTradeSetupRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsureOwnedAsync(connection, "trade_setups", "setup_id", "Trade setup", userId, setupId, cancellationToken);
        await using var command = Command(connection, """
            UPDATE trade_setups
            SET code = COALESCE(@code, code),
                name = COALESCE(@name, name),
                family = COALESCE(@family, family),
                entry_rule = COALESCE(@entry_rule, entry_rule),
                exit_rule = COALESCE(@exit_rule, exit_rule),
                trend = COALESCE(@trend, trend),
                uses_tranches = COALESCE(@uses_tranches, uses_tranches),
                tranche_count = COALESCE(@tranche_count, tranche_count),
                var_multiplier = COALESCE(@var_multiplier, var_multiplier),
                usage_notes = COALESCE(@usage_notes, usage_notes),
                updated_at = now()
            WHERE setup_id = @setup_id
            RETURNING setup_id, user_id, code, name, family, entry_rule, exit_rule, trend,
                      uses_tranches, tranche_count, var_multiplier, usage_notes, created_at, updated_at
            """);
        Add(command, "code", request.Code);
        Add(command, "name", request.Name);
        Add(command, "family", request.Family);
        Add(command, "entry_rule", request.EntryRule);
        Add(command, "exit_rule", request.ExitRule);
        Add(command, "trend", request.Trend);
        Add(command, "uses_tranches", request.UsesTranches);
        Add(command, "tranche_count", request.TrancheCount);
        Add(command, "var_multiplier", request.VarMultiplier);
        Add(command, "usage_notes", request.UsageNotes);
        Add(command, "setup_id", setupId);
        try
        {
            await using var reader = await ExecuteReaderAsync(command, cancellationToken);
            await reader.ReadAsync(cancellationToken);
            return ReadSetup(reader);
        }
        catch (PostgresException exception) when (IsUniqueViolation(exception))
        {
            throw ApiException.Conflict("trade_setup_code_exists", "A trade setup with that code already exists.");
        }
    }

    public async Task Delete(Guid userId, Guid setupId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsureOwnedAsync(connection, "trade_setups", "setup_id", "Trade setup", userId, setupId, cancellationToken);
        await using var command = Command(connection, "DELETE FROM trade_setups WHERE setup_id = @setup_id");
        Add(command, "setup_id", setupId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<TradeSetupResponse> GetSetupAsync(
        NpgsqlConnection connection,
        Guid userId,
        Guid setupId,
        CancellationToken cancellationToken)
    {
        await EnsureOwnedAsync(connection, "trade_setups", "setup_id", "Trade setup", userId, setupId, cancellationToken);
        await using var command = Command(connection, $"SELECT {SelectColumns} FROM trade_setups WHERE setup_id = @setup_id");
        Add(command, "setup_id", setupId);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return ReadSetup(reader);
    }

    private static TradeSetupResponse ReadSetup(NpgsqlDataReader reader)
        => new(
            reader.GetGuid(0),
            reader.GetGuid(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
            reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
            reader.IsDBNull(7) ? "neutral" : reader.GetString(7),
            reader.GetBoolean(8),
            reader.IsDBNull(9) ? null : reader.GetInt16(9),
            reader.IsDBNull(10) ? null : reader.GetDecimal(10),
            reader.IsDBNull(11) ? null : reader.GetString(11),
            Timestamp(reader, "created_at"),
            Timestamp(reader, "updated_at"));
}

internal sealed class PostgresTradeService(NpgsqlDataSource dataSource)
    : PostgresService(dataSource), ITradeService
{
    private const string TradeSelect = """
        SELECT t.trade_id, t.portfolio_id, t.account_id, t.instrument_id, i.symbol,
               t.setup_id, s.code, s.name, t.order_type, t.status, t.opened_on, t.closed_on,
               t.entry_target, t.cut_loss, t.target_price, t.unrealized_pnl_amount,
               t.unrealized_pnl_percent, t.position_result, t.notes, t.created_at, t.updated_at
        FROM trades t
        JOIN instruments i ON i.instrument_id = t.instrument_id
        LEFT JOIN trade_setups s ON s.setup_id = t.setup_id
        """;

    public async Task<IReadOnlyList<TradeListItemResponse>> List(Guid userId, TradeListFilter filter, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        if (filter.PortfolioId is { } portfolioId)
        {
            await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
        }

        await using var command = Command(connection, """
            SELECT t.trade_id, i.symbol, s.name,
                   COALESCE(SUM(e.quantity) FILTER (WHERE e.side = 'buy'), 0)
                     - COALESCE(SUM(e.quantity) FILTER (WHERE e.side = 'sell'), 0) AS shares,
                   t.opened_on,
                   CASE WHEN COALESCE(SUM(e.quantity) FILTER (WHERE e.side = 'buy'), 0) > 0
                     THEN SUM(e.quantity * e.price) FILTER (WHERE e.side = 'buy')
                          / SUM(e.quantity) FILTER (WHERE e.side = 'buy')
                     ELSE t.entry_target END AS entry_price,
                   t.cut_loss, t.target_price, t.position_result, q.last_price, q.change_percent
            FROM trades t
            JOIN portfolios p ON p.portfolio_id = t.portfolio_id
            JOIN instruments i ON i.instrument_id = t.instrument_id
            LEFT JOIN trade_setups s ON s.setup_id = t.setup_id
            LEFT JOIN trade_executions e ON e.trade_id = t.trade_id
            LEFT JOIN instrument_quotes q ON q.instrument_id = t.instrument_id
            WHERE p.user_id = @user_id
              AND (@portfolio_id IS NULL OR t.portfolio_id = @portfolio_id)
              AND (@status IS NULL OR t.status = @status)
              AND (@symbol IS NULL OR lower(i.symbol) = lower(@symbol))
            GROUP BY t.trade_id, i.symbol, s.name, t.opened_on, t.entry_target, t.cut_loss,
                     t.target_price, t.position_result, q.last_price, q.change_percent
            ORDER BY t.opened_on DESC
            """);
        Add(command, "user_id", userId);
        Add(command, "portfolio_id", filter.PortfolioId);
        Add(command, "status", string.IsNullOrWhiteSpace(filter.Status) ? null : filter.Status);
        Add(command, "symbol", string.IsNullOrWhiteSpace(filter.Symbol) ? null : filter.Symbol);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        var results = new List<TradeListItemResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new TradeListItemResponse(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.GetDecimal(3),
                reader.GetFieldValue<DateOnly>(4),
                reader.GetDecimal(5),
                reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                reader.IsDBNull(7) ? null : reader.GetDecimal(7),
                reader.GetString(8),
                reader.IsDBNull(9) ? null : reader.GetDecimal(9),
                reader.IsDBNull(10) ? null : reader.GetDecimal(10)));
        }

        return results;
    }

    public async Task<TradeResponse> Create(Guid userId, CreateTradeRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        var portfolioId = await GetAccountPortfolioAsync(connection, userId, request.AccountId, cancellationToken);
        if (request.SetupId is { } setupId)
        {
            await EnsureOwnedAsync(connection, "trade_setups", "setup_id", "Trade setup", userId, setupId, cancellationToken);
        }

        await using var command = Command(connection, """
            INSERT INTO trades
                (portfolio_id, account_id, instrument_id, setup_id, order_type, status, opened_on,
                 entry_target, cut_loss, target_price, notes)
            VALUES
                (@portfolio_id, @account_id, @instrument_id, @setup_id, @order_type, 'planned', @opened_on,
                 @entry_target, @cut_loss, @target_price, @notes)
            RETURNING trade_id
            """);
        Add(command, "portfolio_id", portfolioId);
        Add(command, "account_id", request.AccountId);
        Add(command, "instrument_id", request.InstrumentId);
        Add(command, "setup_id", request.SetupId);
        Add(command, "order_type", request.OrderType);
        Add(command, "opened_on", request.OpenedOn);
        Add(command, "entry_target", request.EntryTarget);
        Add(command, "cut_loss", request.CutLoss);
        Add(command, "target_price", request.TargetPrice);
        Add(command, "notes", request.Notes);
        Guid tradeId;
        try
        {
            tradeId = (Guid)(await command.ExecuteScalarAsync(cancellationToken)
                ?? throw ApiException.NotFound("Instrument", request.InstrumentId));
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw ApiException.NotFound("Instrument", request.InstrumentId);
        }

        return await GetTradeAsync(connection, userId, tradeId, includeExecutions: false, cancellationToken);
    }

    public async Task<TradeResponse> Get(Guid userId, Guid tradeId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        return await GetTradeAsync(connection, userId, tradeId, includeExecutions: true, cancellationToken);
    }

    public async Task<TradeResponse> Update(Guid userId, Guid tradeId, UpdateTradeRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsureTradeOwnedAsync(connection, userId, tradeId, cancellationToken);
        await using var command = Command(connection, """
            UPDATE trades
            SET entry_target = COALESCE(@entry_target, entry_target),
                cut_loss = COALESCE(@cut_loss, cut_loss),
                target_price = COALESCE(@target_price, target_price),
                status = COALESCE(@status, status),
                closed_on = CASE
                    WHEN @closed_on IS NOT NULL THEN @closed_on
                    WHEN @status = 'closed' THEN COALESCE(closed_on, CURRENT_DATE)
                    WHEN @status IS NOT NULL AND @status <> 'closed' THEN NULL
                    ELSE closed_on
                END,
                notes = COALESCE(@notes, notes),
                updated_at = now()
            WHERE trade_id = @trade_id
            """);
        Add(command, "entry_target", request.EntryTarget);
        Add(command, "cut_loss", request.CutLoss);
        Add(command, "target_price", request.TargetPrice);
        Add(command, "status", request.Status);
        Add(command, "closed_on", request.ClosedOn);
        Add(command, "notes", request.Notes);
        Add(command, "trade_id", tradeId);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return await GetTradeAsync(connection, userId, tradeId, includeExecutions: true, cancellationToken);
    }

    public async Task Delete(Guid userId, Guid tradeId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsureTradeOwnedAsync(connection, userId, tradeId, cancellationToken);
        await using var command = Command(connection, """
            DELETE FROM trades t
            WHERE t.trade_id = @trade_id
              AND NOT EXISTS (SELECT 1 FROM trade_executions e WHERE e.trade_id = t.trade_id)
            """);
        Add(command, "trade_id", tradeId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) == 0)
        {
            throw ApiException.Conflict("trade_has_executions", "Trade cannot be deleted because it has executions.");
        }
    }

    public async Task<IReadOnlyList<TradeExecutionResponse>> ListExecutions(Guid userId, Guid tradeId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsureTradeOwnedAsync(connection, userId, tradeId, cancellationToken);
        return await ReadExecutionsAsync(connection, tradeId, cancellationToken);
    }

    public async Task<TradeExecutionResponse> AddExecution(Guid userId, Guid tradeId, CreateTradeExecutionRequest request, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var trade = await GetTradeContextAsync(connection, userId, tradeId, transaction, cancellationToken);
        await using var command = Command(connection, """
            INSERT INTO trade_executions (trade_id, side, quantity, price, commission, executed_at)
            VALUES (@trade_id, @side, @quantity, @price, @commission, @executed_at)
            RETURNING execution_id, trade_id, side, quantity, price, commission, executed_at
            """);
        command.Transaction = transaction;
        Add(command, "trade_id", tradeId);
        Add(command, "side", request.Side);
        Add(command, "quantity", request.Quantity);
        Add(command, "price", request.Price);
        Add(command, "commission", request.Commission);
        Add(command, "executed_at", request.ExecutedAt);
        TradeExecutionResponse execution;
        await using (var reader = await ExecuteReaderAsync(command, cancellationToken))
        {
            await reader.ReadAsync(cancellationToken);
            execution = ReadExecution(reader);
        }

        await RecalculatePnlAsync(connection, tradeId, transaction, cancellationToken);
        await using var activity = Command(connection, """
            INSERT INTO portfolio_activity (portfolio_id, category, title, detail, occurred_at)
            VALUES (@portfolio_id, 'trade', @title, @detail, @occurred_at)
            """);
        activity.Transaction = transaction;
        Add(activity, "portfolio_id", trade.PortfolioId);
        Add(activity, "title", $"{(request.Side == "buy" ? "Bought" : "Sold")} {trade.Symbol}");
        Add(activity, "detail", $"{request.Quantity:F2} shares at ${request.Price:F2}.");
        Add(activity, "occurred_at", request.ExecutedAt);
        await activity.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return execution;
    }

    private static async Task<Guid> GetAccountPortfolioAsync(
        NpgsqlConnection connection,
        Guid userId,
        Guid accountId,
        CancellationToken cancellationToken)
    {
        await using var command = Command(connection, """
            SELECT a.portfolio_id
            FROM brokerage_accounts a
            JOIN portfolios p ON p.portfolio_id = a.portfolio_id
            WHERE a.account_id = @account_id AND p.user_id = @user_id
            """);
        Add(command, "account_id", accountId);
        Add(command, "user_id", userId);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is null)
        {
            await using var account = Command(connection, "SELECT 1 FROM brokerage_accounts WHERE account_id = @account_id");
            Add(account, "account_id", accountId);
            if (await account.ExecuteScalarAsync(cancellationToken) is not null)
            {
                throw ApiException.Forbidden("You do not have access to this account.");
            }

            throw ApiException.NotFound("Account", accountId);
        }

        return (Guid)result;
    }

    private static async Task EnsureTradeOwnedAsync(
        NpgsqlConnection connection,
        Guid userId,
        Guid tradeId,
        CancellationToken cancellationToken,
        NpgsqlTransaction? transaction = null)
    {
        await using var command = Command(connection, """
            SELECT p.user_id FROM trades t
            JOIN portfolios p ON p.portfolio_id = t.portfolio_id
            WHERE t.trade_id = @trade_id
            """);
        command.Transaction = transaction;
        Add(command, "trade_id", tradeId);
        var owner = await command.ExecuteScalarAsync(cancellationToken);
        if (owner is null)
        {
            throw ApiException.NotFound("Trade", tradeId);
        }

        if ((Guid)owner != userId)
        {
            throw ApiException.Forbidden("You do not have access to this trade.");
        }
    }

    private static async Task<TradeResponse> GetTradeAsync(
        NpgsqlConnection connection,
        Guid userId,
        Guid tradeId,
        bool includeExecutions,
        CancellationToken cancellationToken)
    {
        await EnsureTradeOwnedAsync(connection, userId, tradeId, cancellationToken);
        await using var command = Command(connection, TradeSelect + " WHERE t.trade_id = @trade_id");
        Add(command, "trade_id", tradeId);
        TradeResponse response;
        await using (var reader = await ExecuteReaderAsync(command, cancellationToken))
        {
            if (!await reader.ReadAsync(cancellationToken))
            {
                throw ApiException.NotFound("Trade", tradeId);
            }

            response = ReadTrade(reader);
        }

        if (!includeExecutions)
        {
            return response;
        }

        var executions = await ReadExecutionsAsync(connection, tradeId, cancellationToken);
        return response with { Executions = executions };
    }

    private static TradeResponse ReadTrade(NpgsqlDataReader reader)
    {
        var setup = reader.IsDBNull(5)
            ? null
            : new TradeSetupSummary(reader.GetGuid(5), reader.GetString(6), reader.GetString(7));
        return new TradeResponse(
            reader.GetGuid(0),
            reader.GetGuid(1),
            reader.GetGuid(2),
            new TradeInstrumentSummary(reader.GetGuid(3), reader.GetString(4)),
            setup,
            reader.GetString(8),
            reader.GetString(9),
            reader.GetFieldValue<DateOnly>(10),
            reader.IsDBNull(11) ? null : reader.GetFieldValue<DateOnly>(11),
            reader.GetDecimal(12),
            reader.IsDBNull(13) ? null : reader.GetDecimal(13),
            reader.IsDBNull(14) ? null : reader.GetDecimal(14),
            reader.IsDBNull(15) ? null : reader.GetDecimal(15),
            reader.IsDBNull(16) ? null : reader.GetDecimal(16),
            reader.GetString(17),
            reader.IsDBNull(18) ? null : reader.GetString(18),
            Timestamp(reader, "created_at"),
            Timestamp(reader, "updated_at"));
    }

    private static async Task<IReadOnlyList<TradeExecutionResponse>> ReadExecutionsAsync(
        NpgsqlConnection connection,
        Guid tradeId,
        CancellationToken cancellationToken,
        NpgsqlTransaction? transaction = null)
    {
        await using var command = Command(connection, """
            SELECT execution_id, trade_id, side, quantity, price, commission, executed_at
            FROM trade_executions
            WHERE trade_id = @trade_id
            ORDER BY executed_at
            """);
        command.Transaction = transaction;
        Add(command, "trade_id", tradeId);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        var results = new List<TradeExecutionResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(ReadExecution(reader));
        }

        return results;
    }

    private static TradeExecutionResponse ReadExecution(NpgsqlDataReader reader)
        => new(
            reader.GetGuid(0),
            reader.GetGuid(1),
            reader.GetString(2),
            reader.GetDecimal(3),
            reader.GetDecimal(4),
            reader.GetDecimal(5),
            Timestamp(reader, "executed_at"));

    private static async Task<TradeContext> GetTradeContextAsync(
        NpgsqlConnection connection,
        Guid userId,
        Guid tradeId,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = Command(connection, """
            SELECT t.portfolio_id, t.instrument_id, t.order_type, t.status, i.symbol
            FROM trades t
            JOIN portfolios p ON p.portfolio_id = t.portfolio_id
            JOIN instruments i ON i.instrument_id = t.instrument_id
            WHERE t.trade_id = @trade_id AND p.user_id = @user_id
            """);
        command.Transaction = transaction;
        Add(command, "trade_id", tradeId);
        Add(command, "user_id", userId);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            await reader.DisposeAsync();
            await EnsureTradeOwnedAsync(connection, userId, tradeId, cancellationToken, transaction);
            throw ApiException.NotFound("Trade", tradeId);
        }

        return new TradeContext(reader.GetGuid(0), reader.GetGuid(1), reader.GetString(2), reader.GetString(3), reader.GetString(4));
    }

    private static async Task RecalculatePnlAsync(
        NpgsqlConnection connection,
        Guid tradeId,
        NpgsqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = Command(connection, """
            WITH execution_totals AS (
                SELECT COALESCE(SUM(quantity) FILTER (WHERE side = 'buy'), 0) AS buy_quantity,
                       COALESCE(SUM(quantity) FILTER (WHERE side = 'sell'), 0) AS sell_quantity,
                       COALESCE(SUM(quantity * price) FILTER (WHERE side = 'buy'), 0) AS buy_cost
                FROM trade_executions
                WHERE trade_id = @trade_id
            )
            UPDATE trades t
            SET unrealized_pnl_amount = CASE
                    WHEN q.last_price IS NULL OR x.buy_quantity = 0 OR x.buy_quantity <= x.sell_quantity THEN NULL
                    ELSE (q.last_price - x.buy_cost / x.buy_quantity)
                         * (x.buy_quantity - x.sell_quantity)
                         * CASE WHEN t.order_type = 'short' THEN -1 ELSE 1 END
                END,
                unrealized_pnl_percent = CASE
                    WHEN q.last_price IS NULL OR x.buy_quantity = 0 OR x.buy_quantity <= x.sell_quantity OR x.buy_cost = 0 THEN NULL
                    ELSE ((q.last_price - x.buy_cost / x.buy_quantity)
                         * (x.buy_quantity - x.sell_quantity)
                         * CASE WHEN t.order_type = 'short' THEN -1 ELSE 1 END)
                         / ((x.buy_cost / x.buy_quantity) * (x.buy_quantity - x.sell_quantity)) * 100
                END,
                status = CASE
                    WHEN t.status = 'planned' AND q.last_price IS NOT NULL
                         AND x.buy_quantity > x.sell_quantity THEN 'open'
                    ELSE t.status
                END,
                metrics_updated_at = now(),
                updated_at = now()
            FROM execution_totals x
            LEFT JOIN instrument_quotes q ON q.instrument_id = (
                SELECT instrument_id FROM trades WHERE trade_id = @trade_id
            )
            WHERE t.trade_id = @trade_id
            """);
        command.Transaction = transaction;
        Add(command, "trade_id", tradeId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private sealed record TradeContext(Guid PortfolioId, Guid InstrumentId, string OrderType, string Status, string Symbol);
}

internal sealed class PostgresPortfolioTransactionService(NpgsqlDataSource dataSource)
    : PostgresService(dataSource), IPortfolioTransactionService
{
    public async Task<IReadOnlyList<PortfolioTransactionResponse>> List(
        Guid userId,
        Guid portfolioId,
        TransactionListFilter filter,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
        await using var command = Command(connection, """
            SELECT transaction_id, portfolio_id, account_id, instrument_id, trade_id, transaction_type,
                   cash_amount, currency, quantity, unit_price, occurred_at, memo
            FROM portfolio_transactions
            WHERE portfolio_id = @portfolio_id
              AND (@transaction_type IS NULL OR lower(transaction_type) = lower(@transaction_type))
              AND (@from_at IS NULL OR occurred_at >= @from_at)
              AND (@to_at IS NULL OR occurred_at <= @to_at)
            ORDER BY occurred_at DESC
            """);
        Add(command, "portfolio_id", portfolioId);
        Add(command, "transaction_type", string.IsNullOrWhiteSpace(filter.TransactionType) ? null : filter.TransactionType);
        Add(command, "from_at", filter.From);
        Add(command, "to_at", filter.To);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        var results = new List<PortfolioTransactionResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(ReadTransaction(reader));
        }

        return results;
    }

    public async Task<PortfolioTransactionResponse> Create(
        Guid userId,
        Guid portfolioId,
        CreatePortfolioTransactionRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
        if (request.AccountId is { } accountId)
        {
            await using var ownerCommand = Command(connection, """
                SELECT 1
                FROM brokerage_accounts
                WHERE account_id = @account_id AND portfolio_id = @portfolio_id
                """);
            Add(ownerCommand, "account_id", accountId);
            Add(ownerCommand, "portfolio_id", portfolioId);
            if (await ownerCommand.ExecuteScalarAsync(cancellationToken) is null)
            {
                throw ApiException.NotFound("Account", accountId);
            }
        }

        await using var command = Command(connection, """
            INSERT INTO portfolio_transactions
                (portfolio_id, account_id, transaction_type, cash_amount, currency, occurred_at, memo)
            VALUES
                (@portfolio_id, @account_id, @transaction_type, @cash_amount, @currency, @occurred_at, @memo)
            RETURNING transaction_id, portfolio_id, account_id, instrument_id, trade_id, transaction_type,
                      cash_amount, currency, quantity, unit_price, occurred_at, memo
            """);
        Add(command, "portfolio_id", portfolioId);
        Add(command, "account_id", request.AccountId);
        Add(command, "transaction_type", request.TransactionType);
        Add(command, "cash_amount", request.CashAmount);
        Add(command, "currency", request.Currency);
        Add(command, "occurred_at", request.OccurredAt);
        Add(command, "memo", request.Memo);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return ReadTransaction(reader);
    }

    private static PortfolioTransactionResponse ReadTransaction(NpgsqlDataReader reader)
        => new(
            reader.GetGuid(0),
            reader.GetGuid(1),
            reader.IsDBNull(2) ? null : reader.GetGuid(2),
            reader.IsDBNull(3) ? null : reader.GetGuid(3),
            reader.IsDBNull(4) ? null : reader.GetGuid(4),
            reader.GetString(5),
            reader.GetDecimal(6),
            reader.GetString(7).Trim(),
            reader.IsDBNull(8) ? null : reader.GetDecimal(8),
            reader.IsDBNull(9) ? null : reader.GetDecimal(9),
            Timestamp(reader, "occurred_at"),
            reader.IsDBNull(11) ? null : reader.GetString(11));
}

internal sealed class PostgresPortfolioSnapshotService(NpgsqlDataSource dataSource)
    : PostgresService(dataSource), IPortfolioSnapshotService
{
    public async Task<IReadOnlyList<PortfolioSnapshotResponse>> List(
        Guid userId,
        Guid portfolioId,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
        await using var command = Command(connection, """
            SELECT as_of_date, total_value, cash_value, invested_value,
                   COALESCE(daily_return, 0), COALESCE(time_weighted_return, 0)
            FROM portfolio_snapshots
            WHERE portfolio_id = @portfolio_id
              AND (@from_date IS NULL OR as_of_date >= @from_date)
              AND (@to_date IS NULL OR as_of_date <= @to_date)
            ORDER BY as_of_date
            """);
        Add(command, "portfolio_id", portfolioId);
        Add(command, "from_date", from);
        Add(command, "to_date", to);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        var results = new List<PortfolioSnapshotResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new PortfolioSnapshotResponse(
                reader.GetFieldValue<DateOnly>(0),
                reader.GetDecimal(1),
                reader.GetDecimal(2),
                reader.GetDecimal(3),
                reader.GetDecimal(4),
                reader.GetDecimal(5)));
        }

        return results;
    }
}

internal sealed class PostgresPortfolioActivityService(NpgsqlDataSource dataSource)
    : PostgresService(dataSource), IPortfolioActivityService
{
    public async Task<IReadOnlyList<PortfolioActivityResponse>> List(
        Guid userId,
        Guid portfolioId,
        int limit,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
        await using var command = Command(connection, """
            SELECT activity_id, category, title, COALESCE(detail, ''), occurred_at
            FROM portfolio_activity
            WHERE portfolio_id = @portfolio_id
            ORDER BY occurred_at DESC
            LIMIT @limit
            """);
        Add(command, "portfolio_id", portfolioId);
        Add(command, "limit", Math.Clamp(limit, 1, 100));
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        var results = new List<PortfolioActivityResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new PortfolioActivityResponse(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                Timestamp(reader, "occurred_at")));
        }

        return results;
    }
}

internal sealed class PostgresDashboardService(NpgsqlDataSource dataSource)
    : PostgresService(dataSource), IDashboardService
{
    private const int ActivityLimit = 10;

    public async Task<DashboardSummaryResponse> GetSummary(Guid userId, Guid portfolioId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);

        DashboardPortfolioSummary portfolio;
        await using (var command = Command(connection, """
            SELECT portfolio_id, name, base_currency FROM portfolios
            WHERE portfolio_id = @portfolio_id
            """))
        {
            Add(command, "portfolio_id", portfolioId);
            await using var reader = await ExecuteReaderAsync(command, cancellationToken);
            await reader.ReadAsync(cancellationToken);
            portfolio = new DashboardPortfolioSummary(reader.GetGuid(0), reader.GetString(1), reader.GetString(2).Trim());
        }

        DashboardSnapshotSummary? snapshot = null;
        await using (var command = Command(connection, """
            SELECT as_of_date, total_value, cash_value, invested_value, COALESCE(daily_return, 0)
            FROM portfolio_snapshots
            WHERE portfolio_id = @portfolio_id
            ORDER BY as_of_date DESC
            LIMIT 1
            """))
        {
            Add(command, "portfolio_id", portfolioId);
            await using var reader = await ExecuteReaderAsync(command, cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                snapshot = new DashboardSnapshotSummary(
                    reader.GetFieldValue<DateOnly>(0),
                    reader.GetDecimal(1),
                    reader.GetDecimal(2),
                    reader.GetDecimal(3),
                    reader.GetDecimal(4));
            }
        }

        var watchlist = new List<DashboardWatchlistItem>();
        await using (var command = Command(connection, """
            SELECT i.symbol, i.name, i.sector, COALESCE(q.last_price, 0),
                   q.change_percent, COALESCE(q.tone, 'neutral')
            FROM watchlist_items w
            JOIN instruments i ON i.instrument_id = w.instrument_id
            LEFT JOIN instrument_quotes q ON q.instrument_id = i.instrument_id
            WHERE w.portfolio_id = @portfolio_id
            ORDER BY w.display_order
            """))
        {
            Add(command, "portfolio_id", portfolioId);
            await using var reader = await ExecuteReaderAsync(command, cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                watchlist.Add(new DashboardWatchlistItem(
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? null : reader.GetString(2),
                    reader.GetDecimal(3),
                    reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                    reader.GetString(5)));
            }
        }

        var activity = new List<DashboardActivityItem>();
        await using (var command = Command(connection, """
            SELECT title, COALESCE(detail, ''), occurred_at
            FROM portfolio_activity
            WHERE portfolio_id = @portfolio_id
            ORDER BY occurred_at DESC
            LIMIT @limit
            """))
        {
            Add(command, "portfolio_id", portfolioId);
            Add(command, "limit", ActivityLimit);
            await using var reader = await ExecuteReaderAsync(command, cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                activity.Add(new DashboardActivityItem(
                    reader.GetString(0),
                    reader.GetString(1),
                    Timestamp(reader, "occurred_at")));
            }
        }

        return new DashboardSummaryResponse(portfolio, snapshot, watchlist, activity);
    }
}

internal sealed class PostgresPortfolioSettingsService(NpgsqlDataSource dataSource)
    : PostgresService(dataSource), IPortfolioSettingsService
{
    private const string ReadColumns = """
        portfolio_id, capital, var_rate, cash_value, cash_allocation_percent, reserve_value,
        reserve_allocation_percent, position_entry_price, position_exit_price, position_shares,
        position_cash_value, tranche_entry_price, tranche_exit_price, tranche_shares,
        tranche_cash_value, swing_entry_price, swing_exit_price, swing_shares, swing_cash_value,
        breakout_price, pattern_low_price, swing_target_price, momentum_rate, updated_at
        """;

    private const string WritableColumns = """
        capital, var_rate, cash_value, reserve_value, position_entry_price, position_exit_price,
        position_shares, position_cash_value, tranche_entry_price, tranche_exit_price,
        tranche_shares, tranche_cash_value, swing_entry_price, swing_exit_price, swing_shares,
        swing_cash_value, breakout_price, pattern_low_price, swing_target_price, momentum_rate
        """;

    private const string ParameterNames = """
        @capital, @var_rate, @cash_value, @reserve_value, @position_entry_price, @position_exit_price,
        @position_shares, @position_cash_value, @tranche_entry_price, @tranche_exit_price,
        @tranche_shares, @tranche_cash_value, @swing_entry_price, @swing_exit_price, @swing_shares,
        @swing_cash_value, @breakout_price, @pattern_low_price, @swing_target_price, @momentum_rate
        """;

    public async Task<PortfolioSettingsResponse> Get(Guid userId, Guid portfolioId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
        await using var command = Command(connection, $"SELECT {ReadColumns} FROM portfolio_settings WHERE portfolio_id = @portfolio_id");
        Add(command, "portfolio_id", portfolioId);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw ApiException.NotFound("Portfolio settings", portfolioId);
        }

        return ReadSettings(reader);
    }

    public async Task<PortfolioSettingsResponse> Upsert(
        Guid userId,
        Guid portfolioId,
        UpsertPortfolioSettingsRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, portfolioId, cancellationToken);
        var columns = WritableColumns.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var updates = string.Join(", ", columns.Select(column => $"{column} = EXCLUDED.{column}"));
        await using var command = Command(connection, $"""
            INSERT INTO portfolio_settings (portfolio_id, {WritableColumns})
            VALUES (@portfolio_id, {ParameterNames})
            ON CONFLICT (portfolio_id) DO UPDATE SET {updates}, updated_at = now()
            RETURNING {ReadColumns}
            """);
        Add(command, "portfolio_id", portfolioId);
        Add(command, "capital", request.Capital);
        Add(command, "var_rate", request.VarRate);
        Add(command, "cash_value", request.CashValue);
        Add(command, "reserve_value", request.ReserveValue);
        Add(command, "position_entry_price", request.PositionEntryPrice);
        Add(command, "position_exit_price", request.PositionExitPrice);
        Add(command, "position_shares", request.PositionShares);
        Add(command, "position_cash_value", request.PositionCashValue);
        Add(command, "tranche_entry_price", request.TrancheEntryPrice);
        Add(command, "tranche_exit_price", request.TrancheExitPrice);
        Add(command, "tranche_shares", request.TrancheShares);
        Add(command, "tranche_cash_value", request.TrancheCashValue);
        Add(command, "swing_entry_price", request.SwingEntryPrice);
        Add(command, "swing_exit_price", request.SwingExitPrice);
        Add(command, "swing_shares", request.SwingShares);
        Add(command, "swing_cash_value", request.SwingCashValue);
        Add(command, "breakout_price", request.BreakoutPrice);
        Add(command, "pattern_low_price", request.PatternLowPrice);
        Add(command, "swing_target_price", request.SwingTargetPrice);
        Add(command, "momentum_rate", request.MomentumRate);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return ReadSettings(reader);
    }

    private static PortfolioSettingsResponse ReadSettings(NpgsqlDataReader reader)
        => new(
            reader.GetGuid(0),
            reader.GetDecimal(1),
            Convert.ToDouble(reader.GetDecimal(2)),
            reader.GetDecimal(3),
            Convert.ToDouble(reader.GetDecimal(4)),
            reader.GetDecimal(5),
            Convert.ToDouble(reader.GetDecimal(6)),
            NullableDecimal(reader, 7),
            NullableDecimal(reader, 8),
            NullableDecimal(reader, 9),
            NullableDecimal(reader, 10),
            NullableDecimal(reader, 11),
            NullableDecimal(reader, 12),
            NullableDecimal(reader, 13),
            NullableDecimal(reader, 14),
            NullableDecimal(reader, 15),
            NullableDecimal(reader, 16),
            NullableDecimal(reader, 17),
            NullableDecimal(reader, 18),
            NullableDecimal(reader, 19),
            NullableDecimal(reader, 20),
            NullableDecimal(reader, 21),
            reader.IsDBNull(22) ? null : Convert.ToDouble(reader.GetDecimal(22)),
            Timestamp(reader, "updated_at"));

    private static decimal? NullableDecimal(NpgsqlDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
}

internal sealed class PostgresValuationService(NpgsqlDataSource dataSource)
    : PostgresService(dataSource), IValuationService
{
    private const string Columns = """
        valuation_id, user_id, portfolio_id, instrument_id, name, estimated_revenue, estimated_net_income,
        actual_prior_year_revenue, actual_prior_year_net_income, shares_outstanding, revenue_growth_rate,
        terminal_margin, pe_multiple, current_price, discount_rate, forecast_years, created_at, updated_at
        """;

    public async Task<IReadOnlyList<ValuationModelResponse>> List(
        Guid userId,
        Guid? instrumentId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, $"""
            SELECT {Columns}
            FROM valuation_models
            WHERE user_id = @user_id
              AND (@instrument_id IS NULL OR instrument_id = @instrument_id)
            ORDER BY created_at DESC
            """);
        Add(command, "user_id", userId);
        Add(command, "instrument_id", instrumentId);
        var models = new List<ValuationModelResponse>();
        await using (var reader = await ExecuteReaderAsync(command, cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                models.Add(ReadModel(reader, []));
            }
        }

        for (var index = 0; index < models.Count; index++)
        {
            var forecasts = await ReadForecastsAsync(connection, models[index].Id, cancellationToken);
            models[index] = models[index] with { Forecasts = forecasts };
        }

        return models;
    }

    public async Task<ValuationModelResponse> Create(
        Guid userId,
        CreateValuationModelRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsurePortfolioOwnedAsync(connection, userId, request.PortfolioId, cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var command = Command(connection, """
            INSERT INTO valuation_models
                (user_id, portfolio_id, instrument_id, name, estimated_revenue, estimated_net_income,
                 actual_prior_year_revenue, actual_prior_year_net_income, shares_outstanding,
                 revenue_growth_rate, terminal_margin, pe_multiple, current_price, discount_rate, forecast_years)
            VALUES
                (@user_id, @portfolio_id, @instrument_id, @name, @estimated_revenue, @estimated_net_income,
                 @actual_prior_year_revenue, @actual_prior_year_net_income, @shares_outstanding,
                 @revenue_growth_rate, @terminal_margin, @pe_multiple, @current_price, @discount_rate, @forecast_years)
            RETURNING valuation_id, user_id, portfolio_id, instrument_id, name, estimated_revenue, estimated_net_income,
                      actual_prior_year_revenue, actual_prior_year_net_income, shares_outstanding, revenue_growth_rate,
                      terminal_margin, pe_multiple, current_price, discount_rate, forecast_years, created_at, updated_at
            """);
        command.Transaction = transaction;
        Add(command, "user_id", userId);
        Add(command, "portfolio_id", request.PortfolioId);
        Add(command, "instrument_id", request.InstrumentId);
        Add(command, "name", request.Name);
        Add(command, "estimated_revenue", request.EstimatedRevenue);
        Add(command, "estimated_net_income", request.EstimatedNetIncome);
        Add(command, "actual_prior_year_revenue", request.ActualPriorYearRevenue);
        Add(command, "actual_prior_year_net_income", request.ActualPriorYearNetIncome);
        Add(command, "shares_outstanding", request.SharesOutstanding);
        Add(command, "revenue_growth_rate", request.RevenueGrowthRate);
        Add(command, "terminal_margin", request.TerminalMargin);
        Add(command, "pe_multiple", request.PeMultiple);
        Add(command, "current_price", request.CurrentPrice);
        Add(command, "discount_rate", request.DiscountRate);
        Add(command, "forecast_years", request.ForecastYears);
        ValuationModelResponse model;
        try
        {
            await using var reader = await ExecuteReaderAsync(command, cancellationToken);
            await reader.ReadAsync(cancellationToken);
            model = ReadModel(reader, []);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw ApiException.NotFound("Instrument", request.InstrumentId);
        }

        var forecasts = BuildForecasts(model);
        await InsertForecastsAsync(connection, transaction, model.Id, forecasts, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return model with { Forecasts = forecasts };
    }

    public async Task<ValuationModelResponse> Get(Guid userId, Guid valuationId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        return await GetModelAsync(connection, userId, valuationId, cancellationToken);
    }

    public async Task<ValuationModelResponse> Update(
        Guid userId,
        Guid valuationId,
        UpdateValuationModelRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsureOwnedAsync(connection, "valuation_models", "valuation_id", "Valuation model", userId, valuationId, cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var command = Command(connection, """
            UPDATE valuation_models
            SET name = COALESCE(@name, name),
                estimated_revenue = COALESCE(@estimated_revenue, estimated_revenue),
                estimated_net_income = COALESCE(@estimated_net_income, estimated_net_income),
                actual_prior_year_revenue = COALESCE(@actual_prior_year_revenue, actual_prior_year_revenue),
                actual_prior_year_net_income = COALESCE(@actual_prior_year_net_income, actual_prior_year_net_income),
                shares_outstanding = COALESCE(@shares_outstanding, shares_outstanding),
                revenue_growth_rate = COALESCE(@revenue_growth_rate, revenue_growth_rate),
                terminal_margin = COALESCE(@terminal_margin, terminal_margin),
                pe_multiple = COALESCE(@pe_multiple, pe_multiple),
                current_price = COALESCE(@current_price, current_price),
                discount_rate = COALESCE(@discount_rate, discount_rate),
                forecast_years = COALESCE(@forecast_years, forecast_years),
                updated_at = now()
            WHERE valuation_id = @valuation_id
            RETURNING valuation_id, user_id, portfolio_id, instrument_id, name, estimated_revenue, estimated_net_income,
                      actual_prior_year_revenue, actual_prior_year_net_income, shares_outstanding, revenue_growth_rate,
                      terminal_margin, pe_multiple, current_price, discount_rate, forecast_years, created_at, updated_at
            """);
        command.Transaction = transaction;
        Add(command, "name", request.Name);
        Add(command, "estimated_revenue", request.EstimatedRevenue);
        Add(command, "estimated_net_income", request.EstimatedNetIncome);
        Add(command, "actual_prior_year_revenue", request.ActualPriorYearRevenue);
        Add(command, "actual_prior_year_net_income", request.ActualPriorYearNetIncome);
        Add(command, "shares_outstanding", request.SharesOutstanding);
        Add(command, "revenue_growth_rate", request.RevenueGrowthRate);
        Add(command, "terminal_margin", request.TerminalMargin);
        Add(command, "pe_multiple", request.PeMultiple);
        Add(command, "current_price", request.CurrentPrice);
        Add(command, "discount_rate", request.DiscountRate);
        Add(command, "forecast_years", request.ForecastYears);
        Add(command, "valuation_id", valuationId);
        ValuationModelResponse model;
        await using (var reader = await ExecuteReaderAsync(command, cancellationToken))
        {
            await reader.ReadAsync(cancellationToken);
            model = ReadModel(reader, []);
        }

        var forecasts = BuildForecasts(model);
        await using (var delete = Command(connection, "DELETE FROM valuation_forecasts WHERE valuation_id = @valuation_id"))
        {
            delete.Transaction = transaction;
            Add(delete, "valuation_id", valuationId);
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        await InsertForecastsAsync(connection, transaction, valuationId, forecasts, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return model with { Forecasts = forecasts };
    }

    public async Task Delete(Guid userId, Guid valuationId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsureOwnedAsync(connection, "valuation_models", "valuation_id", "Valuation model", userId, valuationId, cancellationToken);
        await using var command = Command(connection, "DELETE FROM valuation_models WHERE valuation_id = @valuation_id");
        Add(command, "valuation_id", valuationId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ValuationForecastResponse>> ListForecasts(
        Guid userId,
        Guid valuationId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await EnsureOwnedAsync(connection, "valuation_models", "valuation_id", "Valuation model", userId, valuationId, cancellationToken);
        return await ReadForecastsAsync(connection, valuationId, cancellationToken);
    }

    private static async Task<ValuationModelResponse> GetModelAsync(
        NpgsqlConnection connection,
        Guid userId,
        Guid valuationId,
        CancellationToken cancellationToken)
    {
        await EnsureOwnedAsync(connection, "valuation_models", "valuation_id", "Valuation model", userId, valuationId, cancellationToken);
        await using var command = Command(connection, $"SELECT {Columns} FROM valuation_models WHERE valuation_id = @valuation_id");
        Add(command, "valuation_id", valuationId);
        ValuationModelResponse model;
        await using (var reader = await ExecuteReaderAsync(command, cancellationToken))
        {
            await reader.ReadAsync(cancellationToken);
            model = ReadModel(reader, []);
        }

        return model with { Forecasts = await ReadForecastsAsync(connection, valuationId, cancellationToken) };
    }

    private static async Task<IReadOnlyList<ValuationForecastResponse>> ReadForecastsAsync(
        NpgsqlConnection connection,
        Guid valuationId,
        CancellationToken cancellationToken)
    {
        await using var command = Command(connection, """
            SELECT forecast_year, revenue, net_income, eps, target_price
            FROM valuation_forecasts
            WHERE valuation_id = @valuation_id
            ORDER BY forecast_year
            """);
        Add(command, "valuation_id", valuationId);
        await using var reader = await ExecuteReaderAsync(command, cancellationToken);
        var forecasts = new List<ValuationForecastResponse>();
        while (await reader.ReadAsync(cancellationToken))
        {
            forecasts.Add(new ValuationForecastResponse(
                reader.GetInt16(0),
                reader.GetDecimal(1),
                reader.GetDecimal(2),
                reader.GetDecimal(3),
                reader.IsDBNull(4) ? 0m : reader.GetDecimal(4)));
        }

        return forecasts;
    }

    private static async Task InsertForecastsAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid valuationId,
        IReadOnlyList<ValuationForecastResponse> forecasts,
        CancellationToken cancellationToken)
    {
        foreach (var forecast in forecasts)
        {
            await using var command = Command(connection, """
                INSERT INTO valuation_forecasts (valuation_id, forecast_year, revenue, net_income, eps, target_price)
                VALUES (@valuation_id, @forecast_year, @revenue, @net_income, @eps, @target_price)
                """);
            command.Transaction = transaction;
            Add(command, "valuation_id", valuationId);
            Add(command, "forecast_year", forecast.ForecastYear);
            Add(command, "revenue", forecast.Revenue);
            Add(command, "net_income", forecast.NetIncome);
            Add(command, "eps", forecast.Eps);
            Add(command, "target_price", forecast.TargetPrice);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static ValuationModelResponse ReadModel(NpgsqlDataReader reader, IReadOnlyList<ValuationForecastResponse> forecasts)
        => new(
            reader.GetGuid(0),
            reader.GetGuid(1),
            reader.GetGuid(2),
            reader.GetGuid(3),
            reader.GetString(4),
            reader.GetDecimal(5),
            reader.GetDecimal(6),
            reader.IsDBNull(7) ? null : reader.GetDecimal(7),
            reader.IsDBNull(8) ? null : reader.GetDecimal(8),
            reader.GetDecimal(9),
            Convert.ToDouble(reader.GetDecimal(10)),
            Convert.ToDouble(reader.GetDecimal(11)),
            Convert.ToDouble(reader.GetDecimal(12)),
            reader.GetDecimal(13),
            Convert.ToDouble(reader.GetDecimal(14)),
            reader.GetInt16(15),
            forecasts,
            Timestamp(reader, "created_at"),
            Timestamp(reader, "updated_at"));

    private static IReadOnlyList<ValuationForecastResponse> BuildForecasts(ValuationModelResponse model)
    {
        var forecasts = new List<ValuationForecastResponse>(model.ForecastYears);
        var growthFactor = 1 + (decimal)model.RevenueGrowthRate;
        for (var year = 1; year <= model.ForecastYears; year++)
        {
            var compounding = (decimal)Math.Pow((double)growthFactor, year);
            var revenue = model.EstimatedRevenue * compounding;
            var netIncome = model.EstimatedNetIncome * compounding;
            var eps = netIncome / model.SharesOutstanding;
            forecasts.Add(new ValuationForecastResponse(year, revenue, netIncome, eps, eps * (decimal)model.PeMultiple));
        }

        return forecasts;
    }
}
