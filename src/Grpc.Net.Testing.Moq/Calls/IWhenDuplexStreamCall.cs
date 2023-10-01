using System;
using System.Collections.Generic;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Calls;

public interface IWhenDuplexStreamCall<TMock, TRequest, TResponse>
{
    IReturnsResult<TMock> Returns(params TResponse[] response);
    IReturnsResult<TMock> Returns(Func<IEnumerable<TResponse>> func);
    IReturnsResult<TMock> Returns(Func<IEnumerable<TRequest>, IEnumerable<TResponse>> func);
}
