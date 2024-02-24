using MassTransit;
using StorageService.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Configure MassTransit with RabbitMQ, setting up consumers and delayed message scheduler
builder.Services.AddMassTransit(config =>
{
    config.AddDelayedMessageScheduler();
    config.AddConsumers(typeof(HttpRequestCreatedConsumer).Assembly);

    config.UsingRabbitMq(
        (context, cfg) =>
        {
            cfg.Host(builder.Configuration.GetConnectionString("RabbitMq"));
            cfg.UseDelayedMessageScheduler();
            cfg.ReceiveEndpoint(
                "http-request-created-event",
                endpointConfig =>
                {
                    endpointConfig.ConfigureConsumer<HttpRequestCreatedConsumer>(context);
                }
            );
        }
    );
});

// Add MassTransit background service and health checks for RabbitMQ
builder
    .Services.AddHealthChecks()
    .AddRabbitMQ(
        new Uri(
            builder.Configuration.GetConnectionString("RabbitMq")
                ?? throw new InvalidOperationException()
        )
    );

var app = builder.Build();
await app.RunAsync();
