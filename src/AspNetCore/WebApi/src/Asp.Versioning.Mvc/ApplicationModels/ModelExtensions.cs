// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Mvc.ApplicationModels;

using Asp.Versioning;
using System.ComponentModel;

/// <summary>
/// Provides extension methods for <see cref="ApplicationModel">application models</see>, <see cref="ControllerModel">controller models</see>,
/// and <see cref="ActionModel">action models</see>.
/// </summary>
[CLSCompliant( false )]
public static class ModelExtensions
{
    /// <summary>
    /// Gets the API version information associated with an action.
    /// </summary>
    /// <param name="controller">The extended controller .</param>
    /// <returns>The <see cref="ApiVersionModel">API version information</see> for the controller.</returns>
    /// <remarks>This API is meant for infrastructure and should not be used by application code.</remarks>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static ApiVersionModel GetApiVersionModel( this ControllerModel controller )
    {
        ArgumentNullException.ThrowIfNull( controller );

        if ( controller.Properties.TryGetValue( typeof( ApiVersionModel ), out var value ) &&
             value is ApiVersionModel model )
        {
            return model;
        }

        return ApiVersionModel.Empty;
    }

    internal static void AddEndpointMetadata( this ActionModel action, object metadata )
    {
        var selectors = action.Selectors;

        if ( selectors.Count == 0 )
        {
            selectors.Add( new() );
        }

        for ( var i = 0; i < selectors.Count; i++ )
        {
            selectors[i].EndpointMetadata.Add( metadata );
        }
    }
}