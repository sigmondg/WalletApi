using Microsoft.Extensions.Options;
using Shared.Options;

namespace Shared.Validators;

public class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
{
    public ValidateOptionsResult Validate(string? name, DatabaseOptions options) 
        => string.IsNullOrEmpty(options.ConnectionString) ? ValidateOptionsResult.Fail("Connection string is empty") : ValidateOptionsResult.Success;
}