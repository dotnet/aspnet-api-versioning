// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.AspNetCore.Mvc.Abstractions;

using Asp.Versioning;

/// <summary>
/// Provides extension methods for the <see cref="ActionDescriptor"/> class.
/// </summary>
[CLSCompliant( false )]
public static class ActionDescriptorExtensions
{
    /// <param name="action">The extended <see cref="ActionDescriptor">action</see>.</param>
    extension( ActionDescriptor action )
    {
        /// <summary>
        /// Gets the API version information associated with an action.
        /// </summary>
        /// <returns>The <see cref="ApiVersionMetadata">API version information</see> for the action.</returns>
        public ApiVersionMetadata ApiVersionMetadata
        {
            get
            {
                ArgumentNullException.ThrowIfNull( action );

                var endpointMetadata = action.EndpointMetadata;

                if ( endpointMetadata == null )
                {
                    return Asp.Versioning.ApiVersionMetadata.Empty;
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
        }

        internal void AddOrReplaceApiVersionMetadata( ApiVersionMetadata value )
        {
            var endpointMetadata = action.EndpointMetadata;

            if ( endpointMetadata == null )
            {
                action.EndpointMetadata = [value];
                return;
            }

            for ( var i = 0; i < endpointMetadata.Count; i++ )
            {
                if ( endpointMetadata[i] is not Asp.Versioning.ApiVersionMetadata )
                {
                    continue;
                }

                if ( endpointMetadata.IsReadOnly )
                {
                    action.EndpointMetadata = endpointMetadata = [.. endpointMetadata];
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
}