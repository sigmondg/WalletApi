using System.Net;
using System.Text;
using Moq;
using Moq.Protected;

namespace ECBGateway.Tests.Helpers;

public static class HttpClientTestHelper
{
    public static HttpClient CreateMockHttpClient(string xmlResponse, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            { StatusCode = statusCode,
              Content = new StringContent(xmlResponse, Encoding.UTF8, "application/xml") });

        return new HttpClient(handler.Object);
    }
}