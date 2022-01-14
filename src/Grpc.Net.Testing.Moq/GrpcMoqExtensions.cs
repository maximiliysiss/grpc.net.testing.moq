using System;
using System.Collections.Generic;
using System.Linq;
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
        public static IReturnsResult<T> Returns<T, TResponse>(
            this ISetup<T, AsyncUnaryCall<TResponse>> setup,
            TResponse response)
            where T : class
            => setup.Returns(() => response);

        public static IReturnsResult<T> Returns<T, TResponse>(
            this ISetup<T, AsyncUnaryCall<TResponse>> setup,
            Func<TResponse> responseFunc)
            where T : class
        {
            var fakeCall = TestCalls.AsyncUnaryCall(
                Task.FromResult(responseFunc()),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            return setup.Returns(fakeCall);
        }

        public static IReturnsResult<T> Returns<T, TRequest, TResponse>(
            this ISetup<T, AsyncClientStreamingCall<TRequest, TResponse>> setup,
            TResponse response)
            where T : class
            => setup.Returns(() => response);

        public static IReturnsResult<T> Returns<T, TRequest, TResponse>(
            this ISetup<T, AsyncClientStreamingCall<TRequest, TResponse>> setup,
            Func<TResponse> responseFunc)
            where T : class
        {
            var mockRequestStream = new Mock<IClientStreamWriter<TRequest>>();

            var fakeCall = TestCalls.AsyncClientStreamingCall(
                mockRequestStream.Object,
                Task.FromResult(responseFunc()),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(), () => { });

            return setup.Returns(fakeCall);
        }

        public static IReturnsResult<T> Returns<T, TResponse>(
            this ISetup<T, AsyncServerStreamingCall<TResponse>> setup,
            IEnumerable<TResponse> responses)
            where T : class
            => setup.Returns(() => responses);

        public static IReturnsResult<T> Returns<T, TResponse>(
            this ISetup<T, AsyncServerStreamingCall<TResponse>> setup,
            Func<IEnumerable<TResponse>> responsesFunc)
            where T : class
        {
            var mockResponseStream = new Mock<IAsyncStreamReader<TResponse>>();
            var responses = responsesFunc();

            var sequentialCurrent = mockResponseStream.SetupSequence(s => s.Current);
            var sequentialMoveNext = mockResponseStream.SetupSequence(s => s.MoveNext(It.IsAny<CancellationToken>()));

            foreach (var response in responses.ToArray())
            {
                sequentialCurrent.Returns(response);
                sequentialMoveNext.Returns(Task.FromResult(true));
            }

            sequentialMoveNext.Returns(Task.FromResult(false));

            var fakeCall = TestCalls.AsyncServerStreamingCall(
                mockResponseStream.Object,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            return setup.Returns(fakeCall);
        }

        public static IReturnsResult<T> Returns<T, TRequest, TResponse>(
            this ISetup<T, AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
            IEnumerable<TResponse> responses)
            where T : class
            => setup.Returns(() => responses);

        public static IReturnsResult<T> Returns<T, TRequest, TResponse>(
            this ISetup<T, AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
            Func<IEnumerable<TResponse>> responsesFunc)
            where T : class
        {
            var mockResponseStream = new Mock<IAsyncStreamReader<TResponse>>();
            var mockRequestStream = new Mock<IClientStreamWriter<TRequest>>();

            var responses = responsesFunc();

            var sequentialCurrent = mockResponseStream.SetupSequence(s => s.Current);
            var sequentialMoveNext = mockResponseStream.SetupSequence(s => s.MoveNext(It.IsAny<CancellationToken>()));

            foreach (var response in responses.ToArray())
            {
                sequentialCurrent.Returns(response);
                sequentialMoveNext.Returns(Task.FromResult(true));
            }

            sequentialMoveNext.Returns(Task.FromResult(false));

            var fakeCall = TestCalls.AsyncDuplexStreamingCall(
                mockRequestStream.Object,
                mockResponseStream.Object,
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });

            return setup.Returns(fakeCall);
        }
    }
}