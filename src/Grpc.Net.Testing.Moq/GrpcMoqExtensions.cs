using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Testing;
using Moq;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq
{
    public static class GrpcMoqExtensions
    {
        public static ISetup<T, TResult> Setup<T, TResult>(this Mock<T> mock, Expression<Func<T, TResult>> expression)
            where T : class
            => mock.Setup(expression);

        public static IReturnsResult<T> Setup<T, TResponse>(
            this Mock<T> mock,
            Expression<Func<T, AsyncUnaryCall<TResponse>>> expression,
            TResponse response)
            where T : class
        {
            var fakeCall = TestCalls.AsyncUnaryCall(
                Task.FromResult(response),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            return mock
                .Setup(expression)
                .Returns(fakeCall);
        }

        public static IReturnsResult<T> Setup<T, TRequest, TResponse>(
            this Mock<T> mock,
            Expression<Func<T, AsyncClientStreamingCall<TRequest, TResponse>>> expression,
            TResponse response)
            where T : class
        {
            var mockRequestStream = new Mock<IClientStreamWriter<TRequest>>();

            var fakeCall = TestCalls.AsyncClientStreamingCall(
                mockRequestStream.Object,
                Task.FromResult(response),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(), () => { });

            return mock.Setup(expression).Returns(fakeCall);
        }

        public static IReturnsResult<T> Setup<T, TResponse>(
            this Mock<T> mock,
            Expression<Func<T, AsyncServerStreamingCall<TResponse>>> expression,
            params TResponse[] responses)
            where T : class
        {
            var mockResponseStream = new Mock<IAsyncStreamReader<TResponse>>();

            for (var i = 0; i < responses.Length; i++)
            {
                mockResponseStream.SetupSequence(s => s.Current).Returns(responses[i]);
                mockResponseStream
                    .SetupSequence(s => s.MoveNext(It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(i < responses.Length - 1));
            }

            if (responses.Length == 0)
            {
                mockResponseStream
                    .SetupSequence(s => s.MoveNext(It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(false));
            }

            var fakeCall = TestCalls.AsyncServerStreamingCall(
                mockResponseStream.Object,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            return mock.Setup(expression).Returns(fakeCall);
        }

        public static IReturnsResult<T> Setup<T, TRequest, TResponse>(
            this Mock<T> mock,
            Expression<Func<T, AsyncDuplexStreamingCall<TRequest, TResponse>>> expression,
            params TResponse[] responses)
            where T : class
        {
            var mockResponseStream = new Mock<IAsyncStreamReader<TResponse>>();
            var mockRequestStream = new Mock<IClientStreamWriter<TRequest>>();

            for (var i = 0; i < responses.Length; i++)
            {
                mockResponseStream.SetupSequence(s => s.Current).Returns(responses[i]);
                mockResponseStream
                    .SetupSequence(s => s.MoveNext(It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(i < responses.Length - 1));
            }

            if (responses.Length == 0)
            {
                mockResponseStream
                    .SetupSequence(s => s.MoveNext(It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(false));
            }

            var fakeCall = TestCalls.AsyncDuplexStreamingCall(
                mockRequestStream.Object,
                mockResponseStream.Object,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            return mock.Setup(expression).Returns(fakeCall);
        }
    }
}