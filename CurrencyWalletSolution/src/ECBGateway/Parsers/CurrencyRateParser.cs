using System.Globalization;
using System.Xml.Linq;
using ECBGateway.Models;

namespace ECBGateway.Parsers;

public static class CurrencyRateParser
{
    public static CurrencyRateResponse Parse(string xml)
    {
        var doc = XDocument.Parse(xml);

        XNamespace ns = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref";

        var timeCube = doc.Descendants(ns + "Cube")
            .FirstOrDefault(e => e.Attribute("time") != null);

        if (timeCube == null)
            throw new InvalidOperationException("No time attribute found in ECB XML.");

        var date = DateTime.Parse(timeCube.Attribute("time")!.Value, CultureInfo.InvariantCulture);

        var rates = timeCube.Elements(ns + "Cube")
            .ToDictionary(
                e => e.Attribute("currency")!.Value,
                e => decimal.Parse(e.Attribute("rate")!.Value, CultureInfo.InvariantCulture)
            );

        return new CurrencyRateResponse
        { Time = date,
          Rates = rates };
    }
}