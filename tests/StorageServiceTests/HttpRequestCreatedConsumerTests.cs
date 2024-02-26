using System.ComponentModel.DataAnnotations;
using Common;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using StorageService.Consumers;

namespace StorageServiceTests;

public class HttpRequestCreatedConsumerTests
{
    [Fact]
    public async Task HttpRequestCreatedConsumerConsume_WhenConsumedMessageIsValid_ShouldNotThrowException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<HttpRequestCreatedConsumer>>();
        var contextMock = new Mock<ConsumeContext<HttpRequestCreated>>();
        var httpRequestCreated = new HttpRequestCreated(
            "http://example.com",
            "Mozilla/110.0",
            "127.0.0.1",
            Guid.NewGuid().ToString(),
            DateTime.UtcNow
        );
        contextMock.Setup(c => c.Message).Returns(httpRequestCreated);
        var consumer = new HttpRequestCreatedConsumer(loggerMock.Object);

        // Act
        var exception = await Record.ExceptionAsync(() => consumer.Consume(contextMock.Object));

        //Assert
        Assert.Null(exception);
    }

    // Throw an exception if the consumed message is null
    [Fact]
    public async Task HttpRequestCreatedConsumerConsume_WhenConsumedMessageIsNull_ShouldThrowException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<HttpRequestCreatedConsumer>>();
        var contextMock = new Mock<ConsumeContext<HttpRequestCreated>>();
        contextMock.Setup(c => c.Message).Returns((HttpRequestCreated)null);

        var httpRequestCreatedConsumer = new HttpRequestCreatedConsumer(loggerMock.Object);

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => httpRequestCreatedConsumer.Consume(contextMock.Object)
        );
    }

    // Throw a ValidationException if the IpAddress property of the message is null or empty
    [Fact]
    public async Task HttpRequestCreatedConsumerConsume_WhenConsumedMessageDoesNotContainIpAddress_ShouldThrowException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<HttpRequestCreatedConsumer>>();
        var contextMock = new Mock<ConsumeContext<HttpRequestCreated>>();
        var httpRequestCreated = new HttpRequestCreated(
            "http://example.com",
            "Mozilla/110.0",
            "",
            Guid.NewGuid().ToString(),
            DateTime.UtcNow
        );
        contextMock.Setup(c => c.Message).Returns(httpRequestCreated);

        var httpRequestCreatedConsumer = new HttpRequestCreatedConsumer(loggerMock.Object);

        // Act and Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => httpRequestCreatedConsumer.Consume(contextMock.Object)
        );
    }
}
