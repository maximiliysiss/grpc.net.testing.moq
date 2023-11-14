using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Extensions;

public static class AsyncUnaryCallMockExtensions
{
    public static IReturnsResult<T> ReturnsAsync<T, TResponse>(
        this ISetup<T, AsyncUnaryCall<TResponse>> setup,
        TResponse response)
        where T : class
        => ReturnsAsync(setup, () => response);

    public static IReturnsResult<T> ReturnsAsync<T, TResponse>(
        this ISetup<T, AsyncUnaryCall<TResponse>> setup,
        Func<TResponse> func)
        where T : class
        => ReturnsAsync<T, object, TResponse>(setup, _ => func());

    public static IReturnsResult<T> ReturnsAsync<T, TRequest, TResponse>(
        this ISetup<T, AsyncUnaryCall<TResponse>> setup,
        Func<TRequest, TResponse> func)
        where T : class
        => setup.Returns(
            (TRequest r, Metadata? _, DateTime? _, CancellationToken _) => new AsyncUnaryCall<TResponse>(
                Task.FromResult(func(r)),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { }));
}
