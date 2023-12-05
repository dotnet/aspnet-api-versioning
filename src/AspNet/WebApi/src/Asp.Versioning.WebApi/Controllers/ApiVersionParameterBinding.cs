// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Controllers;

using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

/// <summary>
/// Represents the binding for an <see cref="ApiVersion">API version</see> parameter.
/// </summary>
public class ApiVersionParameterBinding : HttpParameterBinding
{
    private static readonly Task CompletedTask = Task.FromResult( default( object ) );

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionParameterBinding"/> class.
    /// </summary>
    /// <param name="descriptor">The <see cref="HttpParameterDescriptor">parameter descriptor</see> associated with the binding.</param>
    public ApiVersionParameterBinding( HttpParameterDescriptor descriptor ) : base( descriptor ) { }

    /// <inheritdoc />
    public override Task ExecuteBindingAsync(
        ModelMetadataProvider metadataProvider,
        HttpActionContext actionContext,
        CancellationToken cancellationToken )
    {
        ArgumentNullException.ThrowIfNull( actionContext );
        var value = actionContext.Request.ApiVersionProperties().RequestedApiVersion;
        SetValue( actionContext, value );
        return CompletedTask;
    }

    /// <summary>
    /// Creates and returns a new parameter binding for the specified descriptor.
    /// </summary>
    /// <param name="descriptor">The <see cref="HttpParameterDescriptor">parameter descriptor</see> to create a binding for.</param>
    /// <returns>A new <see cref="HttpParameterBinding">parameter binding</see>.</returns>
    public static HttpParameterBinding Create( HttpParameterDescriptor descriptor ) => new ApiVersionParameterBinding( descriptor );
}