using System.ComponentModel.DataAnnotations;
using Common;
using MassTransit;
using Serilog;
using Serilog.Core;

namespace StorageService.Consumers;

public class HttpRequestCreatedConsumer : IConsumer<HttpRequestCreated>
{
    private readonly Logger _fileLogger;
    private readonly ILogger<HttpRequestCreatedConsumer> _logger;

    public HttpRequestCreatedConsumer(ILogger<HttpRequestCreatedConsumer> logger)
    {
        _logger = logger;
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        _fileLogger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }

    /// <summary>
    ///     Consumes the HttpRequestCreated message from the message queue.
    /// </summary>
    /// <param name="context">The context of the consumed message. This cannot be null.</param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns>A Task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    public async Task Consume(ConsumeContext<HttpRequestCreated> context)
    {
        _logger.LogDebug(
            "Consuming HttpRequestCreated with ID: {HttpRequestId}",
            context.Message.Id
        );

        try
        {
            var httpRequestCreated = context.Message;
            ArgumentNullException.ThrowIfNull(httpRequestCreated);

            if (string.IsNullOrWhiteSpace(httpRequestCreated.IpAddress))
                throw new ValidationException("Ip Address cannot be empty or null");

            var message = FormatContent(httpRequestCreated);
            // Logging the message as a Serilog ILogger implementation to the file avoiding concurrency issues
            await Task.Run(() => _fileLogger.Information("{Message}", message));
            _logger.LogInformation("Stored HTTP request {HttpRequestId}", httpRequestCreated.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error storing HttpRequestCreated with ID: {HttpRequestId}",
                context.Message.Id
            );
            // Improve this later to handling the error appropriately (e.g., re-queue the message, move to an error queue, etc.)
        }
    }

    private static string FormatContent(HttpRequestCreated content)
    {
        return $"{content.CreatedAt:O}|{content.Referer ?? "null"}|{content.UserAgent ?? "null"}|{content.IpAddress ?? "null"}";
    }
}
