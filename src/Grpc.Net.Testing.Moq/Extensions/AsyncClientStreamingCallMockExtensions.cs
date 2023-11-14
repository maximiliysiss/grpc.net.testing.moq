using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Testing.Moq.Shared;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Extensions;

public static class AsyncClientStreamingCallMockExtensions
{
    public static IReturnsResult<T> ReturnsAsync<T, TRequest, TResponse>(
        this ISetup<T, AsyncClientStreamingCall<TRequest, TResponse>> setup,
        TResponse response)
        where T : class
        => ReturnsAsync(setup, () => response);

    public static IReturnsResult<T> ReturnsAsync<T, TRequest, TResponse>(
        this ISetup<T, AsyncClientStreamingCall<TRequest, TResponse>> setup,
        Func<TResponse> func)
        where T : class
        => ReturnsAsync(setup, _ => func());

    public static IReturnsResult<T> ReturnsAsync<T, TRequest, TResponse>(
        this ISetup<T, AsyncClientStreamingCall<TRequest, TResponse>> setup,
        Func<IEnumerable<TRequest>, TResponse> func)
        where T : class
        => setup.Returns(
            (Metadata? _, DateTime? _, CancellationToken ct) =>
            {
                var requestStream = new WhenStreamWriter<TRequest>();
                var stream = requestStream.ReadAll();
                var responseTask = Task.Run(() => func(stream), ct);

                var fakeCall = new AsyncClientStreamingCall<TRequest, TResponse>(
                    requestStream,
                    responseTask,
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { });

                return fakeCall;
            });
}
