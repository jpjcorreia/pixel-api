using Common;
using MassTransit;
using PixelApi.Extensions;

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

var app = builder.Build();

// Middleware to add the request date to the HttpContext
app.UseHttpRequestDate();

app.MapGet(
    "/Track",
    async (HttpContext context, IPublishEndpoint endpoint, ILogger<Program> logger) =>
    {
        var trackingPixelData = Convert.FromBase64String(
            "R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw=="
        );

        try
        {
            var referer = context.Request.GetTypedHeaders().Referer?.ToString();
            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();
            var ipAddress = context.Connection.RemoteIpAddress?.MapToIPv4().ToString();

            logger.LogDebug(
                "Tracking request received with Referer: {Referer}, User-Agent: {UserAgent}, IP Address: {IPAddress}",
                referer,
                userAgent,
                ipAddress
            );

            var requestCreatedEvent = new HttpRequestCreated(
                referer,
                userAgent,
                ipAddress,
                Guid.NewGuid().ToString(),
                DateTime.UtcNow
            );
            await endpoint.Publish(requestCreatedEvent);

            logger.LogInformation(
                "Published HttpRequestCreated event : {Id}",
                requestCreatedEvent.Id
            );

            return Results.File(trackingPixelData, "image/gif");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing track request");
            return Results.File(trackingPixelData, "image/gif");
        }
    }
);

await app.RunAsync();
