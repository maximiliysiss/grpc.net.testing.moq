using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Testing.Moq.Extensions;
using Grpc.Net.Testing.Moq.Tests.Proto;
using Moq;
using Xunit;

namespace Grpc.Net.Testing.Moq.Tests;

public class AsyncServerStreamingCallMockExtensionsTests
{
    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse(TestResponse[] expectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .When(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default))
            .Returns(expectedResponses);

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleServerStream(new TestRequest());
        var messages = await call.ResponseStream.ReadAllAsync().ToArrayAsync();

        // Assert
        messages.Should().BeEquivalentTo(expectedResponses);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_WhenCalledByLambda(TestResponse[] expectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .When(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default))
            .Returns(() => expectedResponses);

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleServerStream(new TestRequest());
        var messages = await call.ResponseStream.ReadAllAsync().ToArrayAsync();

        // Assert
        messages.Should().BeEquivalentTo(expectedResponses);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_WhenCalledByLambdaWithRequest(TestRequest request)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .When(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default))
            .Returns<TestRequest>(r => Enumerable.Repeat(new TestResponse { Val = r.Val }, 2));

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleServerStream(request);
        var messages = await call.ResponseStream.ReadAllAsync().ToArrayAsync();

        // Assert
        messages.Should().AllSatisfy(response => response.Val.Should().Be(request.Val));
    }
}
