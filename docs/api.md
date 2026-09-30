# Arcana Ledger API

This document describes the HTTP API exposed by `ArcanaLedger.Server`. It is organized around the entities defined in [erd.md](./erd.md) and is the contract the React client (`ArcanaLedger.Client`) should be built against.

## Conventions

- **Base path**: all endpoints are mounted under `/api`.
- **Format**: JSON request and response bodies, `camelCase` property names, UTF-8.
- **Authentication**: bearer JWT in the `Authorization: Bearer <token>` header, issued by `POST /api/auth/login`. Endpoints under a user/portfolio scope always resolve the owner from the token; there is no cross-user access.
- **IDs**: all resource identifiers are `uuid` strings (matches the `uuid` primary keys in the ERD).
- **Money and quantities**: fields backed by `numeric(20, 8)` columns (prices, quantities, cash amounts) are serialized as JSON strings, not floats, to avoid precision loss (e.g. `"entryPrice": "182.50000000"`). Percentages and rates backed by `numeric` are serialized as numbers.
- **Timestamps**: `timestamptz` columns are serialized as ISO-8601 UTC strings (`"2026-09-29T14:03:00Z"`). `date` columns are serialized as `YYYY-MM-DD`.
- **Pagination**: list endpoints accept `?page=1&pageSize=25` (defaults shown) and return:
  ```json
  {
    "items": [ /* ... */ ],
    "page": 1,
    "pageSize": 25,
    "totalCount": 132
  }
  ```
- **Errors**: failures use RFC 9457 Problem Details (`application/problem+json`), matching `AddProblemDetails()` in `Program.cs`:
  ```json
  {
    "type": "https://arcanaledger.dev/errors/validation",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": { "email": ["Email is already registered."] }
  }
  ```
- **Common status codes**: `200` (read/update success), `201` (created, with `Location` header), `204` (deleted/no content), `400` (validation), `401` (missing/invalid token), `403` (authenticated but not the resource owner), `404` (not found), `409` (conflict, e.g. duplicate default portfolio).

## Authentication

Maps to `app_users`.

| Method | Path | Description |
| --- | --- | --- |
| POST | `/api/auth/register` | Create an account. |
| POST | `/api/auth/login` | Exchange credentials for a JWT. |
| POST | `/api/auth/logout` | Revoke the current refresh token/session. |
| GET | `/api/auth/me` | Return the authenticated user. |

#### `POST /api/auth/register`

Request

```json
{
  "email": "trader@example.com",
  "displayName": "Ada Trader",
  "password": "Str0ngPassword!"
}
```

Response `201`

```json
{
  "id": "b3f2b6b0-1e0a-4e9a-9a0e-2a2f7a2f9a10",
  "email": "trader@example.com",
  "displayName": "Ada Trader",
  "createdAt": "2026-09-29T14:03:00Z"
}
```

#### `POST /api/auth/login`

Request

```json
{
  "email": "trader@example.com",
  "password": "Str0ngPassword!"
}
```

Response `200`

```json
{
  "accessToken": "eyJhbGciOi...",
  "expiresAt": "2026-09-29T15:03:00Z",
  "user": {
    "id": "b3f2b6b0-1e0a-4e9a-9a0e-2a2f7a2f9a10",
    "email": "trader@example.com",
    "displayName": "Ada Trader"
  }
}
```

#### `GET /api/auth/me`

Response `200`

```json
{
  "id": "b3f2b6b0-1e0a-4e9a-9a0e-2a2f7a2f9a10",
  "email": "trader@example.com",
  "displayName": "Ada Trader",
  "createdAt": "2026-09-29T14:03:00Z",
  "updatedAt": "2026-09-29T14:03:00Z"
}
```

## Portfolios

Maps to `portfolios`.

| Method | Path | Description |
| --- | --- | --- |
| GET | `/api/portfolios` | List the caller's portfolios. |
| POST | `/api/portfolios` | Create a portfolio. |
| GET | `/api/portfolios/{portfolioId}` | Get a portfolio. |
| PATCH | `/api/portfolios/{portfolioId}` | Update name, base currency, or default flag. |
| DELETE | `/api/portfolios/{portfolioId}` | Delete a portfolio (cascades accounts, trades, etc.). |

#### `POST /api/portfolios`

Request

```json
{
  "name": "Growth",
  "baseCurrency": "CAD",
  "isDefault": true
}
```

Response `201`

