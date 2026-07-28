namespace ApiVersioning.Examples.Services.V3;

using Grpc.Core;

/// <summary>
/// Represents a gRPC greeter service
/// </summary>
public class GreeterService : GreeterByUrl.GreeterByUrlBase
{
    /// <summary>
    /// Say Hello
    /// </summary>
    /// <description>Says hello to the specified user</description>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns>A user-specific greeting</returns>
    public override Task<HelloReply> SayHello( HelloRequest request, ServerCallContext context )
    {
        // note: request.ApiVersion contains the leading 'v'. use the resolved value instead
        var apiVersion = context.GetHttpContext().ApiVersioningFeature.RawRequestedApiVersion;
        return Task.FromResult( new HelloReply { Message = $"Hello {request.Name} (v{apiVersion})" } );
    }
}