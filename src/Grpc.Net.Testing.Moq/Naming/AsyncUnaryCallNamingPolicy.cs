using System;
using Grpc.Core;
using Grpc.Net.Testing.Moq.Extensions;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Naming;

public static class AsyncUnaryCallNamingPolicy
{
    public static IReturnsResult<T> Returns<T, TResponse>(
        this ISetup<T, AsyncUnaryCall<TResponse>> setup,
        TResponse response)
        where T : class
        => Returns(setup, () => response);

    public static IReturnsResult<T> Returns<T, TResponse>(
        this ISetup<T, AsyncUnaryCall<TResponse>> setup,
        Func<TResponse> func)
        where T : class
        => Returns<T, object, TResponse>(setup, _ => func());

    public static IReturnsResult<T> Returns<T, TRequest, TResponse>(
        this ISetup<T, AsyncUnaryCall<TResponse>> setup,
        Func<TRequest, TResponse> func)
        where T : class
        => setup.ReturnsAsync(func);
}
