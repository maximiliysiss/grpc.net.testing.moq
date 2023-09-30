using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;

namespace Grpc.Net.Testing.Moq.Shared;

public sealed class WhenClientStreamWriter<TRequest> : IClientStreamWriter<TRequest>
{
    private volatile bool _isCompleted;
    private readonly ConcurrentQueue<TRequest> _messages = new();

    public Task WriteAsync(TRequest message)
    {
        _messages.Enqueue(message);
        return Task.CompletedTask;
    }

    public WriteOptions? WriteOptions { get; set; }

    public Task CompleteAsync()
    {
        _isCompleted = true;
        return Task.CompletedTask;
    }

    public IEnumerable<TRequest> ReadAll()
    {
        while (!_messages.IsEmpty || !_isCompleted)
        {
            if (_messages.TryDequeue(out var message))
            {
                yield return message;
            }
        }
    }
}
