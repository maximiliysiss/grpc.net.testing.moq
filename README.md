# grpc.net.testing.moq

[![.NET](https://github.com/maximiliysiss/grpc.net.testing.moq/actions/workflows/dotnet.yml/badge.svg?branch=master&event=push)](https://github.com/maximiliysiss/grpc.net.testing.moq/actions/workflows/dotnet.yml)

Library to mocking gRPC client. Instead of `Grpc.Core.Testing` using extensions for `Moq`

Based on libraries:

* [Moq](https://www.nuget.org/packages/Moq)

## Install

### Nuget:

`Install-Package Net.Testing.Moq.Grpc`

## Using example

> There is naming policy aka `.Setup(...).Returns(...)`, but it cannot use with Delegates (only for values computed
> outside)

### Simple sync calling:

#### 1. Call with exists response

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();

// Setup and set return
grpcMock
    .Setup(c => c.Simple(It.IsAny<TestRequest>(), null, null, default))
    .ReturnsAsync(new TestResponse());

var client = grpcMock.Object;

// Call
var response = client.Simple(new TestRequest());
```

### Simple async calling:

#### 1. Call with exists response

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new TestResponse();

// Setup and set return
grpcMock
    .Setup(c => c.SimpleAsync(It.IsAny<TestRequest>(), null, null, default))
    .ReturnsAsync(testResponse);

var client = grpcMock.Object;

// Call
var response = await client.SimpleAsync(new TestRequest());
```

#### 2. Call with response creating on call

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();

// Setup and set return by lambda
grpcMock
    .Setup(c => c.SimpleAsync(It.IsAny<TestRequest>(), null, null, default))
    .ReturnsAsync(() => new TestResponse());

var client = grpcMock.Object;

// Call
var response = client.SimpleAsync(new TestRequest());
```

#### 3. Call with response creating on call and based on request

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();

// Setup and set return by lambda with request param
grpcMock
    .Setup(c => c.SimpleAsync(It.IsAny<TestRequest>(), null, null, default))
    .ReturnsAsync<TestService.TestServiceClient, TestRequest, TestResponse>(r => new TestResponse{ Val = r.Val });

var client = grpcMock.Object;

// Call
var response = client.SimpleAsync(new TestRequest());
```

#### 4. Call with sequential results

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();

// Setup
grpcMock
    .SetupSequence(c => c.SimpleAsync(It.IsAny<TestRequest>(), null, null, default))
    .ReturnsAsync(firstExpectedResponse)
    .ReturnsAsync(secondExpectedResponse);

var client = grpcMock.Object;

// Call
var firstResponse = await client.SimpleAsync(new TestRequest());
var secondResponse = await client.SimpleAsync(new TestRequest());
```

### Simple client stream calling:

#### 1. Call with exists response

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new TestResponse();

// Setup and set response
grpcMock
    .Setup(c => c.SimpleClientStream(null, null, default))
    .ReturnsAsync(testResponse);

var client = grpcMock.Object;

// Call
var stream = client.SimpleClientStream();
await stream.RequestStream.WriteAllAsync(new[] {new TestRequest()}, true);
var response = await stream;
```

#### 2. Call with response creating on call

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new TestResponse();

// Setup and set response by lambda
grpcMock
    .Setup(c => c.SimpleClientStream(null, null, default))
    .ReturnsAsync(() => testResponse);

var client = grpcMock.Object;

// Call
var stream = client.SimpleClientStream();
await stream.RequestStream.WriteAllAsync(new[] {new TestRequest()}, true);
var response = await stream;
```

#### 3. Call with response creating on call and based on request

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();

// Setup and set response by lambda and requests like param
grpcMock
    .Setup(c => c.SimpleClientStream(null, null, default))
    .ReturnsAsync(rs => new TestResponse{ Val = rs.Sum(r => r.Val) });

var client = grpcMock.Object;

// Call
var stream = client.SimpleClientStream();
await stream.RequestStream.WriteAllAsync(new[] {new TestRequest()}, true);
var response = await stream;
```

#### 4. Call with sequential results

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();

// Setup
grpcMock
    .SetupSequence(c => c.SimpleClientStream(null, null, default))
    .ReturnsAsync(firstExpectedResponses)
    .ReturnsAsync(secondExpectedResponses);

var client = grpcMock.Object;

// Call
var firstCall = client.SimpleClientStream();
await firstCall.RequestStream.CompleteAsync();

var firstMessage = await firstCall;

var secondCall = client.SimpleClientStream();
await secondCall.RequestStream.CompleteAsync();

var secondMessage = await secondCall;
```

### Simple server stream calling:

#### 1. Call with exists response

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new[]{ new TestResponse() };

// Setup and set responses like argument (params TResponse[] responses)
grpcMock
    .Setup(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default))
    .ReturnsAsync(testResponse);

var client = grpcMock.Object;

// Call
var stream = client.SimpleServerStream(new TestRequest());
var responses = stream.ResponseStream.ReadAllAsync();
```

#### 2. Call with response creating on call

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new[]{ new TestResponse() };

// Setup and set responses by lambda (factory)
grpcMock
    .Setup(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default))
    .ReturnsAsync(() => testResponse);

var client = grpcMock.Object;

// Call
var stream = client.SimpleServerStream(new TestRequest());
var responses = stream.ResponseStream.ReadAllAsync();
```

#### 3. Call with response creating on call and based on request

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();

// Setup and set responses by lambda and request like argument
grpcMock
    .Setup(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default))
    .ReturnsAsync<TestService.TestServiceClient, TestRequest, TestResponse>(r => new[]{ new TestResponse{ Val = r.Val } });

var client = grpcMock.Object;

// Call
var stream = client.SimpleServerStream(new TestRequest());
var responses = stream.ResponseStream.ReadAllAsync();
```

#### 4. Call with sequential results

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();

// Setup
grpcMock
    .SetupSequence(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default))
    .ReturnsAsync(firstExpectedResponses)
    .ReturnsAsync(secondExpectedResponses);

var client = grpcMock.Object;

// Call
var firstMessages = await client.SimpleServerStream(new TestRequest()).ResponseStream.ReadAllAsync().ToArrayAsync();
var secondMessages = await client.SimpleServerStream(new TestRequest()).ResponseStream.ReadAllAsync().ToArrayAsync();
```

### Simple client server streams calling:

#### 1. Call with exists response

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new[]{ new TestResponse() };

// Setup and set responses like argument (params TResponse[] responses)
grpcMock
    .Setup(c => c.SimpleClientServerStream(null, null, default))
    .ReturnsAsync(testResponse);

var client = grpcMock.Object;

// Call
var stream = client.SimpleClientServerStream();
await stream.RequestStream.WriteAsync(new TestRequest(), true);
var responses = stream.ResponseStream.ReadAllAsync();
```

#### 2. Call with response creating on call

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new[]{ new TestResponse() };

// Setup and set responses by lambda
grpcMock
    .Setup(c => c.SimpleClientServerStream(null, null, default))
    .ReturnsAsync(() => testResponse);

var client = grpcMock.Object;

// Call
var stream = client.SimpleClientServerStream();
await stream.RequestStream.WriteAsync(new TestRequest(), true);
var responses = stream.ResponseStream.ReadAllAsync();
```

#### 3. Call with response creating on call and based on request

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new[]{ new TestResponse() };

// Setup and set responses by lambda and requests like args
grpcMock
    .Setup(c => c.SimpleClientServerStream(null, null, default))
    .ReturnsAsync(rs => rs.Select(r => new TestResponse{ Val = r.Val }));

var client = grpcMock.Object;

// Call
var stream = client.SimpleClientServerStream();
await stream.RequestStream.WriteAsync(new TestRequest(), true);
var responses = stream.ResponseStream.ReadAllAsync();
```

#### 4. Using reactive handling

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new[]{ new TestResponse() };

// Setup and set responses by lambda and requests like args
grpcMock
    .Setup(c => c.SimpleClientServerStream(null, null, default))
    .ReturnsAsync(r => new TestResponse{ Val = r.Val });

var client = grpcMock.Object;

// Call
var stream = client.SimpleClientServerStream();
await stream.RequestStream.WriteAsync(new TestRequest(), true);
var responses = stream.ResponseStream.ReadAllAsync();
```

#### 5. Call with sequential results

```csharp
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();

// Setup
grpcMock
    .SetupSequence(c => c.SimpleClientServerStream(null, null, default))
    .ReturnsAsync(firstExpectedResponses)
    .ReturnsAsync(secondExpectedResponses);

var client = grpcMock.Object;

// Call
var firstCall = client.SimpleClientServerStream();
await firstCall.RequestStream.CompleteAsync();

var firstMessages = await firstCall.ResponseStream
    .ReadAllAsync()
    .ToArrayAsync();

var secondCall = client.SimpleClientServerStream();
await secondCall.RequestStream.CompleteAsync();

var secondMessages = await secondCall.ResponseStream
    .ReadAllAsync()
    .ToArrayAsync();
```
