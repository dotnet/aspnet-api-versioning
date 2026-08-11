<!-- description: Map API version query string and route parameters into the messages a gRPC service defines. -->

# Request Parameters

In most cases, adding API versioning to your gRPC services is orthogonal to how you define your service. gRPC sits atop
HTTP. API versioning relies on the _Uniform Interface_ REST constraint, which does not require knowing anything about
your service. It doesn't even know that gRPC is in use. This allows versioning by query string, HTTP header, or media
type to work without any direct changes to your service. gRPC only supports mapping query string and route parameters
in the path to messages defined by your service.

## Query String

When you version by query string, you do not need to define any additional message fields, but you _can_. Fields that
do not appear as route parameters, the request body, nor response body are considered to be query parameters. The
default query parameter is `"api-version"`, but this can be any name you like as long as the API versioning
configuration and message field name align.

```protobuf
syntax = "proto3";

import "google/api/annotations.proto";

package greet;

message HelloRequest {
  // optional if you want to retrieve the api-version query parameter in requests
  string api_version = 1 [json_name = "api-version"];
  string name = 2;
}

message HelloReply {
  string message = 1;
}

service Greeter {
  // GET /greet/{name}?api-version=1.0
  rpc SayHello (HelloRequest) returns (HelloReply) {
    option (google.api.http) = {        
      get: "/greet/{name}"
      response_body: "message"
    };
  }
}
```

If you need or want to retrieve the requested API version, but you do not want to include it as a message field, you
can get it directly from the incoming request:

```c#
public class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello( HelloRequest request, ServerCallContext context )
    {
        var apiVersion = context.GetHttpContext().ApiVersioningFeature.RawRequestedApiVersion;
        return Task.FromResult( new HelloReply { Message = $"Hello {request.Name} (v{apiVersion})" } );
    }
}
```

>[!NOTE]
>`RawRequestedApiVersion` is the API version as it appear over the wire, whereas `RequestedApiVersion` will be the
>parsed `ApiVersion` type.

## URL Path Segment

An alternate method of API versioning is using a URL path segment. This method of versioning contradicts the
_Uniform Interface_ REST constraint and, therefore, comes with several limitations and additional configuration.

```protobuf
syntax = "proto3";

import "google/api/annotations.proto";

package greet;

message HelloRequest {
  // required because it is a route parameter
  string api_version = 1;
  string name = 2;
}

message HelloReply {
  string message = 1;
}

service Greeter {
  // GET /v1/greet/{name}
  rpc SayHello (HelloRequest) returns (HelloReply) {
    option (google.api.http) = {        
      get: "/{api_version}/greet/{name}"
      response_body: "message"
    };
  }
}
```

When you version by a URL path segment, you need a route parameter to serve as the placeholder for the API version. If
you do not do this, then `/v1/greet/{name}` cannot be mapped to other API versions. Mapping `v1 → 3.0` is nonsensical.

gRPC route parameters do not have constraints and do not behave the same as ASP.NET Core route parameters and their
constraints. API versioning has to do additional work to determine if the incoming request matches and is thus slower
to execute. The matched route parameter will also hold the entire URL path segment, not just the API version. Recall
that the literal `'v'` is not part of the API version. The `{api_version}` route parameter is, therefore, useful for
routing, but not useful in the message. This method of versioning requires you to get the incoming API version from the
server call context instead.

```c#
public class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello( HelloRequest request, ServerCallContext context )
    {
        // required because `request.ApiVersion` will hold 'v1', but the requested version is '1.0'
        var apiVersion = context.GetHttpContext().ApiVersioningFeature.RawRequestedApiVersion;
        return Task.FromResult( new HelloReply { Message = $"Hello {request.Name} (v{apiVersion})" } );
    }
}
```