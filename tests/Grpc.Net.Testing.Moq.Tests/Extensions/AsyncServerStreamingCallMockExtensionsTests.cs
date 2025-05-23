﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Testing.Moq.Extensions;
using Grpc.Net.Testing.Moq.Tests.Proto;
using Moq;
using Xunit;

namespace Grpc.Net.Testing.Moq.Tests.Extensions;

public class AsyncServerStreamingCallMockExtensionsTests
{
    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse(TestResponse[] expectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default))
            .ReturnsAsync(expectedResponses);

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleServerStream(new TestRequest());
        var messages = await call.ResponseStream.ReadAllAsync().ToArrayAsync();

        // Assert
        messages.Should().BeEquivalentTo(expectedResponses);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_Sequential(TestResponse[] firstExpectedResponses, TestResponse[] secondExpectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .SetupSequence(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default))
            .ReturnsAsync(firstExpectedResponses)
            .ReturnsAsync(secondExpectedResponses);

        var client = grpcMock.Object;

        // Act
        var firstMessages = await client.SimpleServerStream(new TestRequest()).ResponseStream.ReadAllAsync().ToArrayAsync();
        var secondMessages = await client.SimpleServerStream(new TestRequest()).ResponseStream.ReadAllAsync().ToArrayAsync();

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
            .SetupSequence(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default))
            .ReturnsAsync(() => firstExpectedResponses)
            .ReturnsAsync(() => secondExpectedResponses);

        var client = grpcMock.Object;

        // Act
        var firstMessages = await client.SimpleServerStream(new TestRequest()).ResponseStream.ReadAllAsync().ToArrayAsync();
        var secondMessages = await client.SimpleServerStream(new TestRequest()).ResponseStream.ReadAllAsync().ToArrayAsync();

        // Assert
        firstMessages.Should().BeEquivalentTo(firstExpectedResponses);
        secondMessages.Should().BeEquivalentTo(secondExpectedResponses);
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponseAndCallback(TestResponse[] expectedResponses)
    {
        // Arrange
        var flag = false;

        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default))
            .Callback(() => flag = true)
            .ReturnsAsync(expectedResponses);

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleServerStream(new TestRequest());
        var messages = await call.ResponseStream.ReadAllAsync().ToArrayAsync();

        // Assert
        messages.Should().BeEquivalentTo(expectedResponses);

        flag.Should().BeTrue();
    }

    [Theory, AutoData]
    public async Task SimpleServer_ShouldReturnResponse_WhenCalledByLambda(TestResponse[] expectedResponses)
    {
        // Arrange
        var grpcMock = new Mock<TestService.TestServiceClient>();
        grpcMock
            .Setup(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default))
            .ReturnsAsync(() => expectedResponses);

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
            .Setup(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default))
            .ReturnsAsync<TestService.TestServiceClient, TestRequest, TestResponse>(Map);

        var client = grpcMock.Object;

        // Act
        var call = client.SimpleServerStream(request);
        var messages = await call.ResponseStream.ReadAllAsync().ToArrayAsync();

        // Assert
        messages.Should().AllSatisfy(response => response.Val.Should().Be(request.Val));

        IEnumerable<TestResponse> Map(TestRequest r) => Enumerable.Repeat(new TestResponse { Val = r.Val }, 2);
    }
}
