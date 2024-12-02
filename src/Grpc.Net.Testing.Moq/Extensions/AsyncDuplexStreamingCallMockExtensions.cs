using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
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
                var requests = new List<TRequest>();
                var responses = new List<TResponse>();

                var writer = new Mock<IClientStreamWriter<TRequest>>();
                var reader = new Mock<IAsyncStreamReader<TResponse>>();

                writer
                    .Setup(c => c.WriteAsync(It.IsAny<TRequest>()))
                    .Callback((TRequest request) => requests.Add(request))
                    .Returns(Task.CompletedTask);
                writer
                    .Setup(c => c.CompleteAsync())
                    .Callback(() => responses.AddRange(func(requests)))
                    .Returns(Task.CompletedTask);

                var index = -1;

                reader
                    .Setup(c => c.MoveNext(It.IsAny<CancellationToken>()))
                    .Returns(() => Task.FromResult(++index < responses.Count));
                reader
                    .SetupGet(c => c.Current)
                    .Returns(() => responses[index]);

                return new AsyncDuplexStreamingCall<TRequest, TResponse>(
                    requestStream: writer.Object,
                    responseStream: reader.Object,
                    responseHeadersAsync: Task.FromResult(new Metadata()),
                    getStatusFunc: () => Status.DefaultSuccess,
                    getTrailersFunc: () => [],
                    disposeAction: () => { });
            });
}
