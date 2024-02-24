using Common;
using MassTransit;
using StorageService.Services;

namespace StorageService.Consumers;

public class HttpRequestCreatedConsumer : IConsumer<HttpRequestCreated>
{
    private readonly ILogger<HttpRequestCreatedConsumer> _logger;
    private readonly IStorageService<HttpRequestCreated> _storageService;

    public HttpRequestCreatedConsumer(
        ILogger<HttpRequestCreatedConsumer> logger,
        IStorageService<HttpRequestCreated> storageService
    )
    {
        _logger = logger;
        _storageService = storageService;
    }

    /// <summary>
    ///     Consumes the HttpRequestCreated message from the message queue.
    /// </summary>
    /// <param name="context">The context of the consumed message. This cannot be null.</param>
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
            await _storageService.StoreAsync(context.Message);
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
}
