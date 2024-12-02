using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Grpc.Core;
using Grpc.Core.Utils;
using Grpc.Net.Testing.Moq.Extensions;
using Grpc.Net.Testing.Moq.Tests.Proto;
using Moq;
using Xunit;

namespace Grpc.Net.Testing.Moq.Tests.Extensions;

public class AsyncDuplexStreamingCallMockExtensionsTests
{
    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse(TestResponse[] expectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleClientServerStream(null, null, default))
            .ReturnsAsync(expectedResponses);

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientServerStream();

        await call.RequestStream.CompleteAsync();

        var messages = await call.ResponseStream
            .ReadAllAsync()
            .ToArrayAsync();

        // Assert
        messages.Should().BeEquivalentTo(expectedResponses);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_WithRequest(TestRequest[] requests, TestResponse[] expectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleClientServerStream(null, null, default))
            .ReturnsAsync(expectedResponses);

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientServerStream();

        await call.RequestStream.WriteAllAsync(requests, complete: true);
        var messages = await call.ResponseStream.ReadAllAsync().ToArrayAsync();

        // Assert
        messages.Should().BeEquivalentTo(expectedResponses);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_ByLambda(TestResponse[] expectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleClientServerStream(null, null, default))
            .ReturnsAsync(() => expectedResponses);

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientServerStream();

        await call.RequestStream.CompleteAsync();

        var messages = await call.ResponseStream
            .ReadAllAsync()
            .ToArrayAsync();

        // Assert
        messages.Should().BeEquivalentTo(expectedResponses);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_ByLambda_WithRequests(TestRequest[] requests, TestResponse[] expectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleClientServerStream(null, null, default))
            .ReturnsAsync(() => expectedResponses);

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientServerStream();

        await call.RequestStream.WriteAllAsync(requests, complete: true);
        var messages = await call.ResponseStream.ReadAllAsync().ToArrayAsync();

        // Assert
        messages.Should().BeEquivalentTo(expectedResponses);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_ByLambdaWithRequest(TestRequest[] requests)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleClientServerStream(null, null, default))
            .ReturnsAsync(rs => rs.Select(r => new TestResponse { Val = r.Val }));

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientServerStream();

        await call.RequestStream.WriteAllAsync(requests, complete: true);
        var messages = await call.ResponseStream.ReadAllAsync().ToArrayAsync();

        // Assert
        for (var i = 0; i < messages.Length; i++)
        {
            var request = requests[i];
            var response = messages[i];

            response.Val.Should().Be(request.Val);
        }
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_ByLambdaWithRequestAndNotStandardWayToAwait(TestRequest[] requests)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleClientServerStream(null, null, default))
            .ReturnsAsync(rs => rs.Select(r => new TestResponse { Val = r.Val }));

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientServerStream();

        var requestTask = call.RequestStream.WriteAllAsync(requests, complete: true);
        var messages = await call.ResponseStream.ReadAllAsync().ToArrayAsync();

        await requestTask;

        // Assert
        for (var i = 0; i < messages.Length; i++)
        {
            var request = requests[i];
            var response = messages[i];

            response.Val.Should().Be(request.Val);
        }
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_ByLambdaWithRequestAndAccumulate(TestRequest[] requests)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleClientServerStream(null, null, default))
            .ReturnsAsync(rs => new[] { new TestResponse { Val = rs.Sum(r => r.Val) } });

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientServerStream();

        var requestTask = call.RequestStream.WriteAllAsync(requests, complete: true);
        var messages = await call.ResponseStream.ReadAllAsync().ToArrayAsync();

        await requestTask;

        // Assert
        var expectedValue = requests.Sum(r => r.Val);

        messages
            .Should().ContainSingle()
            .Which.Val.Should().Be(expectedValue);
    }
}
