using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using Moq.Language;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Extensions;

public static class AsyncDuplexStreamingCallMockExtensions
{
    public static IReturnsResult<T> ReturnsAsync<T, TRequest, TResponse>(
        this IReturnsThrows<T, AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        params TResponse[] response)
        where T : class
        => ReturnsAsync(setup, () => response);

    public static IReturnsResult<T> ReturnsAsync<T, TRequest, TResponse>(
        this IReturnsThrows<T, AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        Func<IEnumerable<TResponse>> func)
        where T : class
        => ReturnsAsync(setup, _ => func());

    public static IReturnsResult<T> ReturnsAsync<T, TRequest, TResponse>(
        this IReturnsThrows<T, AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        Func<IEnumerable<TRequest>, IEnumerable<TResponse>> func)
        where T : class
        => setup.Returns((Metadata? _, DateTime? _, CancellationToken _) => SimpleDuplexStreamingCall(func));

    public static IReturnsResult<T> ReturnsAsync<T, TRequest, TResponse>(
        this IReturnsThrows<T, AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        Func<TRequest, TResponse> func)
        where T : class
        => setup.Returns((Metadata? _, DateTime? _, CancellationToken _) => SimpleDuplexStreamingReactiveCall(func));

    public static ISetupSequentialResult<AsyncDuplexStreamingCall<TRequest, TResponse>> ReturnsAsync<TRequest, TResponse>(
        this ISetupSequentialResult<AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        params TResponse[] response)
        => ReturnsAsync(setup, () => response);

    public static ISetupSequentialResult<AsyncDuplexStreamingCall<TRequest, TResponse>> ReturnsAsync<TRequest, TResponse>(
        this ISetupSequentialResult<AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        Func<IEnumerable<TResponse>> func)
        => ReturnsAsync(setup, _ => func());

    public static ISetupSequentialResult<AsyncDuplexStreamingCall<TRequest, TResponse>> ReturnsAsync<TRequest, TResponse>(
        this ISetupSequentialResult<AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        Func<IEnumerable<TRequest>, IEnumerable<TResponse>> func)
        => setup.Returns(() => SimpleDuplexStreamingCall(func));

    public static ISetupSequentialResult<AsyncDuplexStreamingCall<TRequest, TResponse>> ReturnsAsync<TRequest, TResponse>(
        this ISetupSequentialResult<AsyncDuplexStreamingCall<TRequest, TResponse>> setup,
        Func<TRequest, TResponse> func)
        => setup.Returns(() => SimpleDuplexStreamingReactiveCall(func));

    private static AsyncDuplexStreamingCall<TRequest, TResponse> SimpleDuplexStreamingCall<TRequest, TResponse>(
        Func<IEnumerable<TRequest>, IEnumerable<TResponse>> func)
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
    }

    private static AsyncDuplexStreamingCall<TRequest, TResponse> SimpleDuplexStreamingReactiveCall<TRequest, TResponse>(
        Func<TRequest, TResponse> func)
    {
        var (responses, ended) = (new Queue<TResponse>(), false);
        var source = new TaskCompletionSource<bool>();

        var writer = new Mock<IClientStreamWriter<TRequest>>();
        var reader = new Mock<IAsyncStreamReader<TResponse>>();

        writer
            .Setup(c => c.WriteAsync(It.IsAny<TRequest>()))
            .Callback(Handle)
            .Returns(Task.CompletedTask);

        writer
            .Setup(c => c.CompleteAsync())
            .Callback(Complete)
            .Returns(Task.CompletedTask);

        reader
            .Setup(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(WaitNext);

        reader
            .SetupGet(c => c.Current)
            .Returns(() => responses.Dequeue());

        return new AsyncDuplexStreamingCall<TRequest, TResponse>(
            requestStream: writer.Object,
            responseStream: reader.Object,
            responseHeadersAsync: Task.FromResult(new Metadata()),
            getStatusFunc: () => Status.DefaultSuccess,
            getTrailersFunc: () => [],
            disposeAction: () => { });

        void Handle(TRequest request)
        {
            responses.Enqueue(func(request));
            source.TrySetResult(true);
        }

        void Complete()
        {
            ended = true;
            source.TrySetResult(false);
        }

        async Task<bool> WaitNext()
        {
            while (responses.Count is 0 && ended is false)
            {
                await source.Task;
                source = new TaskCompletionSource<bool>();
            }

            return responses.Count > 0;
        }
    }
}
