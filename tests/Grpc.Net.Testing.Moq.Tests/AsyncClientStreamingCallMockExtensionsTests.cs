using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Grpc.Core.Utils;
using Grpc.Net.Testing.Moq.Extensions;
using Grpc.Net.Testing.Moq.Tests.Proto;
using Moq;
using Xunit;

namespace Grpc.Net.Testing.Moq.Tests;

public class AsyncClientStreamingCallMockExtensionsTests
{
    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse(TestResponse expectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .When(c => c.SimpleClientStream(null, null, default))
            .Returns(expectedResponses);

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientStream();
        var message = await call;

        // Assert
        message.Should().BeEquivalentTo(expectedResponses);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_WithRequests(TestRequest[] requests, TestResponse expectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .When(c => c.SimpleClientStream(null, null, default))
            .Returns(expectedResponses);

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientStream();

        await call.RequestStream.WriteAllAsync(requests, complete: true);
        var message = await call;

        // Assert
        message.Should().BeEquivalentTo(expectedResponses);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_ByLambda(TestResponse expectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .When(c => c.SimpleClientStream(null, null, default))
            .Returns(() => expectedResponses);

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientStream();
        var message = await call;

        // Assert
        message.Should().BeEquivalentTo(expectedResponses);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_ByLambda_WithRequests(TestRequest[] requests, TestResponse expectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .When(c => c.SimpleClientStream(null, null, default))
            .Returns(() => expectedResponses);

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientStream();

        await call.RequestStream.WriteAllAsync(requests, complete: true);
        var message = await call;

        // Assert
        message.Should().BeEquivalentTo(expectedResponses);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_ByLambdaWithRequests(TestRequest[] requests)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .When(c => c.SimpleClientStream(null, null, default))
            .Returns(rs => new TestResponse { Val = rs.Sum(r => r.Val) });

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientStream();

        await call.RequestStream.WriteAllAsync(requests, complete: true);
        var message = await call;

        // Assert
        var expectedValue = requests.Sum(r => r.Val);

        message.Val.Should().Be(expectedValue);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_ByLambdaWithRequestsAndNotStandardWayToAwait(TestRequest[] requests)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .When(c => c.SimpleClientStream(null, null, default))
            .Returns(rs => new TestResponse { Val = rs.Sum(r => r.Val) });

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientStream();

        var requestTask = call.RequestStream.WriteAllAsync(requests, complete: true);
        var message = await call;

        await requestTask;

        // Assert
        var expectedValue = requests.Sum(r => r.Val);

        message.Val.Should().Be(expectedValue);
    }
}
