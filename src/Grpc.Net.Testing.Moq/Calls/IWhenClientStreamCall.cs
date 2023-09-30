using System;
using System.Collections.Generic;
using Moq.Language.Flow;

namespace Grpc.Net.Testing.Moq.Calls;

public interface IWhenClientStreamCall<TMock, TRequest, TResponse>
{
    IReturnsResult<TMock> Returns(TResponse response);
    IReturnsResult<TMock> Returns(Func<TResponse> func);
    IReturnsResult<TMock> Returns(Func<IEnumerable<TRequest>, TResponse> func);
}
