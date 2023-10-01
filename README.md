# grpc.net.testing.moq

[![.NET](https://github.com/maximiliysiss/grpc.net.testing.moq/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/maximiliysiss/grpc.net.testing.moq/actions/workflows/dotnet.yml)

Library to mocking gRPC client. Instead of `Grpc.Core.Testing` using extensions for `Moq`

Based on libraries:

* [Moq](https://www.nuget.org/packages/Moq)

## Install

### Nuget:

`Install-Package Net.Testing.Moq.Grpc`

## Using example

### Simple sync calling:

#### 1. Call with exists response
```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();

// Setup and set return
grpcMock
    .When(c => c.Simple(It.IsAny<TestRequest>(), null, null, default))
    .Returns(new TestResponse());

var client = grpcMock.Object;

// Call
var response = client.Simple(new TestRequest());
```


### Simple async calling:

#### 1. Call with exists response

```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new TestResponse();

// Setup and set return
grpcMock
    .When(c => c.SimpleAsync(It.IsAny<TestRequest>(), null, null, default))
    .Returns(testResponse);

var client = grpcMock.Object;

// Call
var response = await client.SimpleAsync(new TestRequest());
```

#### 2. Call with response creating on call
```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();

// Setup and set return by lambda
grpcMock
    .When(c => c.SimpleAsync(It.IsAny<TestRequest>(), null, null, default))
    .Returns(() => new TestResponse());

var client = grpcMock.Object;

// Call
var response = client.SimpleAsync(new TestRequest());
```

#### 3. Call with response creating on call and based on request
```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();

// Setup and set return by lambda with request param
grpcMock
    .When(c => c.SimpleAsync(It.IsAny<TestRequest>(), null, null, default))
    .Returns<TestRequest>(r => new TestResponse{ Val = r.Val });

var client = grpcMock.Object;

// Call
var response = client.SimpleAsync(new TestRequest());
```

### Simple client stream calling:

#### 1. Call with exists response

```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new TestResponse();

// Setup and set response
grpcMock
    .When(c => c.SimpleClientStream(null, null, default))
    .Returns(testResponse);

var client = grpcMock.Object;

// Call
var stream = client.SimpleClientStream();
await stream.RequestStream.WriteAllAsync(new[] {new TestRequest()}, true);
var response = await stream;
```

#### 2. Call with response creating on call

```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new TestResponse();

// Setup and set response by lambda
grpcMock
    .When(c => c.SimpleClientStream(null, null, default))
    .Returns(() => testResponse);

var client = grpcMock.Object;

// Call
var stream = client.SimpleClientStream();
await stream.RequestStream.WriteAllAsync(new[] {new TestRequest()}, true);
var response = await stream;
```

#### 3. Call with response creating on call and based on request

```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();

// Setup and set response by lambda and requests like param
grpcMock
    .When(c => c.SimpleClientStream(null, null, default))
    .Returns(rs => new TestResponse{ Val = rs.Sum(r => r.Val) });

var client = grpcMock.Object;

// Call
var stream = client.SimpleClientStream();
await stream.RequestStream.WriteAllAsync(new[] {new TestRequest()}, true);
var response = await stream;
```

### Simple server stream calling:

#### 1. Call with exists response

```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new[]{ new TestResponse() };

// Setup and set responses like argument (params TResponse[] responses)
grpcMock
    .When(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default))
    .Returns(testResponse);

var client = grpcMock.Object;

// Call
var stream = client.SimpleServerStream(new TestRequest());
var responses = stream.ResponseStream.ReadAllAsync();
```

#### 2. Call with response creating on call

```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new[]{ new TestResponse() };

// Setup and set responses by lambda (factory)
grpcMock
    .When(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default))
    .Returns(() => testResponse);

var client = grpcMock.Object;

// Call
var stream = client.SimpleServerStream(new TestRequest());
var responses = stream.ResponseStream.ReadAllAsync();
```

#### 3. Call with response creating on call and based on request

```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();

// Setup and set responses by lambda and request like argument
grpcMock
    .When(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default))
    .Returns<TestRequest>(r => new[]{ new TestResponse{ Val = r.Val } });

var client = grpcMock.Object;

// Call
var stream = client.SimpleServerStream(new TestRequest());
var responses = stream.ResponseStream.ReadAllAsync();
```

### Simple client server streams calling:

#### 1. Call with exists response

```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new[]{ new TestResponse() };

// Setup and set responses like argument (params TResponse[] responses)
grpcMock
    .When(c => c.SimpleClientServerStream(null, null, default))
    .Returns(testResponse);

var client = grpcMock.Object;

// Call
var stream = client.SimpleClientServerStream();
await stream.RequestStream.WriteAsync(new TestRequest(), true);
var responses = stream.ResponseStream.ReadAllAsync();
```

#### 2. Call with response creating on call

```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new[]{ new TestResponse() };

// Setup and set responses by lambda
grpcMock
    .When(c => c.SimpleClientServerStream(null, null, default))
    .Returns(() => testResponse);

var client = grpcMock.Object;

// Call
var stream = client.SimpleClientServerStream();
await stream.RequestStream.WriteAsync(new TestRequest(), true);
var responses = stream.ResponseStream.ReadAllAsync();
```

#### 3. Call with response creating on call and based on request

```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new[]{ new TestResponse() };

// Setup and set responses by lambda and requests like args
grpcMock
    .When(c => c.SimpleClientServerStream(null, null, default))
    .Returns(rs => rs.Select(r => new TestResponse{ Val = r.Val }));

var client = grpcMock.Object;

// Call
var stream = client.SimpleClientServerStream();
await stream.RequestStream.WriteAsync(new TestRequest(), true);
var responses = stream.ResponseStream.ReadAllAsync();
```
