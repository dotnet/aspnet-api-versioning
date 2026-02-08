// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

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
    /// <param name="controller">The extended controller .</param>
    extension( ControllerModel controller )
    {
        /// <summary>
        /// Gets the API version information associated with an action.
        /// </summary>
        /// <returns>The <see cref="ApiVersionModel">API version information</see> for the controller.</returns>
        /// <remarks>This API is meant for infrastructure and should not be used by application code.</remarks>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public ApiVersionModel ApiVersionModel
        {
            get
            {
                ArgumentNullException.ThrowIfNull( controller );

                if ( controller.Properties.TryGetValue( typeof( ApiVersionModel ), out var value ) &&
                     value is ApiVersionModel model )
                {
                    return model;
                }

                return ApiVersionModel.Empty;
            }
        }
    }

    extension( ActionModel action )
    {
        internal void AddEndpointMetadata( object metadata )
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
}