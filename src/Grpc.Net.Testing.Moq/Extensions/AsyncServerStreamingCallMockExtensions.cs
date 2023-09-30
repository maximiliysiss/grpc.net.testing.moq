using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Testing.Moq.Calls;
using Moq;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Extensions;

public static class AsyncServerStreamingCallMockExtensions
{
    public static IWhenServerStreamCall<TMock, TResponse> When<TMock, TResponse>(
        this Mock<TMock> mock,
        Expression<Func<TMock, AsyncServerStreamingCall<TResponse>>> expression)
        where TMock : class
    {
        var setup = mock.Setup(expression);
        return new WhenUnaryAsyncServerStreamingCall<TMock, TResponse>(setup);
    }

    private sealed class WhenUnaryAsyncServerStreamingCall<TMock, TResponse> : IWhenServerStreamCall<TMock, TResponse> where TMock : class
    {
        private readonly ISetup<TMock, AsyncServerStreamingCall<TResponse>> _setup;

        public WhenUnaryAsyncServerStreamingCall(ISetup<TMock, AsyncServerStreamingCall<TResponse>> setup) => _setup = setup;

        public IReturnsResult<TMock> Returns(params TResponse[] response) => Returns(() => response);
        public IReturnsResult<TMock> Returns(Func<IEnumerable<TResponse>> func) => Returns<object>(_ => func());

        public IReturnsResult<TMock> Returns<TRequest>(Func<TRequest, IEnumerable<TResponse>> func) => _setup.Returns(
            (TRequest r, Metadata? _, DateTime? _, CancellationToken _) =>
            {
                var mockResponseStream = new Mock<IAsyncStreamReader<TResponse>>();
                var responses = func(r);

                var sequentialCurrent = mockResponseStream.SetupSequence(s => s.Current);
                var sequentialMoveNext = mockResponseStream.SetupSequence(s => s.MoveNext(It.IsAny<CancellationToken>()));

                foreach (var response in responses.ToArray())
                {
                    sequentialCurrent.Returns(response);
                    sequentialMoveNext.Returns(Task.FromResult(true));
                }

                sequentialMoveNext.Returns(Task.FromResult(false));

                var fakeCall = new AsyncServerStreamingCall<TResponse>(
                    mockResponseStream.Object,
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { });

                return fakeCall;
            });
    }
}
