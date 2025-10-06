using Microsoft.Extensions.Options;
using Shared.Options;

namespace Shared.Validators;

public class KafkaOptionsValidator : IValidateOptions<KafkaOptions>
{
    public ValidateOptionsResult Validate(string? name, KafkaOptions options) => ValidateOptionsResult.Success;
}