using Common;
using PixelService;
using System.Globalization;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TrackerSolution.IntegrationTests.PixelService
{
    [TestClass]
    public class PixelServiceMessageSenderTests
    {
        const string hostName = "localhost";

        [TestMethod]
        public async Task SendMessageTest_VisitMessageIsSentAndCanBeConsumed()
        {
            // Arrange
            var queueName = "testVisitQueue";

            var messageSender = new RabbitMqMessageSender<VisitMessage>(hostName, queueName);

            var expectedMessage = new VisitMessage
            {
                DateTime = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                Ip = "127.0.0.1",
                Referrer = "PixelServiceTests",
                UserAgent = "testHttpClient"
            };

            // Act
            messageSender.SendMessage(expectedMessage);

            // Assert
            var actualMessage = ConsumeMessage<VisitMessage>(hostName, queueName);
            Assert.IsNotNull(actualMessage);
            Assert.AreEqual(expectedMessage.DateTime, actualMessage.DateTime);
            Assert.AreEqual(expectedMessage.UserAgent, actualMessage.UserAgent);
            Assert.AreEqual(expectedMessage.Referrer, actualMessage.Referrer);
            Assert.AreEqual(expectedMessage.Ip, actualMessage.Ip);
        }

        [TestMethod]
        public void SendMessageTest_MessageIsNull_ThrowException()
        {
            // Arrange
            // ToDo: move to appsettings.json of Test proj
            var hostName = "localhost";
            var queueName = "testVisitQueue";

            var messageSender = new RabbitMqMessageSender<VisitMessage>(hostName, queueName);

            // Act and Assert
            Assert.ThrowsException<ArgumentNullException>(() => messageSender.SendMessage(null));
        }

        private T ConsumeMessage<T>(string hostName, string queueName)
        {
            T message = default;
            var factory = new ConnectionFactory { HostName = hostName };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
            autoDelete: false,
                                 arguments: null);

            var received = false;
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var jsonMessage = Encoding.UTF8.GetString(body);
                message = JsonSerializer.Deserialize<T>(jsonMessage);
                received = true;
            };

            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);
            var tryCount = 0;
            while (!received && tryCount < 5)
            {
                Thread.Sleep(500);
                tryCount++;
            }

            return message;
        }
    }
}