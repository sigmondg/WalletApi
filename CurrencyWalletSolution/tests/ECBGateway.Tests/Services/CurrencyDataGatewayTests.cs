using ECBGateway.Services;
using ECBGateway.Tests.Helpers;

namespace ECBGateway.Tests.Services;

public class CurrencyDataGatewayTests
{
    [Fact]
    public async Task GetLatestCurrencyRate_ReturnExpectedRates_ValidXMLResponse()
    {
        var xmlResponse = @"<gesmes:Envelope xmlns:gesmes=""http://www.gesmes.org/xml/2002-08-01"" xmlns=""http://www.ecb.int/vocabulary/2002-08-01/eurofxref"">
<gesmes:subject>Reference rates</gesmes:subject>
<gesmes:Sender>
<gesmes:name>European Central Bank</gesmes:name>
</gesmes:Sender>
<Cube>
<Cube time=""2025-09-24"">
<Cube currency=""USD"" rate=""1.1756""/>
<Cube currency=""JPY"" rate=""174.51""/>
<Cube currency=""BGN"" rate=""1.9558""/>
<Cube currency=""CZK"" rate=""24.290""/>
<Cube currency=""DKK"" rate=""7.4639""/>
<Cube currency=""GBP"" rate=""0.87310""/>
<Cube currency=""HUF"" rate=""391.08""/>
<Cube currency=""PLN"" rate=""4.2673""/>
<Cube currency=""RON"" rate=""5.0759""/>
<Cube currency=""SEK"" rate=""11.0350""/>
<Cube currency=""CHF"" rate=""0.9335""/>
<Cube currency=""ISK"" rate=""142.40""/>
<Cube currency=""NOK"" rate=""11.6880""/>
<Cube currency=""TRY"" rate=""48.7289""/>
<Cube currency=""AUD"" rate=""1.7791""/>
<Cube currency=""BRL"" rate=""6.2240""/>
<Cube currency=""CAD"" rate=""1.6302""/>
<Cube currency=""CNY"" rate=""8.3786""/>
<Cube currency=""HKD"" rate=""9.1422""/>
<Cube currency=""IDR"" rate=""19630.64""/>
<Cube currency=""ILS"" rate=""3.9373""/>
<Cube currency=""INR"" rate=""104.2845""/>
<Cube currency=""KRW"" rate=""1644.99""/>
<Cube currency=""MXN"" rate=""21.6342""/>
<Cube currency=""MYR"" rate=""4.9493""/>
<Cube currency=""NZD"" rate=""2.0139""/>
<Cube currency=""PHP"" rate=""67.568""/>
<Cube currency=""SGD"" rate=""1.5128""/>
<Cube currency=""THB"" rate=""37.649""/>
<Cube currency=""ZAR"" rate=""20.3339""/>
</Cube>
</Cube>
</gesmes:Envelope>";

        var httpClient = HttpClientTestHelper.CreateMockHttpClient(xmlResponse);
        var gateway = new EcbCurrencyDataProvider(httpClient);
        var result = await gateway.GetLatestCurrencyRates();

        Assert.Equal(new DateTime(2025, 9, 24), result.Time);
        Assert.Equal(1.1756m, result.Rates["USD"]);
        Assert.Equal(174.51m, result.Rates["JPY"]);
        Assert.Equal(1.9558m, result.Rates["BGN"]);
        Assert.Equal(24.290m, result.Rates["CZK"]);
        Assert.Equal(7.4639m, result.Rates["DKK"]);
        Assert.Equal(0.87310m, result.Rates["GBP"]);
        Assert.Equal(391.08m, result.Rates["HUF"]);
        Assert.Equal(4.2673m, result.Rates["PLN"]);
        Assert.Equal(5.0759m, result.Rates["RON"]);
        Assert.Equal(11.0350m, result.Rates["SEK"]);
        Assert.Equal(0.9335m, result.Rates["CHF"]);
        Assert.Equal(142.40m, result.Rates["ISK"]);
        Assert.Equal(11.6880m, result.Rates["NOK"]);
        Assert.Equal(48.7289m, result.Rates["TRY"]);
        Assert.Equal(1.7791m, result.Rates["AUD"]);
        Assert.Equal(6.2240m, result.Rates["BRL"]);
        Assert.Equal(1.6302m, result.Rates["CAD"]);
        Assert.Equal(8.3786m, result.Rates["CNY"]);
        Assert.Equal(37.649m, result.Rates["THB"]);
        Assert.Equal(20.3339m, result.Rates["ZAR"]);
    }
}