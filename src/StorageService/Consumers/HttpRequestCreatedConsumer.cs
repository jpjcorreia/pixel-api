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
    ///     Asynchronously consumes the HttpRequestCreated message from the message queue.
    /// </summary>
    /// <param name="context">The context of the consumed message. This cannot be null.</param>
    /// <exception cref="ValidationException">Thrown when IP address from the HttpRequestCreated message is empty or null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    /// <returns>A Task that represents the asynchronous operation.</returns>
    /// <remarks>
    ///     If an exception occurs during the operation, it logs the error and rethrows the exception.
    ///     Should be improved to handle the error appropriately (e.g., re-queue the message, move to an error queue, etc.).
    /// </remarks>
    public Task Consume(ConsumeContext<HttpRequestCreated> context)
    {
        var httpRequestCreated = context.Message;

        ArgumentNullException.ThrowIfNull(httpRequestCreated);

        if (string.IsNullOrWhiteSpace(httpRequestCreated.IpAddress))
            throw new ValidationException("Ip Address cannot be empty or null");

        try
        {
            _logger.LogDebug(
                "Consuming HttpRequestCreated with ID: {MessageId}",
                context.Message.Id
            );

            var message = FormatContent(httpRequestCreated);

            // Logging the message as a Serilog ILogger implementation to the file avoiding concurrency issues
            _fileLogger.Information("{Message}", message);

            _logger.LogInformation("Stored HTTP request {HttpRequestId}", httpRequestCreated.Id);
        }
        catch (Exception ex)
        {
            // Improve this later to handling the error appropriately (e.g., re-queue the message, move to an error queue, etc.)
            _logger.LogError(ex, "Error storing HttpRequestCreated: {Message}", context.Message);

            throw;
        }

        return Task.CompletedTask;
    }

    private static string FormatContent(HttpRequestCreated content)
    {
        return $"{content.CreatedAt:O}|{content.Referer ?? "null"}|{content.UserAgent ?? "null"}|{content.IpAddress ?? "null"}";
    }
}
