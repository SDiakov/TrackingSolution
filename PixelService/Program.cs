using Common;
using PixelService;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IMessageSender<VisitMessage>>((services) =>
    new RabbitMqMessageSender<VisitMessage>(
        builder.Configuration.GetValue<string>("MessageSender:HostName"),
        builder.Configuration.GetValue<string>("MessageSender:VisitQueueName")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/track", (HttpContext context, IMessageSender<VisitMessage> visitSender) =>
{
    // In order to provide asynchronous communication between
    // multiple instances of PixelService and single Storage service
    // it's proposed to use any message/event based approach
    // An abstract sender is injected here with RabbitMQ simple implementation binded
    var visit = new VisitMessage
    {
        DateTime = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
        Ip = context.Connection.RemoteIpAddress?.ToString(),
        Referrer = context.Request.Headers.Referer,
        UserAgent = context.Request.Headers.UserAgent
    };
    visitSender.SendMessage(visit);

    const string trackingPixel = "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7";
    return Results.File(Convert.FromBase64String(trackingPixel), "image/gif");
})
.WithName("GetTrackingPixel")
.WithOpenApi();

app.Run();

// Added public partial class to allow integration testing
public partial class Program { }
