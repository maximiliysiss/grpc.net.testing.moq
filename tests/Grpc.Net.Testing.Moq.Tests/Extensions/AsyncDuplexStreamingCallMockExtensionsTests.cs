using System.Collections.Generic;
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
    public async Task SimpleServer_ShouldReturnResponse_Sequential(TestResponse[] firstExpectedResponses, TestResponse[] secondExpectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .SetupSequence(c => c.SimpleClientServerStream(null, null, default))
            .ReturnsAsync(firstExpectedResponses)
            .ReturnsAsync(secondExpectedResponses);

        var client = grpcMock.Object;

        // Act
        var firstCall = client.SimpleClientServerStream();
        await firstCall.RequestStream.CompleteAsync();

        var firstMessages = await firstCall.ResponseStream
            .ReadAllAsync()
            .ToArrayAsync();

        var secondCall = client.SimpleClientServerStream();
        await secondCall.RequestStream.CompleteAsync();

        var secondMessages = await secondCall.ResponseStream
            .ReadAllAsync()
            .ToArrayAsync();

        // Assert
        firstMessages.Should().BeEquivalentTo(firstExpectedResponses);
        secondMessages.Should().BeEquivalentTo(secondExpectedResponses);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_SequentialByFunc(TestResponse[] firstExpectedResponses, TestResponse[] secondExpectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .SetupSequence(c => c.SimpleClientServerStream(null, null, default))
            .ReturnsAsync(() => firstExpectedResponses)
            .ReturnsAsync(() => secondExpectedResponses);

        var client = grpcMock.Object;

        // Act
        var firstCall = client.SimpleClientServerStream();
        await firstCall.RequestStream.CompleteAsync();

        var firstMessages = await firstCall.ResponseStream
            .ReadAllAsync()
            .ToArrayAsync();

        var secondCall = client.SimpleClientServerStream();
        await secondCall.RequestStream.CompleteAsync();

        var secondMessages = await secondCall.ResponseStream
            .ReadAllAsync()
            .ToArrayAsync();

        // Assert
        firstMessages.Should().BeEquivalentTo(firstExpectedResponses);
        secondMessages.Should().BeEquivalentTo(secondExpectedResponses);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_SequentialByLambda(TestRequest[] firstRequests, TestRequest[] secondRequests)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .SetupSequence(c => c.SimpleClientServerStream(null, null, default))
            .ReturnsAsync(rs => rs.Select(r => new TestResponse { Val = r.Val }))
            .ReturnsAsync(rs => rs.Select(r => new TestResponse { Val = r.Val * 2 }));

        var client = grpcMock.Object;

        // Act
        var firstCall = client.SimpleClientServerStream();
        await firstCall.RequestStream.WriteAllAsync(firstRequests);

        var firstMessages = await firstCall.ResponseStream
            .ReadAllAsync()
            .ToArrayAsync();

        var secondCall = client.SimpleClientServerStream();
        await secondCall.RequestStream.WriteAllAsync(secondRequests);

        var secondMessages = await secondCall.ResponseStream
            .ReadAllAsync()
            .ToArrayAsync();

        // Assert
        firstMessages.Should().BeEquivalentTo(firstRequests.Select(r => new TestResponse { Val = r.Val }));
        secondMessages.Should().BeEquivalentTo(secondRequests.Select(r => new TestResponse { Val = r.Val * 2 }));
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_SequentialByReactive(TestRequest[] firstRequests, TestRequest[] secondRequests)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .SetupSequence(c => c.SimpleClientServerStream(null, null, default))
            .ReturnsAsync(rs => new TestResponse { Val = rs.Val })
            .ReturnsAsync(rs => new TestResponse { Val = rs.Val * 2 });

        var client = grpcMock.Object;

        // Act
        var firstCall = client.SimpleClientServerStream();
        await firstCall.RequestStream.WriteAllAsync(firstRequests);

        var firstMessages = await firstCall.ResponseStream
            .ReadAllAsync()
            .ToArrayAsync();

        var secondCall = client.SimpleClientServerStream();
        await secondCall.RequestStream.WriteAllAsync(secondRequests);

        var secondMessages = await secondCall.ResponseStream
            .ReadAllAsync()
            .ToArrayAsync();

        // Assert
        firstMessages.Should().BeEquivalentTo(firstRequests.Select(r => new TestResponse { Val = r.Val }));
        secondMessages.Should().BeEquivalentTo(secondRequests.Select(r => new TestResponse { Val = r.Val * 2 }));
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponseAndCallback(TestResponse[] expectedResponses)
    {
        // Arrange
        var flag = false;

        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleClientServerStream(null, null, default))
            .Callback(() => flag = true)
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

        flag.Should().BeTrue();
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
    public async Task SimpleServer_ShouldReturnResponse_ByLambdaWithReactiveHandling(TestRequest[] requests)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleClientServerStream(null, null, default))
            .ReturnsAsync(rs => new TestResponse { Val = rs.Val });

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientServerStream();

        var messages = new List<TestResponse>(requests.Length);

        var streamWriter = call.RequestStream;
        var responseStream = call.ResponseStream;

        foreach (var testRequest in requests)
        {
            await streamWriter.WriteAsync(testRequest);

            await responseStream.MoveNext();
            messages.Add(responseStream.Current);
        }

        await streamWriter.CompleteAsync();
        var moveNext = await responseStream.MoveNext();

        // Assert
        moveNext.Should().BeFalse();

        for (var i = 0; i < messages.Count; i++)
            messages[i].Val.Should().Be(requests[i].Val);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_ByLambdaWithReactiveHandlingAndCreationTaskBeforeSendRequest(TestRequest[] requests)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleClientServerStream(null, null, default))
            .ReturnsAsync(rs => new TestResponse { Val = rs.Val });

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientServerStream();

        var responseTask = call.ResponseStream.ReadAllAsync().ToArrayAsync();
        await call.RequestStream.WriteAllAsync(requests);

        // Assert
        var messages = await responseTask;

        for (var i = 0; i < messages.Length; i++)
            messages[i].Val.Should().Be(requests[i].Val);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_ByLambdaWithReactiveHandlingAndCallCallback(TestRequest[] requests)
    {
        // Arrange
        var flag = false;

        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleClientServerStream(null, null, default))
            .Callback(() => flag = true)
            .ReturnsAsync(rs => new TestResponse { Val = rs.Val });

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleClientServerStream();

        var messages = new List<TestResponse>(requests.Length);

        var streamWriter = call.RequestStream;
        var responseStream = call.ResponseStream;

        foreach (var testRequest in requests)
        {
            await streamWriter.WriteAsync(testRequest);

            await responseStream.MoveNext();
            messages.Add(responseStream.Current);
        }

        await streamWriter.CompleteAsync();
        var moveNext = await responseStream.MoveNext();

        // Assert
        moveNext.Should().BeFalse();

        for (var i = 0; i < messages.Count; i++)
            messages[i].Val.Should().Be(requests[i].Val);

        flag.Should().BeTrue();
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
