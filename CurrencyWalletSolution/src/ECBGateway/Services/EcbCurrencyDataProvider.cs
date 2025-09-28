using ECBGateway.Models;
using ECBGateway.Parsers;

namespace ECBGateway.Services;

public class EcbCurrencyDataProvider : ICurrencyDataProvider
{
    private readonly HttpClient _httpClient;
    public string ProviderName => "European Central Bank";

    public EcbCurrencyDataProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CurrencyRateResponse> GetLatestCurrencyRates()
    {
        try
        {
            var response = await _httpClient.GetAsync("https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to get latest Currency Rate, Status Code: {response.StatusCode}");
                return null!;
            }

            var xml = await response.Content.ReadAsStringAsync();

            try
            {
                return CurrencyRateParser.Parse(xml);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to parse response content: {e.Message}");
                return null!;
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"HttpRequestException: {e.Message}");
            return null!;
        }
        catch (TaskCanceledException e)
        {
            Console.WriteLine($"TaskCanceledException: {e.Message}");
            return null!;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected exception: {e.Message}");
            return null!;
        }
    }
}