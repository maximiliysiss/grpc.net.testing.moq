using System;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Calls;

public interface IWhenUnaryCall<TMock, TResponse>
{
    IReturnsResult<TMock> Returns(TResponse response);
    IReturnsResult<TMock> Returns(Func<TResponse> func);
    IReturnsResult<TMock> Returns<TRequest>(Func<TRequest, TResponse> func);
}
