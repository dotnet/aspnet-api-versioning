namespace ApiVersioning.Examples.Services.V1;

using Grpc.Core;

/// <summary>
/// Represents a gRPC greeter service
/// </summary>
public class GreeterService : Greeter.GreeterBase
{
    /// <summary>
    /// Say Hello
    /// </summary>
    /// <description>Says hello to the specified user</description>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns>A user-specific greeting</returns>
    public override Task<HelloReply> SayHello( HelloRequest request, ServerCallContext context ) =>
        Task.FromResult( new HelloReply { Message = $"Hello {request.Name} (v{request.ApiVersion})" } );
}