```json
{
  "id": "1e3a3b3e-2f2f-4a9a-8a9a-1a2b3c4d5e6f",
  "userId": "b3f2b6b0-1e0a-4e9a-9a0e-2a2f7a2f9a10",
  "name": "Growth",
  "baseCurrency": "CAD",
  "isDefault": true,
  "createdAt": "2026-09-29T14:03:00Z",
  "updatedAt": "2026-09-29T14:03:00Z"
}
```

#### `GET /api/portfolios/{portfolioId}`

Response `200` — same shape as the create response above.

#### `PATCH /api/portfolios/{portfolioId}`

Request (partial)

```json
{
  "name": "Growth (Aggressive)",
  "isDefault": true
}
```

Response `200` — updated portfolio resource.

## Brokerage accounts

Maps to `brokerage_accounts`, nested under a portfolio.

| Method | Path | Description |
| --- | --- | --- |
| GET | `/api/portfolios/{portfolioId}/accounts` | List accounts in a portfolio. |
| POST | `/api/portfolios/{portfolioId}/accounts` | Add an account. |
| PATCH | `/api/portfolios/{portfolioId}/accounts/{accountId}` | Rename, change currency, or activate/deactivate. |
| DELETE | `/api/portfolios/{portfolioId}/accounts/{accountId}` | Remove an account. |

#### `POST /api/portfolios/{portfolioId}/accounts`

Request

```json
{
  "name": "Questrade Margin",
  "brokerName": "Questrade",
  "currency": "CAD"
}
```

Response `201`

```json
{
  "id": "2b1c6a2e-9f9f-4c3c-8b9b-3c4d5e6f7a8b",
  "portfolioId": "1e3a3b3e-2f2f-4a9a-8a9a-1a2b3c4d5e6f",
  "name": "Questrade Margin",
  "brokerName": "Questrade",
  "currency": "CAD",
  "isActive": true,
  "createdAt": "2026-09-29T14:03:00Z"
}
```

## Instruments and quotes

Maps to `instruments` and `instrument_quotes`. Read-only from the client; instruments and quotes are maintained by a market-data sync job.

| Method | Path | Description |
| --- | --- | --- |
| GET | `/api/instruments?search=AAPL` | Search instruments by symbol/name. |
| GET | `/api/instruments/{instrumentId}` | Get an instrument, including its latest quote. |

#### `GET /api/instruments/{instrumentId}`

Response `200`

```json
{
  "id": "6d5e4f3a-2b1c-4a9a-8a9a-9a8b7c6d5e4f",
  "symbol": "AAPL",
  "name": "Apple Inc.",
  "sector": "Technology",
  "assetType": "equity",
  "currency": "USD",
  "isActive": true,
  "quote": {
    "lastPrice": "182.50000000",
    "previousClose": "180.10000000",
    "changeAmount": "2.40000000",
    "changePercent": "1.33259300",
    "tone": "positive",
    "quotedAt": "2026-09-29T13:59:00Z"
  }
}
```

## Watchlist

Maps to `watchlist_items`, nested under a portfolio. Backs the dashboard watchlist widget.

| Method | Path | Description |
| --- | --- | --- |
| GET | `/api/portfolios/{portfolioId}/watchlist` | List watchlist items in display order. |
| POST | `/api/portfolios/{portfolioId}/watchlist` | Add an instrument to the watchlist. |
| PATCH | `/api/portfolios/{portfolioId}/watchlist/{instrumentId}` | Reorder an item (`displayOrder`). |
| DELETE | `/api/portfolios/{portfolioId}/watchlist/{instrumentId}` | Remove an item. |

#### `POST /api/portfolios/{portfolioId}/watchlist`

Request

```json
{
  "instrumentId": "6d5e4f3a-2b1c-4a9a-8a9a-9a8b7c6d5e4f",
  "displayOrder": 0
}
```

Response `201`

```json
{
  "portfolioId": "1e3a3b3e-2f2f-4a9a-8a9a-1a2b3c4d5e6f",
  "instrument": {
    "id": "6d5e4f3a-2b1c-4a9a-8a9a-9a8b7c6d5e4f",
    "symbol": "AAPL",
    "name": "Apple Inc.",
    "sector": "Technology"
  },
  "quote": {
    "lastPrice": "182.50000000",
    "changeAmount": "2.40000000",
    "changePercent": "1.33259300",
    "tone": "positive"
  },
  "displayOrder": 0,
  "addedAt": "2026-09-29T14:03:00Z"
}
```

