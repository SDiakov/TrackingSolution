using System.Net;
using Microsoft.AspNetCore.Http;

namespace TrackerSolution.IntegrationTests.PixelService.Mocks
{
    public class FakeRemoteIpAddressMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IPAddress fakeIpAddress;

        public FakeRemoteIpAddressMiddleware(RequestDelegate next, string fakeIpAddress)
        {
            this.next = next;
            this.fakeIpAddress = IPAddress.Parse(fakeIpAddress); ;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Connection.RemoteIpAddress = fakeIpAddress;

            await next(httpContext);
        }
    }
}