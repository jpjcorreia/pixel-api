using Common;
using MassTransit;
using StorageService.Consumers;
using StorageService.Services;

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

// Configure the storage service
builder.Services.AddSingleton<IStorageService<HttpRequestCreated>, FileStorageService>(
    serviceProvider =>
    {
        var logger = serviceProvider.GetRequiredService<ILogger<FileStorageService>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var storagePath = configuration.GetValue<string>("Storage:File:Path");
        return new FileStorageService(storagePath, logger);
    }
);

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
