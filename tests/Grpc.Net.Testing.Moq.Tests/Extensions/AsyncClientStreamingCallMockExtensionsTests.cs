using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Grpc.Core.Utils;
using Grpc.Net.Testing.Moq.Extensions;
using Grpc.Net.Testing.Moq.Tests.Proto;
using Moq;
using Xunit;

namespace Grpc.Net.Testing.Moq.Tests.Extensions;

public class AsyncClientStreamingCallMockExtensionsTests
{
    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse(TestResponse expectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleClientStream(null, null, default))
            .ReturnsAsync(expectedResponses);

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientStream();

        await call.RequestStream.CompleteAsync();

        var message = await call;

        // Assert
        message.Should().BeEquivalentTo(expectedResponses);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_Sequential(TestResponse firstExpectedResponses, TestResponse secondExpectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .SetupSequence(c => c.SimpleClientStream(null, null, default))
            .ReturnsAsync(firstExpectedResponses)
            .ReturnsAsync(secondExpectedResponses);

        var client = grpcMock.Object;

        // Act
        var firstCall = client.SimpleClientStream();
        await firstCall.RequestStream.CompleteAsync();

        var firstMessage = await firstCall;

        var secondCall = client.SimpleClientStream();
        await secondCall.RequestStream.CompleteAsync();

        var secondMessage = await secondCall;

        // Assert
        firstMessage.Should().BeEquivalentTo(firstExpectedResponses);
        secondMessage.Should().BeEquivalentTo(secondExpectedResponses);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_SequentialByFunc(TestResponse firstExpectedResponses, TestResponse secondExpectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .SetupSequence(c => c.SimpleClientStream(null, null, default))
            .ReturnsAsync(() => firstExpectedResponses)
            .ReturnsAsync(() => secondExpectedResponses);

        var client = grpcMock.Object;

        // Act
        var firstCall = client.SimpleClientStream();
        await firstCall.RequestStream.CompleteAsync();

        var firstMessage = await firstCall;

        var secondCall = client.SimpleClientStream();
        await secondCall.RequestStream.CompleteAsync();

        var secondMessage = await secondCall;

        // Assert
        firstMessage.Should().BeEquivalentTo(firstExpectedResponses);
        secondMessage.Should().BeEquivalentTo(secondExpectedResponses);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_SequentialByLambda(TestRequest[] firstRequests, TestRequest[] secondRequests)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .SetupSequence(c => c.SimpleClientStream(null, null, default))
            .ReturnsAsync(rs => new TestResponse { Val = rs.Sum(c => c.Val) })
            .ReturnsAsync(rs => new TestResponse { Val = rs.Sum(c => c.Val) * 2 });

        var client = grpcMock.Object;

        // Act
        var firstCall = client.SimpleClientStream();
        await firstCall.RequestStream.WriteAllAsync(firstRequests);

        var firstMessage = await firstCall;

        var secondCall = client.SimpleClientStream();
        await secondCall.RequestStream.WriteAllAsync(secondRequests);

        var secondMessage = await secondCall;

        // Assert
        firstMessage.Should().BeEquivalentTo(new TestResponse { Val = firstRequests.Sum(c => c.Val) });
        secondMessage.Should().BeEquivalentTo(new TestResponse { Val = secondRequests.Sum(c => c.Val) * 2 });
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponseAndCallback(TestResponse expectedResponses)
    {
        // Arrange
        var flag = false;

        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleClientStream(null, null, default))
            .Callback(() => flag = true)
            .ReturnsAsync(expectedResponses);

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientStream();

        await call.RequestStream.CompleteAsync();

        var message = await call;

        // Assert
        message.Should().BeEquivalentTo(expectedResponses);

        flag.Should().BeTrue();
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_WithRequests(TestRequest[] requests, TestResponse expectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleClientStream(null, null, default))
            .ReturnsAsync(expectedResponses);

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
            .Setup(c => c.SimpleClientStream(null, null, default))
            .ReturnsAsync(() => expectedResponses);

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientStream();

        await call.RequestStream.CompleteAsync();

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
            .Setup(c => c.SimpleClientStream(null, null, default))
            .ReturnsAsync(() => expectedResponses);

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
            .Setup(c => c.SimpleClientStream(null, null, default))
            .ReturnsAsync(rs => new TestResponse { Val = rs.Sum(r => r.Val) });

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
            .Setup(c => c.SimpleClientStream(null, null, default))
            .ReturnsAsync(rs => new TestResponse { Val = rs.Sum(r => r.Val) });

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
