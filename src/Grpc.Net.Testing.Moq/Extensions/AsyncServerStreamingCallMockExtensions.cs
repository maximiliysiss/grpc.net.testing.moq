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
                var mockResponseStream = new Mock<IAsyncStreamReader<TResponse>>();
                var responses = func(r);

                var sequentialCurrent = mockResponseStream.SetupSequence(s => s.Current);
                var sequentialMoveNext = mockResponseStream.SetupSequence(s => s.MoveNext(It.IsAny<CancellationToken>()));

                foreach (var response in responses.ToArray())
                {
                    sequentialCurrent.Returns(response);
                    sequentialMoveNext.Returns(Task.FromResult(true));
                }

                sequentialMoveNext.Returns(Task.FromResult(false));

                var fakeCall = new AsyncServerStreamingCall<TResponse>(
                    mockResponseStream.Object,
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { });

                return fakeCall;
            });
}
