namespace PixelApi.Domain;

public record HttpRequestLogNotification(string? Referer, string? UserAgent, string? IpAddress) : Notification;