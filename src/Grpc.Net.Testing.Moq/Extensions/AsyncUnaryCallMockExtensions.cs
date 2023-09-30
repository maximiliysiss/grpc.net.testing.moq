using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Testing.Moq.Calls;
using Moq;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Extensions;

public static class AsyncUnaryCallMockExtensions
{
    public static ISetup<TMock, TResponse> When<TMock, TResponse>(
        this Mock<TMock> mock,
        Expression<Func<TMock, TResponse>> expression)
        where TMock : class
        => mock.Setup(expression);

    public static IWhenUnaryCall<TMock, TResponse> When<TMock, TResponse>(
        this Mock<TMock> mock,
        Expression<Func<TMock, AsyncUnaryCall<TResponse>>> expression)
        where TMock : class
    {
        var setup = mock.Setup(expression);
        return new WhenUnaryAsyncUnaryCall<TMock, TResponse>(setup);
    }

    private sealed class WhenUnaryAsyncUnaryCall<TMock, TResponse> : IWhenUnaryCall<TMock, TResponse> where TMock : class
    {
        private readonly ISetup<TMock, AsyncUnaryCall<TResponse>> _setup;

        public WhenUnaryAsyncUnaryCall(ISetup<TMock, AsyncUnaryCall<TResponse>> setup) => _setup = setup;

        public IReturnsResult<TMock> Returns(TResponse response) => Returns(() => response);
        public IReturnsResult<TMock> Returns(Func<TResponse> func) => Returns<object>(_ => func());

        public IReturnsResult<TMock> Returns<TRequest>(Func<TRequest, TResponse> func)
            => _setup.Returns(
                (TRequest r, Metadata? _, DateTime? _, CancellationToken _) => new AsyncUnaryCall<TResponse>(
                    Task.FromResult(func(r)),
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { }));
    }
}
