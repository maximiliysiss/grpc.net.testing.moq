using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace Grpc.Net.Testing.Moq.Shared;

public sealed class WhenStreamReader<TResponse> : IAsyncStreamReader<TResponse>
{
    private readonly Func<IEnumerable<TResponse>> _source;

    private IEnumerator<TResponse>? _enumerator;

    public WhenStreamReader(Func<IEnumerable<TResponse>> source) => _source = source;

    public Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        PreInit();
        return Task.FromResult(_enumerator!.MoveNext());
    }

    private void PreInit() => _enumerator ??= _source().GetEnumerator();

    public TResponse Current => _enumerator!.Current;
}
