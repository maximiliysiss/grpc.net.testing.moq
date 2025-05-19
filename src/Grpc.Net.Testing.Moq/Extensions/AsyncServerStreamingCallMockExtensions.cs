using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using Moq.Language;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Extensions;

public static class AsyncServerStreamingCallMockExtensions
{
    public static IReturnsResult<T> ReturnsAsync<T, TResponse>(
        this IReturnsThrows<T, AsyncServerStreamingCall<TResponse>> setup,
        params TResponse[] response)
        where T : class
        => ReturnsAsync(setup, () => response);

    public static IReturnsResult<T> ReturnsAsync<T, TResponse>(
        this IReturnsThrows<T, AsyncServerStreamingCall<TResponse>> setup,
        Func<IEnumerable<TResponse>> func)
        where T : class
        => ReturnsAsync<T, object, TResponse>(setup, _ => func());

    public static IReturnsResult<T> ReturnsAsync<T, TRequest, TResponse>(
        this IReturnsThrows<T, AsyncServerStreamingCall<TResponse>> setup,
        Func<TRequest, IEnumerable<TResponse>> func)
        where T : class
        => setup.Returns((TRequest r, Metadata? _, DateTime? _, CancellationToken _) => SimpleServerStreamingCall(func(r)));

    public static ISetupSequentialResult<AsyncServerStreamingCall<TResponse>> ReturnsAsync<TResponse>(
        this ISetupSequentialResult<AsyncServerStreamingCall<TResponse>> setup,
        params TResponse[] response)
        => ReturnsAsync(setup, () => response);

    public static ISetupSequentialResult<AsyncServerStreamingCall<TResponse>> ReturnsAsync<TResponse>(
        this ISetupSequentialResult<AsyncServerStreamingCall<TResponse>> setup,
        Func<IEnumerable<TResponse>> func)
        => setup.Returns(() => SimpleServerStreamingCall(func()));

    private static AsyncServerStreamingCall<TResponse> SimpleServerStreamingCall<TResponse>(IEnumerable<TResponse> enumerable)
    {
        var enumerator = enumerable.ToList().GetEnumerator();

        var responseStream = new Mock<IAsyncStreamReader<TResponse>>(MockBehavior.Strict);

        responseStream
            .SetupGet(c => c.Current)
            .Returns(() => enumerator.Current);

        responseStream
            .Setup(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(enumerator.MoveNext()));

        return new AsyncServerStreamingCall<TResponse>(
            responseStream: responseStream.Object,
            responseHeadersAsync: Task.FromResult(new Metadata()),
            getStatusFunc: () => Status.DefaultSuccess,
            getTrailersFunc: () => [],
            disposeAction: () => { });
    }
}
