using System.Net;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PixelApiTests.Middlewares;

namespace PixelApiTests;

public class PixelApiTests
{
    [Fact]
    public async Task Track_WhenRequestHasIpAddress_ShouldReturnSuccessStatusCode()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IStartupFilter>(
                    new CustomRemoteIpStartupFilter(IPAddress.Parse("127.0.0.1"))
                );
                services.AddScoped(_ => Mock.Of<IPublishEndpoint>());
            });
        });
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/Track");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Track_WhenRequestIpAddressDoesNotExist_ShouldReturnSuccessBadRequest()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => Mock.Of<IPublishEndpoint>());
            });
        });
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/Track");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
