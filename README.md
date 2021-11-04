# grpc.net.testing.moq

[![.NET](https://github.com/maximiliysiss/grpc.net.testing.moq/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/maximiliysiss/grpc.net.testing.moq/actions/workflows/dotnet.yml)

Library to mocking gRPC client. Instead of `Grpc.Core.Testing` using extensions for `Moq`

Based on libraries:

* [Grpc.Core.Testing](https://www.nuget.org/packages/Grpc.Core.Testing)
* [Moq](https://www.nuget.org/packages/Moq)

## Using example

### Simple sync calling:

```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
// Setup and set return
grpcMock
    .Setup(c => c.Simple(It.IsAny<TestRequest>(), null, null, default))
    .Returns(new TestResponse());

var client = grpcMock.Object;

// Call
var response = client.Simple(new TestRequest());
```

### Simple async calling:

```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new TestResponse();

// Setup and set response like second argument
grpcMock.Setup(c => c.SimpleAsync(It.IsAny<TestRequest>(), null, null, default), testResponse);

var client = grpcMock.Object;

// Call
var response = await client.SimpleAsync(new TestRequest());
```

### Simple client stream calling:

```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new TestResponse();

// Setup and set response like second argument
grpcMock.Setup(c => c.SimpleClientStream(null, null, default), testResponse);

var client = grpcMock.Object;

// Call
var stream = client.SimpleClientStream();
await stream.RequestStream.WriteAllAsync(new[] {new TestRequest()});
var response = await stream;
```

### Simple server stream calling:

```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new TestResponse();

// Setup and set responses like second argument (params TResponse[] responses)
grpcMock.Setup(c => c.SimpleServerStream(It.IsAny<TestRequest>(), null, null, default), testResponse);

var client = grpcMock.Object;

// Call
var stream = client.SimpleServerStream(new TestRequest());
var responses = stream.ResponseStream.ReadAllAsync();
```

### Simple client server streams calling:

```c#
// Creation moq
var grpcMock = new Mock<TestService.TestServiceClient>();
var testResponse = new TestResponse();

// Setup and set responses like second argument (params TResponse[] responses)
grpcMock.Setup(c => c.SimpleClientServerStream(null, null, default), testResponse);

var client = grpcMock.Object;

// Call
var stream = client.SimpleClientServerStream();
await stream.RequestStream.WriteAsync(new TestRequest());
var responses = stream.ResponseStream.ReadAllAsync();
```