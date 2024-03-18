using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PixelService
{
    public class RabbitMqMessageSender<T>: IMessageSender<T>
    {
        private readonly string hostName;
        private readonly string queueName;

        public RabbitMqMessageSender(string hostName, string queueName)
        {
            this.hostName = hostName;
            this.queueName = queueName;
        }

        public void SendMessage(T message)
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
