using PixelApi.Middlewares;

namespace PixelApi.Extensions;

public static class HttpRequestDateMiddlewareExtensions
{
    public static void UseHttpRequestDate(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<HttpRequestDateTimeMiddleware>();
    }
}