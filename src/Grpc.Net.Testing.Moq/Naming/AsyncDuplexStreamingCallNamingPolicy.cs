using System;
using System.Collections.Generic;
using Grpc.Core;
using Grpc.Net.Testing.Moq.Extensions;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Naming;

public static class AsyncDuplexStreamingCallNamingPolicy
{
    public static IReturnsResult<T> Returns<T, TRequest, TResponse>(
        this ISetup<T, AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        params TResponse[] response)
        where T : class
        => Returns(setup, () => response);

    public static IReturnsResult<T> Returns<T, TRequest, TResponse>(
        this ISetup<T, AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        Func<IEnumerable<TResponse>> func)
        where T : class
        => Returns(setup, _ => func());

    public static IReturnsResult<T> Returns<T, TRequest, TResponse>(
        this ISetup<T, AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        Func<IEnumerable<TRequest>, IEnumerable<TResponse>> func)
        where T : class
        => setup.ReturnsAsync(func);
}
