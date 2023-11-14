using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Grpc.Net.Testing.Moq.Naming;
using Grpc.Net.Testing.Moq.Tests.Proto;
using Moq;
using Xunit;

namespace Grpc.Net.Testing.Moq.Tests.Naming;

public class AsyncUnaryCallNamingPolicyTests
{
    [Theory, AutoData]
    public async Task SimpleAsync_ShouldReturnResponse(TestResponse expectedResponse)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleAsync(It.IsAny<TestRequest>(), null, null, default))
            .Returns(expectedResponse);

        var client = grpcMock.Object;

        // Act
        var response = await client.SimpleAsync(new TestRequest());

        // Assert
        response.Should().BeEquivalentTo(expectedResponse);
    }

    [Theory, AutoData]
    public async Task SimpleAsync_ShouldReturnResponse_WhenResponseIsLambda(TestResponse expectedResponse)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        AsyncUnaryCallNamingPolicy.Returns(
            grpcMock.Setup(c => c.SimpleAsync(It.IsAny<TestRequest>(), null, null, default)),
            () => expectedResponse);

        var client = grpcMock.Object;

        // Act
        var response = await client.SimpleAsync(new TestRequest());

        // Assert
        response.Should().BeEquivalentTo(expectedResponse);
    }

    [Theory, AutoData]
    public async Task SimpleAsync_ShouldReturnResponse_WhenResponseIsFunc(TestRequest request)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleAsync(It.IsAny<TestRequest>(), null, null, default))
            .Returns<TestService.TestServiceClient, TestRequest, TestResponse>(c => new TestResponse { Val = c.Val });

        var client = grpcMock.Object;

        // Act
        var response = await client.SimpleAsync(request);

        // Assert
        response.Val.Should().Be(request.Val);
    }
}
