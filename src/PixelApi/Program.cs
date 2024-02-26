using Common;
using MassTransit;
using PixelApi.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Configure MassTransit with RabbitMQ, including the delayed message scheduler and message retry policies
builder.Services.AddMassTransit(config =>
{
    config.AddDelayedMessageScheduler();
    config.UsingRabbitMq(
        (context, cfg) =>
        {
            var rabbitMqConnectionString = builder.Configuration.GetConnectionString("RabbitMq");
            cfg.Host(rabbitMqConnectionString);
            cfg.UseDelayedMessageScheduler();
            cfg.UseMessageRetry(retryConfig => retryConfig.Interval(3, TimeSpan.FromSeconds(3)));
        }
    );
});

// Add health checks for the application and RabbitMQ
builder
    .Services.AddHealthChecks()
    .AddRabbitMQ(
        new Uri(
            builder.Configuration.GetConnectionString("RabbitMq")
                ?? throw new InvalidOperationException()
        )
    );

// Add logging and exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();

app.MapGet(
    "/Track",
    async (HttpContext context, IPublishEndpoint endpoint, ILogger<Program> logger) =>
    {
        if (
            context.Connection.RemoteIpAddress is null
            || string.IsNullOrWhiteSpace(context.Connection.RemoteIpAddress.ToString())
        )
            return Results.BadRequest("IP Address is required to track the request.");

        var trackingPixelData = Convert.FromBase64String(
            "R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw=="
        );

        var referer = context.Request.GetTypedHeaders().Referer?.ToString();
        var userAgent = context.Request.Headers.UserAgent.FirstOrDefault();
        var ipAddress = context.Connection.RemoteIpAddress?.MapToIPv4().ToString();

        var requestCreatedEvent = new HttpRequestCreated(
            referer,
            userAgent,
            ipAddress,
            Guid.NewGuid().ToString(),
            DateTime.UtcNow
        );

        logger.LogDebug(
            "Tracking request received with Id: {Id}, Referer: {Referer}, User-Agent: {UserAgent}, IP Address: {IPAddress}",
            requestCreatedEvent.Id,
            requestCreatedEvent.Referer,
            requestCreatedEvent.UserAgent,
            requestCreatedEvent.IpAddress
        );

        await endpoint.Publish(requestCreatedEvent);

        logger.LogInformation("Published HttpRequestCreated event : {Id}", requestCreatedEvent.Id);

        return Results.File(trackingPixelData, "image/gif");
    }
);

await app.RunAsync();

public partial class Program { }
