using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Grpc.Core;
using Grpc.Core.Utils;
using Grpc.Net.Testing.Moq.Naming;
using Grpc.Net.Testing.Moq.Tests.Proto;
using Moq;
using Xunit;

namespace Grpc.Net.Testing.Moq.Tests.Naming;

public class AsyncDuplexStreamingCallNamingPolicyTests
{
    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_WithRequest(TestRequest[] requests, TestResponse[] expectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleClientServerStream(null, null, default))
            .Returns(expectedResponses);

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
        AsyncDuplexStreamingCallNamingPolicy.Returns(
            grpcMock.Setup(c => c.SimpleClientServerStream(null, null, default)),
            () => expectedResponses);

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientServerStream();
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
            .Returns(rs => rs.Select(r => new TestResponse { Val = r.Val }));

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
}
