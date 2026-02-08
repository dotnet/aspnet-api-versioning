// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace System.Web.Http;

using Asp.Versioning;
using Backport;
using System.Web.Http.Controllers;

/// <summary>
/// Provides extension methods for the <see cref="HttpActionDescriptor"/> class.
/// </summary>
public static class HttpActionDescriptorExtensions
{
    private const string AttributeRoutedPropertyKey = "MS_IsAttributeRouted";

    /// <param name="action">The <see cref="HttpActionDescriptor">action</see> to evaluate.</param>
    extension( HttpActionDescriptor action )
    {
        /// <summary>
        /// Gets or sets the API version information associated with an action.
        /// </summary>
        /// <returns>The <see cref="ApiVersionMetadata">API version information</see> for the action.</returns>
        /// <remarks>Setting this property is meant for infrastructure and should not be used by application code.</remarks>
        public ApiVersionMetadata ApiVersionMetadata
        {
            get
            {
                ArgumentNullException.ThrowIfNull( action );

                if ( action.Properties.TryGetValue( typeof( ApiVersionMetadata ), out ApiVersionMetadata? value ) )
                {
                    return value!;
                }

                return Asp.Versioning.ApiVersionMetadata.Empty;
            }
            set
            {
                ArgumentNullException.ThrowIfNull( action );
                action.Properties.AddOrUpdate( typeof( ApiVersionMetadata ), value, ( key, oldValue ) => value );
            }
        }

        internal bool IsAttributeRouted =>
            action.Properties.TryGetValue( AttributeRoutedPropertyKey, out bool? value ) && ( value ?? false );
    }
}