## Trade setups

Maps to `trade_setups`. Backs the setup picker on the Trades page and the "when to use" guidance on Configuration.

| Method | Path | Description |
| --- | --- | --- |
| GET | `/api/trade-setups` | List the caller's setups. |
| POST | `/api/trade-setups` | Create a setup. |
| GET | `/api/trade-setups/{setupId}` | Get a setup. |
| PATCH | `/api/trade-setups/{setupId}` | Update a setup. |
| DELETE | `/api/trade-setups/{setupId}` | Delete a setup (trades keep history via `ON DELETE SET NULL`). |

#### `POST /api/trade-setups`

Request

```json
{
  "code": "BOOMER",
  "name": "Boomer",
  "family": "position",
  "entryRule": "Breakout above 20-day high with volume confirmation.",
  "exitRule": "Close below 10-day EMA.",
  "trend": "up",
  "usesTranches": true,
  "trancheCount": 3,
  "varMultiplier": 1.5,
  "usageNotes": "Use for high-conviction, longer-duration positions."
}
```

Response `201` — echoes the request plus `id`, `userId`, `createdAt`, `updatedAt`.

## Trades and executions

Maps to `trades` and `trade_executions`. `trades` are the logical position shown on the Trades page; `trade_executions` are the individual fills/tranches.

| Method | Path | Description |
| --- | --- | --- |
| GET | `/api/trades?portfolioId=&status=&symbol=` | List trades with filters, includes cached P&L. |
| POST | `/api/trades` | Open a new trade (planned or open). |
| GET | `/api/trades/{tradeId}` | Get a trade, including its executions. |
| PATCH | `/api/trades/{tradeId}` | Update targets, status, or notes. |
| DELETE | `/api/trades/{tradeId}` | Delete a trade (only if it has no executions). |
| GET | `/api/trades/{tradeId}/executions` | List executions/fills for a trade. |
| POST | `/api/trades/{tradeId}/executions` | Record a fill; recalculates cached P&L. |

#### `POST /api/trades`

Request

```json
{
  "accountId": "2b1c6a2e-9f9f-4c3c-8b9b-3c4d5e6f7a8b",
  "instrumentId": "6d5e4f3a-2b1c-4a9a-8a9a-9a8b7c6d5e4f",
  "setupId": "4f5e6d7c-8b9a-4a1a-9b8b-7c6d5e4f3a2b",
  "orderType": "long",
  "openedOn": "2026-09-29",
  "entryTarget": "180.00000000",
  "cutLoss": "172.00000000",
  "targetPrice": "205.00000000",
  "notes": "Breakout setup off the August base."
}
```

Response `201`

```json
{
  "id": "9a8b7c6d-5e4f-4a1a-9b8b-3a2b1c6a2e9f",
  "portfolioId": "1e3a3b3e-2f2f-4a9a-8a9a-1a2b3c4d5e6f",
  "accountId": "2b1c6a2e-9f9f-4c3c-8b9b-3c4d5e6f7a8b",
  "instrument": { "id": "6d5e4f3a-2b1c-4a9a-8a9a-9a8b7c6d5e4f", "symbol": "AAPL" },
  "setup": { "id": "4f5e6d7c-8b9a-4a1a-9b8b-7c6d5e4f3a2b", "code": "BOOMER", "name": "Boomer" },
  "orderType": "long",
  "status": "planned",
  "openedOn": "2026-09-29",
  "closedOn": null,
  "entryTarget": "180.00000000",
  "cutLoss": "172.00000000",
  "targetPrice": "205.00000000",
  "unrealizedPnlAmount": null,
  "unrealizedPnlPercent": null,
  "positionResult": "neutral",
  "notes": "Breakout setup off the August base.",
  "createdAt": "2026-09-29T14:03:00Z",
  "updatedAt": "2026-09-29T14:03:00Z"
}
```

#### `GET /api/trades?portfolioId=...` (list item shape, matches the Trades table)

```json
{
  "items": [
    {
      "id": "9a8b7c6d-5e4f-4a1a-9b8b-3a2b1c6a2e9f",
      "symbol": "AAPL",
      "setup": "Boomer",
      "shares": "50.00000000",
      "date": "2026-09-29",
      "entryPrice": "180.00000000",
      "cutLoss": "172.00000000",
      "targetPrice": "205.00000000",
      "result": "up",
      "quote": "182.50000000",
      "change": "1.33"
    }
  ],
  "page": 1,
  "pageSize": 25,
  "totalCount": 1
}
```

