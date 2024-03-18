using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace StorageService
{
    public class RabbitMqMessageConsumer<T> : IHostedService
    {
        private string hostName;
        private string queueName;
        private IMessageProcessor<T> processor;
        private IModel channel = null;
        private IConnection connection = null;
        private string consumerTag;

        // ToDo: Think about an additional layer of abstraction
        // to decouple starting of HostedService and RabbitMQ consumer
        public RabbitMqMessageConsumer(string hostName, string queueName, IMessageProcessor<T> logger)
        {
            this.hostName = hostName;
            this.queueName = queueName;
            this.processor = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            var factory = new ConnectionFactory { HostName = hostName };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += ProcessMessage;

            // ToDo: Revisit acknowledgement policy
            consumerTag = channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(consumerTag))
            {
                channel.BasicCancel(consumerTag);
            }
            channel.Dispose();
            connection.Dispose();
        }

        private void ProcessMessage(object? model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var jsonMessage = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<T>(jsonMessage);
            processor.Process(message);
        }
    }
}
