using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Testing.Moq.Shared;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Extensions;

public static class AsyncDuplexStreamingCallMockExtensions
{
    public static IReturnsResult<T> ReturnsAsync<T, TRequest, TResponse>(
        this ISetup<T, AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        params TResponse[] response)
        where T : class
        => ReturnsAsync(setup, () => response);

    public static IReturnsResult<T> ReturnsAsync<T, TRequest, TResponse>(
        this ISetup<T, AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        Func<IEnumerable<TResponse>> func)
        where T : class
        => ReturnsAsync(setup, _ => func());

    public static IReturnsResult<T> ReturnsAsync<T, TRequest, TResponse>(
        this ISetup<T, AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        Func<IEnumerable<TRequest>, IEnumerable<TResponse>> func)
        where T : class
        => setup.Returns(
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