#### `POST /api/trades/{tradeId}/executions`

Request

```json
{
  "side": "buy",
  "quantity": "50.00000000",
  "price": "181.20000000",
  "commission": "1.99000000",
  "executedAt": "2026-09-29T14:31:00Z"
}
```

Response `201`

```json
{
  "id": "c6d5e4f3-a2b1-4c6a-8e9f-9a8b7c6d5e4f",
  "tradeId": "9a8b7c6d-5e4f-4a1a-9b8b-3a2b1c6a2e9f",
  "side": "buy",
  "quantity": "50.00000000",
  "price": "181.20000000",
  "commission": "1.99000000",
  "executedAt": "2026-09-29T14:31:00Z"
}
```

## Portfolio transactions

Maps to `portfolio_transactions` — the full cash/position ledger.

| Method | Path | Description |
| --- | --- | --- |
| GET | `/api/portfolios/{portfolioId}/transactions?type=&from=&to=` | List ledger entries. |
| POST | `/api/portfolios/{portfolioId}/transactions` | Record a manual entry (deposit, withdrawal, fee, adjustment). |

#### `POST /api/portfolios/{portfolioId}/transactions`

Request

```json
{
  "accountId": "2b1c6a2e-9f9f-4c3c-8b9b-3c4d5e6f7a8b",
  "transactionType": "deposit",
  "cashAmount": "5000.00000000",
  "currency": "CAD",
  "occurredAt": "2026-09-29T14:00:00Z",
  "memo": "Initial funding"
}
```

Response `201` — echoes the request plus `id`, `portfolioId`, `instrumentId` (nullable), `tradeId` (nullable), `quantity`, `unitPrice`.

## Portfolio snapshots

Maps to `portfolio_snapshots`. Powers the dashboard performance chart; always read from this cache, never recomputed from raw transactions per request.

| Method | Path | Description |
| --- | --- | --- |
| GET | `/api/portfolios/{portfolioId}/snapshots?from=&to=` | List daily snapshots for charting. |

Response `200`

```json
{
  "items": [
    {
      "asOfDate": "2026-09-29",
      "totalValue": "128450.32000000",
      "cashValue": "12450.00000000",
      "investedValue": "116000.32000000",
      "dailyReturn": "0.00842100",
      "timeWeightedReturn": "0.08213400"
    }
  ]
}
```

## Portfolio activity

Maps to `portfolio_activity`. Powers the dashboard "recent activity" feed.

| Method | Path | Description |
| --- | --- | --- |
| GET | `/api/portfolios/{portfolioId}/activity?limit=10` | List recent activity, newest first. |

Response `200`

```json
{
  "items": [
    {
      "id": "7c6d5e4f-3a2b-4c6a-8e9f-1e3a3b3e2f2f",
      "category": "trade",
      "title": "Opened AAPL long",
      "detail": "50 shares at $181.20 using the Boomer setup.",
      "occurredAt": "2026-09-29T14:31:00Z"
    }
  ]
}
```

## Dashboard summary

Convenience aggregation endpoint for the Dashboard page, combining the latest snapshot, watchlist, and activity so the client does not have to make four round trips.

| Method | Path | Description |
| --- | --- | --- |
| GET | `/api/portfolios/{portfolioId}/dashboard` | Get the dashboard summary. |

Response `200`

```json
{
  "portfolio": { "id": "1e3a3b3e-2f2f-4a9a-8a9a-1a2b3c4d5e6f", "name": "Growth", "baseCurrency": "CAD" },
  "latestSnapshot": {
    "asOfDate": "2026-09-29",
    "totalValue": "128450.32000000",
    "cashValue": "12450.00000000",
    "investedValue": "116000.32000000",
    "dailyReturn": "0.00842100"
  },
  "watchlist": [
    { "symbol": "AAPL", "name": "Apple Inc.", "sector": "Technology", "price": "182.50000000", "change": "1.33", "tone": "positive" }
  ],
  "activity": [
    { "title": "Opened AAPL long", "detail": "50 shares at $181.20 using the Boomer setup.", "time": "2026-09-29T14:31:00Z" }
  ]
}
```

## Valuation models and forecasts

Maps to `valuation_models` and `valuation_forecasts`. Backs the Valuation page's DCF-style inputs and per-year projections.

