using Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using PixelService;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using TrackerSolution.IntegrationTests.PixelService.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TrackerSolution.IntegrationTests.PixelService
{
    [TestClass]
    public class PixelServiceApiTests
    {
        const string trackingPixel = "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7";
        private const string expectedReferrer = "PixelServiceTests";
        private string expectedUserAgent = "testHttpClient";
        private string expectedIp = "127.168.1.16";

        private Mock<IMessageSender<VisitMessage>> messageSenderMock;
        private WebApplicationFactory<Program> factory;
        private HttpClient httpClient;

        [TestInitialize]
        public void InitializeTest()
        {
            messageSenderMock = new Mock<IMessageSender<VisitMessage>>();
            factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder => builder
                    .ConfigureServices(services =>
                    {
                        services.AddScoped(services => messageSenderMock.Object);
                        services.AddSingleton<IStartupFilter>(services => new CustomStartupFilter(expectedIp));
                    }));

            httpClient = factory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", expectedUserAgent);
            httpClient.DefaultRequestHeaders.Add("Referer", expectedReferrer);
        }

        [TestCleanup]
        public void CleanupTest()
        {
            httpClient?.Dispose();
            factory?.Dispose();
        }


        [TestMethod]
        public async Task GetTrackingPixelTest_TransparentGifIsReceived_VisitMessageIsSentWithCorrectData()
        {
            // Act
            var response = await httpClient.GetAsync("/track");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("image/gif", response.Content.Headers.ContentType.MediaType);

            // Check if correct gif is returned
            var gifByteArray = await response.Content.ReadAsByteArrayAsync();
            var gifString = Convert.ToBase64String(gifByteArray);
            Assert.AreEqual(trackingPixel, gifString);

            // Check if the message is sent with the correct info
            messageSenderMock.Verify(c => c.SendMessage(
                It.Is<VisitMessage>(vm => vm.Ip == expectedIp &&
                vm.Referrer == expectedReferrer &&
                vm.UserAgent == expectedUserAgent)),
                Times.Once);
        }
    }
}