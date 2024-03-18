using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace TrackerSolution.IntegrationTests.PixelService.Mocks
{
    public class CustomStartupFilter : IStartupFilter
    {
        private string fakeIpAddress;

        public CustomStartupFilter(string fakeIpAddress)
        {
            this.fakeIpAddress = fakeIpAddress;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseMiddleware<FakeRemoteIpAddressMiddleware>(fakeIpAddress);
                next(app);
            };
        }
    }
}