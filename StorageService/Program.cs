using Common;

namespace StorageService
{
    // No requirements or lomotations were provided for the type of StorageService
    // So it's implemented as Web Service with HostedService to consume RabbitMQ messages continiously
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // Added hosted service to consume message continiously
            builder.Services.AddHostedService(services => 
                new RabbitMqMessageConsumer<VisitMessage>(
                    builder.Configuration.GetValue<string>("MessageConsumer:HostName"),
                    builder.Configuration.GetValue<string>("MessageConsumer:Visits:QueueName"),
                    new VisitLogger(builder.Configuration.GetValue<string>("MessageConsumer:Visits:LogFilePath"))));
            
            var app = builder.Build();
            app.Run();
        }
    }
}