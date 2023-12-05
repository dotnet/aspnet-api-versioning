// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Mvc.Abstractions;

using Asp.Versioning;

/// <summary>
/// Provides extension methods for the <see cref="ActionDescriptor"/> class.
/// </summary>
[CLSCompliant( false )]
public static class ActionDescriptorExtensions
{
    /// <summary>
    /// Gets the API version information associated with an action.
    /// </summary>
    /// <param name="action">The extended <see cref="ActionDescriptor">action</see>.</param>
    /// <returns>The <see cref="ApiVersionMetadata">API version information</see> for the action.</returns>
    public static ApiVersionMetadata GetApiVersionMetadata( this ActionDescriptor action )
    {
        ArgumentNullException.ThrowIfNull( action );

        var endpointMetadata = action.EndpointMetadata;

        if ( endpointMetadata == null )
        {
            return ApiVersionMetadata.Empty;
        }

        for ( var i = 0; i < endpointMetadata.Count; i++ )
        {
            if ( endpointMetadata[i] is ApiVersionMetadata metadata )
            {
                return metadata;
            }
        }

        return ApiVersionMetadata.Empty;
    }

    internal static void AddOrReplaceApiVersionMetadata( this ActionDescriptor action, ApiVersionMetadata value )
    {
        var endpointMetadata = action.EndpointMetadata;

        if ( endpointMetadata == null )
        {
            action.EndpointMetadata = [value];
            return;
        }

        for ( var i = 0; i < endpointMetadata.Count; i++ )
        {
            if ( endpointMetadata[i] is not ApiVersionMetadata )
            {
                continue;
            }

            if ( endpointMetadata.IsReadOnly )
            {
                action.EndpointMetadata = endpointMetadata = endpointMetadata.ToList();
            }

            endpointMetadata[i] = value;
            return;
        }

        if ( endpointMetadata.IsReadOnly )
        {
            action.EndpointMetadata = [value];
        }
        else
        {
            endpointMetadata.Add( value );
        }
    }
}