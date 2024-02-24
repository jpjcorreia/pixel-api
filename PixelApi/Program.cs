using System.Net;
using Common;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
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
            var trackingPixelData = Convert.FromBase64String(
                "R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw=="
            );

            var referer = context.Request.GetTypedHeaders().Referer?.ToString();
            var userAgent = context.Request.Headers.UserAgent.FirstOrDefault();
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
    )
    .AddEndpointFilter(
        async (context, next) =>
        {
            if (context.HttpContext.Connection.RemoteIpAddress is null)
            {
                return Results.Problem(
                    "IP Address is required to track the request.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Bad Request"
                );
            }

            return await next(context);
        }
    );

await app.RunAsync();
