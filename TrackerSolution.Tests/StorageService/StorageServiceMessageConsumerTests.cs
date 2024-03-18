using Common;
using System.Globalization;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using StorageService;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TrackerSolution.IntegrationTests.StorageService
{
    [TestClass]
    public class StorageServiceMessageConsumerTests
    {
        const string hostName = "localhost";

        [TestMethod]
        public async Task ConsumeMessageTest_VisitMessageIsConsumed()
        {
            // Arrange
            var queueName = "testVisitQueue";
            var mockMessageProcessor = new Mock<IMessageProcessor<VisitMessage>>();

            var messageConsumer = new RabbitMqMessageConsumer<VisitMessage>(hostName, queueName, mockMessageProcessor.Object);

            var expectedMessage = new VisitMessage
            {
                DateTime = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                Ip = "127.0.0.1",
                Referrer = "PixelServiceTests",
                UserAgent = "testHttpClient"
            };

            try
            {
                // Act
                var cancelationTokenSource = new CancellationTokenSource();
                await messageConsumer.StartAsync(cancelationTokenSource.Token);

                // Assert
                var received = false;
                mockMessageProcessor.Setup(mp => mp.Process(It.IsAny<VisitMessage>()))
                    .Callback<VisitMessage>(actualMessage =>
                    {
                        Assert.IsNotNull(actualMessage);
                        Assert.AreEqual(expectedMessage.DateTime, actualMessage.DateTime);
                        Assert.AreEqual(expectedMessage.UserAgent, actualMessage.UserAgent);
                        Assert.AreEqual(expectedMessage.Referrer, actualMessage.Referrer);
                        Assert.AreEqual(expectedMessage.Ip, actualMessage.Ip);

                        received = true;
                    });

                SendMessage(hostName, queueName, expectedMessage);

                var tryCount = 0;
                while (!received && tryCount < 5)
                {
                    Thread.Sleep(500);
                    tryCount++;
                }

                Assert.IsTrue(received);
            }
            finally
            {
                var cancelationTokenSource = new CancellationTokenSource();
                await messageConsumer.StopAsync(cancelationTokenSource.Token);
            }
        }

        private void SendMessage<T>(string hostName, string queueName, T message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("Argument message can not be null");
            }

            var factory = new ConnectionFactory { HostName = hostName };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
            arguments: null);

            var jsonMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: queueName,
                                 basicProperties: null,
                                 body: body);
        }
    }
}