using System;
using System.Collections.Generic;
using Grpc.Core;
using Grpc.Net.Testing.Moq.Extensions;
using Moq.Language;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Naming;

public static class AsyncDuplexStreamingCallNamingPolicy
{
    public static IReturnsResult<T> Returns<T, TRequest, TResponse>(
        this IReturnsThrows<T, AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        params TResponse[] response)
        where T : class
        => Returns(setup, () => response);

    /// <summary>
    /// Use only like `AsyncDuplexStreamingCallNamingPolicy.Returns`
    /// </summary>
    public static IReturnsResult<T> Returns<T, TRequest, TResponse>(
        this IReturnsThrows<T, AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        Func<IEnumerable<TResponse>> func)
        where T : class
        => Returns(setup, _ => func());

    public static IReturnsResult<T> Returns<T, TRequest, TResponse>(
        this IReturnsThrows<T, AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        Func<IEnumerable<TRequest>, IEnumerable<TResponse>> func)
        where T : class
        => setup.ReturnsAsync(func);

    public static IReturnsResult<T> Returns<T, TRequest, TResponse>(
        this IReturnsThrows<T, AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        Func<TRequest, TResponse> func)
        where T : class
        => setup.ReturnsAsync(func);

    public static ISetupSequentialResult<AsyncDuplexStreamingCall<TRequest, TResponse>> Returns<TRequest, TResponse>(
        this ISetupSequentialResult<AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        params TResponse[] response)
        => setup.ReturnsAsync(response);

    public static ISetupSequentialResult<AsyncDuplexStreamingCall<TRequest, TResponse>> Returns<TRequest, TResponse>(
        this ISetupSequentialResult<AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        Func<IEnumerable<TResponse>> func)
        => setup.ReturnsAsync(func);

    public static ISetupSequentialResult<AsyncDuplexStreamingCall<TRequest, TResponse>> Returns<TRequest, TResponse>(
        this ISetupSequentialResult<AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        Func<IEnumerable<TRequest>, IEnumerable<TResponse>> func)
        => setup.ReturnsAsync(func);

    public static ISetupSequentialResult<AsyncDuplexStreamingCall<TRequest, TResponse>> Returns<TRequest, TResponse>(
        this ISetupSequentialResult<AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        Func<TRequest, TResponse> func)
        => setup.ReturnsAsync(func);
}
