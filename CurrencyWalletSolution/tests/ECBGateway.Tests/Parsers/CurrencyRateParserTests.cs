using ECBGateway.Parsers;

namespace ECBGateway.Tests.Parsers;

public class CurrencyRateParserTests
{
    [Fact]
    public void Parse_ValidXml_ReturnsCorrectCurrencyRateResponse()
    {
        var xml = @"
        <gesmes:Envelope xmlns:gesmes='http://www.gesmes.org/xml/2002-08-01' 
                         xmlns='http://www.ecb.int/vocabulary/2002-08-01/eurofxref'>
            <Cube>
                <Cube time='2025-09-25'>
                    <Cube currency='USD' rate='1.10'/>
                    <Cube currency='GBP' rate='0.85'/>
                </Cube>
            </Cube>
        </gesmes:Envelope>";

      
        var result = CurrencyRateParser.Parse(xml);
        
        Assert.Equal(new DateTime(2025, 9, 25), result.Time);
        Assert.Equal(2, result.Rates.Count);
        Assert.Equal(1.10m, result.Rates["USD"]);
        Assert.Equal(0.85m, result.Rates["GBP"]);
    }

    [Fact]
    public void Parse_NoTimeCube_ThrowsInvalidOperationException()
    {
        var xml = @"
        <gesmes:Envelope xmlns:gesmes='http://www.gesmes.org/xml/2002-08-01' 
                         xmlns='http://www.ecb.int/vocabulary/2002-08-01/eurofxref'>
            <Cube>
                <Cube>
                    <Cube currency='USD' rate='1.10'/>
                </Cube>
            </Cube>
        </gesmes:Envelope>";
        
        Assert.Throws<InvalidOperationException>(() => CurrencyRateParser.Parse(xml));
    }
}