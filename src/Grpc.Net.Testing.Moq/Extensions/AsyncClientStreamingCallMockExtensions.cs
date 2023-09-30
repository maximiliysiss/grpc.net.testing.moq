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

public static class AsyncClientStreamingCallMockExtensions
{
    public static IWhenClientStreamCall<TMock, TRequest, TResponse> When<TMock, TRequest, TResponse>(
        this Mock<TMock> mock,
        Expression<Func<TMock, AsyncClientStreamingCall<TRequest, TResponse>>> expression)
        where TMock : class
    {
        var setup = mock.Setup(expression);
        return new WhenClientStreamCall<TMock, TRequest, TResponse>(setup);
    }

    private sealed class WhenClientStreamCall<TMock, TRequest, TResponse> : IWhenClientStreamCall<TMock, TRequest, TResponse>
        where TMock : class
    {
        private readonly ISetup<TMock, AsyncClientStreamingCall<TRequest, TResponse>> _setup;

        public WhenClientStreamCall(ISetup<TMock, AsyncClientStreamingCall<TRequest, TResponse>> setup) => _setup = setup;

        public IReturnsResult<TMock> Returns(TResponse response) => Returns(() => response);

        public IReturnsResult<TMock> Returns(Func<TResponse> func) => Returns(_ => func());

        public IReturnsResult<TMock> Returns(Func<IEnumerable<TRequest>, TResponse> func) => _setup.Returns(
            (Metadata? _, DateTime? _, CancellationToken ct) =>
            {
                var mockRequestStream = new WhenClientStreamWriter<TRequest>();
                var stream = mockRequestStream.ReadAll();
                var responseTask = Task.Run(() => func(stream), ct);

                var fakeCall = new AsyncClientStreamingCall<TRequest, TResponse>(
                    mockRequestStream,
                    responseTask,
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { });

                return fakeCall;
            });
    }
}
