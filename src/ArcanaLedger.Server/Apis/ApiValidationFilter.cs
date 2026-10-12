using System.ComponentModel.DataAnnotations;
using ArcanaLedger.Server.Services;

namespace ArcanaLedger.Server.Apis;

/// <summary>
/// Validates <c>ArcanaLedger.Server.Dtos</c> request bodies using data annotations before the
/// endpoint handler runs. Applied once, at the root <c>/api/v1</c> route group, so endpoint
/// handlers do not need a second validation mechanism.
/// </summary>
public sealed class ApiValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        Dictionary<string, string[]>? errors = null;

        foreach (var argument in context.Arguments)
        {
            if (argument is null || argument.GetType().Namespace is not { } ns || !ns.StartsWith("ArcanaLedger.Server.Dtos", StringComparison.Ordinal))
            {
                continue;
            }

            var validationContext = new ValidationContext(argument);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(argument, validationContext, results, validateAllProperties: true);

            foreach (var result in results)
            {
                var memberNames = result.MemberNames.Any() ? result.MemberNames : [string.Empty];
                foreach (var memberName in memberNames)
                {
                    errors ??= [];
                    var key = ToCamelCase(memberName);
                    var message = result.ErrorMessage ?? "Invalid value.";
                    errors[key] = errors.TryGetValue(key, out var existing) ? [.. existing, message] : [message];
                }
            }
        }

        if (errors is not null)
        {
            throw ApiException.Validation(errors);
        }

        return await next(context);
    }

    private static string ToCamelCase(string name)
        => string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];
}
