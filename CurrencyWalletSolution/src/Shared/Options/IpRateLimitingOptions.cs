namespace Shared.Options;

public class IpRateLimitingOptions
{
    public const string SectionName = "IpRateLimiting";

    public bool EnableEndpointRateLimiting { get; set; } = false;
    public bool StackBlockedRequests { get; set; } = false;
    public int HttpStatusCode { get; set; } = 429;
    public List<RateLimitRule> GeneralRules { get; set; } = new();
    public List<string> ClientWhitelist { get; set; } = new();
}

public class RateLimitRule
{
    public string Endpoint { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public int Limit { get; set; }
}