namespace Common;

public record HttpRequestCreated(
    string? Referer,
    string? UserAgent,
    string? IpAddress,
    string? Id,
    DateTime? CreatedAt);