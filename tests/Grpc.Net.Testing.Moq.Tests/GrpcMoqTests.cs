using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Grpc.Core;
using Grpc.Core.Utils;
using Grpc.Net.Testing.Moq.Tests.Proto;
using Moq;
using Xunit;

namespace Grpc.Net.Testing.Moq.Tests
{
    public class GrpcMoqTests
    {
        [Fact]
        public void Simple_ShouldReturnResponse()
        {
            // Arrange 
            var grpcMock = new Mock<TestService.TestServiceClient>();
            grpcMock
                .Setup(c => c.Simple(It.IsAny<TestRequest>(), null, null, default))
                .Returns(new TestResponse());

            var client = grpcMock.Object;

            // Act
            var response = client.Simple(new TestRequest());

            // Assert
            response.Should().NotBeNull();
        }

        [Fact]
        public async Task SimpleAsync_ShouldReturnResponse()
        {
            // Arrange 
            var grpcMock = new Mock<TestService.TestServiceClient>();
            var testResponse = new TestResponse();

            grpcMock
                .Setup(c => c.SimpleAsync(It.IsAny<TestRequest>(), null, null, default))
                .Returns(testResponse);

            var client = grpcMock.Object;

            // Act
            var response = await client.SimpleAsync(new TestRequest());

            // Assert
            response.Should().NotBeNull();
            response.Should().BeSameAs(testResponse);
        }

        [Fact]
        public async Task SimpleClientStream_ShouldReturnResponse_ByEmptyClientStream()
        {
            // Arrange 
            var grpcMock = new Mock<TestService.TestServiceClient>();
            var testResponse = new TestResponse();

            grpcMock
                .Setup(c => c.SimpleClientStream(null, null, default))
                .Returns(testResponse);

            var client = grpcMock.Object;

            // Act
            var response = await client.SimpleClientStream();

            // Assert
            response.Should().NotBeNull();
            response.Should().BeSameAs(testResponse);
        }

        [Fact]
        public async Task SimpleClientStream_ShouldReturnResponse()
        {
            // Arrange 
            var grpcMock = new Mock<TestService.TestServiceClient>();
            var testResponse = new TestResponse();

            grpcMock
                .Setup(c => c.SimpleClientStream(null, null, default))
                .Returns(testResponse);

            var client = grpcMock.Object;

            // Act
            var stream = client.SimpleClientStream();
            await stream.RequestStream.WriteAllAsync(new[] {new TestRequest()});
            var response = await stream;

            // Assert
            response.Should().NotBeNull();
            response.Should().BeSameAs(testResponse);
        }

        [Theory]
        [MemberData(nameof(SampleData))]
        public async Task SimpleServerStream_ShouldReturnResponse(TestResponse[] testResponse)
        {
            // Arrange 
            var grpcMock = new Mock<TestService.TestServiceClient>();

            grpcMock
                .Setup(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default))
                .Returns(testResponse);

            var client = grpcMock.Object;

            // Act
            var stream = client.SimpleServerStream(new TestRequest());

            // Assert
            var responses = await stream.ResponseStream.ReadAllAsync().ToArrayAsync();

            responses.Should().HaveCount(testResponse.Length);

            foreach (var response in responses)
                response.Should().NotBeNull();
        }

        [Theory]
        [MemberData(nameof(SampleData))]
        public async Task SimpleClientServerStream_ShouldReturnResponse_ByEmptyTestRequest(TestResponse[] testResponse)
        {
            // Arrange 
            var grpcMock = new Mock<TestService.TestServiceClient>();

            grpcMock
                .Setup(c => c.SimpleClientServerStream(null, null, default))
                .Returns(testResponse);

            var client = grpcMock.Object;

            // Act
            var stream = client.SimpleClientServerStream();

            // Assert
            var responses = await stream.ResponseStream.ReadAllAsync().ToArrayAsync();

            responses.Should().HaveCount(testResponse.Length);

            foreach (var response in responses)
                response.Should().NotBeNull();
        }

        [Theory]
        [MemberData(nameof(SampleData))]
        public async Task SimpleClientServerStream_ShouldReturnResponse(TestResponse[] testResponse)
        {
            // Arrange 
            var grpcMock = new Mock<TestService.TestServiceClient>();

            grpcMock
                .Setup(c => c.SimpleClientServerStream(null, null, default))
                .Returns(testResponse);

            var client = grpcMock.Object;

            // Act
            var stream = client.SimpleClientServerStream();
            await stream.RequestStream.WriteAsync(new TestRequest());

            // Assert
            var responses = await stream.ResponseStream.ReadAllAsync().ToArrayAsync();

            responses.Should().HaveCount(testResponse.Length);

            foreach (var response in responses)
                response.Should().NotBeNull();
        }

        public static IEnumerable<object[]> SampleData()
        {
            yield return new object[] {Array.Empty<TestResponse>()};
            yield return new object[] {new[] {new TestResponse()}};
            yield return new object[] {new[] {new TestResponse(), new TestResponse()}};
        }
    }
}