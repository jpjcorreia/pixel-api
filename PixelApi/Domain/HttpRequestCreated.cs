namespace PixelApi.Domain;

public interface HttpRequestCreated
{
    Guid Id { get; set; }
    string? Referer { get; set; }
    string? UserAgent { get; set; }
    string? IpAddress { get; set; }
    DateTime CreationDate { get; set; }
}