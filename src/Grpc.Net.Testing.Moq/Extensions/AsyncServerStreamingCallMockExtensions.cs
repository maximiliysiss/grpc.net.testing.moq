using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Extensions;

public static class AsyncServerStreamingCallMockExtensions
{
    public static IReturnsResult<T> ReturnsAsync<T, TResponse>(
        this ISetup<T, AsyncServerStreamingCall<TResponse>> setup,
        params TResponse[] response)
        where T : class
        => ReturnsAsync(setup, () => response);

    public static IReturnsResult<T> ReturnsAsync<T, TResponse>(
        this ISetup<T, AsyncServerStreamingCall<TResponse>> setup,
        Func<IEnumerable<TResponse>> func)
        where T : class
        => ReturnsAsync<T, object, TResponse>(setup, _ => func());

    public static IReturnsResult<T> ReturnsAsync<T, TRequest, TResponse>(
        this ISetup<T, AsyncServerStreamingCall<TResponse>> setup,
        Func<TRequest, IEnumerable<TResponse>> func)
        where T : class
        => setup.Returns(
            (TRequest r, Metadata? _, DateTime? _, CancellationToken _) =>
            {
                var enumerator = func(r).ToList().GetEnumerator();

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
            });
}
