using Microsoft.Extensions.Options;
using Shared.Options;

namespace Shared.Validators;

public class IpRateLimitingValidator : IValidateOptions<IpRateLimitingOptions>
{
    public ValidateOptionsResult Validate(string? name, IpRateLimitingOptions options)
    {
        var failures = new List<string>();


        if (options.HttpStatusCode < 400 || options.HttpStatusCode > 599)
        {
            failures.Add("HttpStatusCode must be between 400 and 599");
        }

        if (options.GeneralRules?.Any() != true)
            return failures.Count > 0
                ? ValidateOptionsResult.Fail(failures)
                : ValidateOptionsResult.Success;
        
        for (var i = 0; i < options.GeneralRules.Count; i++)
        {
            var rule = options.GeneralRules[i];
            var rulePrefix = $"GeneralRules[{i}]";

            if (string.IsNullOrWhiteSpace(rule.Endpoint))
            {
                failures.Add($"{rulePrefix}.Endpoint cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(rule.Period))
            {
                failures.Add($"{rulePrefix}.Period cannot be empty");
            }

            if (rule.Limit <= 0)
            {
                failures.Add($"{rulePrefix}.Limit must be greater than 0");
            }
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}