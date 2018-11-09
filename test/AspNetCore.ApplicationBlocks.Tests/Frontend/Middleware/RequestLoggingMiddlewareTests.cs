using FakeItEasy;
using AspNetCore.ApplicationBlocks.FrontEnd.Middleware;
using SerilogEventLogger;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCore.ApplicationBlocks.Tests.Frontend.Middleware
{
    public class RequestLoggingMiddlewareTests
    {
        [Fact]
        public async Task Connection_Information_Should_Be_Logged()
        {
            var logger = A.Fake<IEventLogger<RequestLoggingMiddleware>>();
            RequestLoggingMiddleware middleware = new RequestLoggingMiddleware(logger);
            var context = A.Fake<HttpContext>();
            var connectionInfo = A.Fake<ConnectionInfo>();
            A.CallTo(() => context.Connection).Returns(connectionInfo);
            A.CallTo(() => connectionInfo.Id).Returns("dummy");
            A.CallTo(() => connectionInfo.LocalIpAddress).Returns(IPAddress.Parse("1.1.1.1"));
            A.CallTo(() => connectionInfo.LocalPort).Returns(1234);
            A.CallTo(() => connectionInfo.RemoteIpAddress).Returns(IPAddress.Parse("2.2.2.2"));
            A.CallTo(() => connectionInfo.RemotePort).Returns(4321);

            await middleware.Invoke(context, () => Task.CompletedTask);

            A.CallTo(logger)
                .Where(call => call.Method.Name == "BeginScope")
                .WhenArgumentsMatch(args =>
                {
                    dynamic data = args[0];
                    var connection = data.Connection;
                    return connection.Id == "dummy" &&
                        connection.LocalIpAddress.ToString() == "1.1.1.1" &&
                        connection.LocalPort == 1234 &&
                        connection.RemoteIpAddress.ToString() == "2.2.2.2" &&
                        connection.RemotePort == 4321;
                }).MustHaveHappened();
        }
    }
}
