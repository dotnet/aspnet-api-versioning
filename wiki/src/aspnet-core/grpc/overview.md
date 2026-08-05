# API Versioning with gRPC

Service API versioning using gRPC is nearly identical to the standard configuration with only a few modifications. When
a gRPC service is registered, it indicates which API versions it serves. As with all other versioned services, the
routes for gRPC services can overlap and they will be disambiguated by the requested API version.

```protobuf
syntax = "proto3";

import "google/api/annotations.proto";

package greet;

message HelloRequest {
  string name = 1;
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

```c#
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddApiVersioning().AddGrpc();

var app = builder.Build();
var greeter = app.NewVersionedApi();

greeter.MapGrpcService<GreeterService>().HasApiVersion( 1.0 );
app.Run();
```