using Common;
using Serilog;
using Serilog.Core;

namespace StorageService.Services;

public class FileStorageService : IStorageService<HttpRequestCreated>, IDisposable
{
    private readonly Logger _fileLogger;
    private readonly string _filePath;
    private readonly ILogger<FileStorageService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileStorageService" /> class.
    /// </summary>
    /// <param name="filePath">The path to the file where the HTTP request data will be stored. This cannot be null or empty.</param>
    /// <param name="logger">The logger used for logging information about the storage operations.</param>
    /// <exception cref="ArgumentException">Thrown when the provided filePath is null or empty.</exception>
    public FileStorageService(string? filePath, ILogger<FileStorageService> logger)
    {
        _filePath = !string.IsNullOrWhiteSpace(filePath)
            ? filePath
            : throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        _logger = logger;
        _fileLogger = new LoggerConfiguration()
            .WriteTo.Async(
                configure => configure.File(_filePath, rollingInterval: RollingInterval.Infinite,
                    outputTemplate: "{Message}{NewLine}"), blockWhenFull: false)
            .CreateLogger();
    }

    public void Dispose()
    {
        _fileLogger.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Asynchronously stores the provided HTTP request data.
    /// </summary>
    /// <param name="content">The HTTP request data to be stored. This cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided content is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the IP address in the provided content is null or empty.</exception>
    public async Task StoreAsync(HttpRequestCreated content)
    {
        if (content is null) throw new ArgumentNullException(nameof(content));
        if (string.IsNullOrWhiteSpace(content.IpAddress))
            throw new ArgumentException("Ip Address cannot be empty or null", nameof(content));

        string message = FormatContent(content);
        await Task.Run(() => _fileLogger.Information("{Message}", message));
        _logger.LogInformation("Stored HTTP request {HttpRequestId} in file {FilePath}", content.Id, _filePath);
    }

    private static string FormatContent(HttpRequestCreated content)
    {
        return
            $"{content.CreatedAt:O}|{content.Referer ?? "null"}|{content.UserAgent ?? "null"}|{content.IpAddress ?? "null"}";
    }
}