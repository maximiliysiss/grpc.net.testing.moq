using System;
using System.Collections.Generic;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Calls;

public interface IWhenServerStreamCall<TMock, TResponse>
{
    IReturnsResult<TMock> Returns(params TResponse[] response);
    IReturnsResult<TMock> Returns(Func<IEnumerable<TResponse>> func);
    IReturnsResult<TMock> Returns<TRequest>(Func<TRequest, IEnumerable<TResponse>> func);
}
