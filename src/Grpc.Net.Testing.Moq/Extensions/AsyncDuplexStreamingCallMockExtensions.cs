using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Testing.Moq.Calls;
using Grpc.Net.Testing.Moq.Shared;
using Moq;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Extensions;

public static class AsyncDuplexStreamingCallMockExtensions
{
    public static IWhenDuplexStreamCall<TMock, TRequest, TResponse> When<TMock, TRequest, TResponse>(
        this Mock<TMock> mock,
        Expression<Func<TMock, AsyncDuplexStreamingCall<TRequest, TResponse>>> expression)
        where TMock : class
    {
        var setup = mock.Setup(expression);
        return new WhenAsyncDuplexStreamingCall<TMock, TRequest, TResponse>(setup);
    }

    private sealed class WhenAsyncDuplexStreamingCall<TMock, TRequest, TResponse> : IWhenDuplexStreamCall<TMock, TRequest, TResponse>
        where TMock : class
    {
        private readonly ISetup<TMock, AsyncDuplexStreamingCall<TRequest, TResponse>> _setup;

        public WhenAsyncDuplexStreamingCall(ISetup<TMock, AsyncDuplexStreamingCall<TRequest, TResponse>> setup) => _setup = setup;

        public IReturnsResult<TMock> Returns(params TResponse[] response) => Returns(() => response);

        public IReturnsResult<TMock> Returns(Func<IEnumerable<TResponse>> func) => Returns(_ => func());

        public IReturnsResult<TMock> Returns(Func<IEnumerable<TRequest>, IEnumerable<TResponse>> func) => _setup.Returns(
            (Metadata? _, DateTime? _, CancellationToken _) =>
            {
                var requestStream = new WhenStreamWriter<TRequest>();
                var handler = () => func(requestStream.ReadAll());
                var responseStream = new WhenStreamReader<TResponse>(handler);

                var fakeCall = new AsyncDuplexStreamingCall<TRequest, TResponse>(
                    requestStream,
                    responseStream,
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { });

                return fakeCall;
            });
    }
}
