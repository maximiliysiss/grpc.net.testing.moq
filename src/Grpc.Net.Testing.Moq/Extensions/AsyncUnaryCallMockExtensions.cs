using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Moq.Language;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Extensions;

public static class AsyncUnaryCallMockExtensions
{
    public static IReturnsResult<T> ReturnsAsync<T, TResponse>(
        this IReturnsThrows<T, AsyncUnaryCall<TResponse>> setup,
        TResponse response)
        where T : class
        => ReturnsAsync(setup, () => response);

    public static IReturnsResult<T> ReturnsAsync<T, TResponse>(
        this IReturnsThrows<T, AsyncUnaryCall<TResponse>> setup,
        Func<TResponse> func)
        where T : class
        => ReturnsAsync<T, object, TResponse>(setup, _ => func());

    public static IReturnsResult<T> ReturnsAsync<T, TRequest, TResponse>(
        this IReturnsThrows<T, AsyncUnaryCall<TResponse>> setup,
        Func<TRequest, TResponse> func)
        where T : class
        => setup.Returns((TRequest r, Metadata? _, DateTime? _, CancellationToken _) => SimpleUnaryCall(func(r)));

    public static ISetupSequentialResult<AsyncUnaryCall<TResponse>> ReturnsAsync<TResponse>(
        this ISetupSequentialResult<AsyncUnaryCall<TResponse>> setup,
        TResponse response)
        => ReturnsAsync(setup, () => response);

    public static ISetupSequentialResult<AsyncUnaryCall<TResponse>> ReturnsAsync<TResponse>(
        this ISetupSequentialResult<AsyncUnaryCall<TResponse>> setup,
        Func<TResponse> func)
        => setup.Returns(() => SimpleUnaryCall(func()));

    private static AsyncUnaryCall<TResponse> SimpleUnaryCall<TResponse>(TResponse response) => new(
        responseAsync: Task.FromResult(response),
        responseHeadersAsync: Task.FromResult(new Metadata()),
        getStatusFunc: () => Status.DefaultSuccess,
        getTrailersFunc: () => [],
        disposeAction: () => { });
}
