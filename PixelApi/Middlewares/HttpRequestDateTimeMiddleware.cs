namespace PixelApi.Middlewares;

public class HttpRequestDateTimeMiddleware
{
    private readonly RequestDelegate _next;

    public HttpRequestDateTimeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context)
    {
        context.Items.Add("RequestDate", DateTime.UtcNow);
        return _next(context);
    }
}
