using System;
using System.Collections.Generic;
using Grpc.Core;
using Grpc.Net.Testing.Moq.Extensions;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Naming;

public static class AsyncClientStreamingCallNamingPolicy
{
    public static IReturnsResult<T> Returns<T, TRequest, TResponse>(
        this ISetup<T, AsyncClientStreamingCall<TRequest, TResponse>> setup,
        TResponse response)
        where T : class
        => Returns(setup, () => response);

    /// <summary>
    /// Use only like `AsyncClientStreamingCallNamingPolicy.Returns`
    /// </summary>
    public static IReturnsResult<T> Returns<T, TRequest, TResponse>(
        this ISetup<T, AsyncClientStreamingCall<TRequest, TResponse>> setup,
        Func<TResponse> func)
        where T : class
        => Returns(setup, _ => func());

    public static IReturnsResult<T> Returns<T, TRequest, TResponse>(
        this ISetup<T, AsyncClientStreamingCall<TRequest, TResponse>> setup,
        Func<IEnumerable<TRequest>, TResponse> func)
        where T : class
        => setup.ReturnsAsync(func);
}