| Method | Path | Description |
| --- | --- | --- |
| GET | `/api/valuations?instrumentId=` | List the caller's valuation models. |
| POST | `/api/valuations` | Create a valuation model (also generates its forecasts). |
| GET | `/api/valuations/{valuationId}` | Get a valuation model, including forecasts. |
| PATCH | `/api/valuations/{valuationId}` | Update inputs (recalculates forecasts). |
| DELETE | `/api/valuations/{valuationId}` | Delete a valuation model. |
| GET | `/api/valuations/{valuationId}/forecasts` | List forecast years. |

#### `POST /api/valuations`

Request

```json
{
  "portfolioId": "1e3a3b3e-2f2f-4a9a-8a9a-1a2b3c4d5e6f",
  "instrumentId": "6d5e4f3a-2b1c-4a9a-8a9a-9a8b7c6d5e4f",
  "name": "AAPL base case",
  "estimatedRevenue": 400000000000,
  "estimatedNetIncome": 100000000000,
  "actualPriorYearRevenue": 383000000000,
  "actualPriorYearNetIncome": 97000000000,
  "sharesOutstanding": 15200000000,
  "revenueGrowthRate": 0.08,
  "terminalMargin": 0.25,
  "peMultiple": 28,
  "currentPrice": "182.50000000",
  "discountRate": 0.08,
  "forecastYears": 5
}
```

Response `201`

```json
{
  "id": "5e4f3a2b-1c6a-4c6a-8e9f-6d5e4f3a2b1c",
  "userId": "b3f2b6b0-1e0a-4e9a-9a0e-2a2f7a2f9a10",
  "portfolioId": "1e3a3b3e-2f2f-4a9a-8a9a-1a2b3c4d5e6f",
  "instrumentId": "6d5e4f3a-2b1c-4a9a-8a9a-9a8b7c6d5e4f",
  "name": "AAPL base case",
  "estimatedRevenue": 400000000000,
  "estimatedNetIncome": 100000000000,
  "actualPriorYearRevenue": 383000000000,
  "actualPriorYearNetIncome": 97000000000,
  "sharesOutstanding": 15200000000,
  "revenueGrowthRate": 0.08,
  "terminalMargin": 0.25,
  "peMultiple": 28,
  "currentPrice": "182.50000000",
  "discountRate": 0.08,
  "forecastYears": 5,
  "forecasts": [
    { "forecastYear": 1, "revenue": 432000000000, "netIncome": 108000000000, "eps": "7.10500000", "targetPrice": "198.90000000" }
  ],
  "createdAt": "2026-09-29T14:03:00Z",
  "updatedAt": "2026-09-29T14:03:00Z"
}
```

## Portfolio settings

Maps to `portfolio_settings`. Backs the Configuration page (risk, reserve, and per-setup position sizing defaults). One row per portfolio.

| Method | Path | Description |
| --- | --- | --- |
| GET | `/api/portfolios/{portfolioId}/settings` | Get the portfolio's configuration. |
| PUT | `/api/portfolios/{portfolioId}/settings` | Replace the portfolio's configuration (upsert). |

#### `PUT /api/portfolios/{portfolioId}/settings`

Request

```json
{
  "capital": "100000.00000000",
  "varRate": 0.02,
  "cashValue": "15000.00000000",
  "reserveValue": "10000.00000000",
  "positionEntryPrice": "180.00000000",
  "positionExitPrice": "172.00000000",
  "positionShares": "50.00000000",
  "positionCashValue": "9000.00000000",
  "trancheEntryPrice": "180.00000000",
  "trancheExitPrice": "172.00000000",
  "trancheShares": "17.00000000",
  "trancheCashValue": "3060.00000000",
  "swingEntryPrice": "180.00000000",
  "swingExitPrice": "172.00000000",
  "swingShares": "25.00000000",
  "swingCashValue": "4500.00000000",
  "breakoutPrice": "185.00000000",
  "patternLowPrice": "170.00000000",
  "swingTargetPrice": "205.00000000",
  "momentumRate": 0.015
}
```

Response `200` — echoes the request plus the generated columns:

```json
{
  "portfolioId": "1e3a3b3e-2f2f-4a9a-8a9a-1a2b3c4d5e6f",
  "capital": "100000.00000000",
  "varRate": 0.02,
  "cashValue": "15000.00000000",
  "cashAllocationPercent": 15.0,
  "reserveValue": "10000.00000000",
  "reserveAllocationPercent": 10.0,
  "updatedAt": "2026-09-29T14:03:00Z"
}
```
