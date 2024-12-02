using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
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
            (Metadata? _, DateTime? _, CancellationToken _) =>
            {
                var requests = new List<TRequest>();

                var taskCompletionSource = new TaskCompletionSource<TResponse>();

                var writer = new Mock<IClientStreamWriter<TRequest>>(MockBehavior.Strict);

                writer
                    .Setup(c => c.WriteAsync(It.IsAny<TRequest>()))
                    .Callback((TRequest request) => requests.Add(request))
                    .Returns(Task.CompletedTask);
                writer
                    .Setup(c => c.CompleteAsync())
                    .Callback(() => taskCompletionSource.SetResult(func(requests)))
                    .Returns(Task.CompletedTask);

                return new AsyncClientStreamingCall<TRequest, TResponse>(
                    requestStream: writer.Object,
                    responseAsync: taskCompletionSource.Task,
                    responseHeadersAsync: Task.FromResult(new Metadata()),
                    getStatusFunc: () => Status.DefaultSuccess,
                    getTrailersFunc: () => [],
                    disposeAction: () => { });
            });
}
