// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http;

using Asp.Versioning;
using Backport;
using System.ComponentModel;
using System.Web.Http.Controllers;

/// <summary>
/// Provides extension methods for the <see cref="HttpActionDescriptor"/> class.
/// </summary>
public static class HttpActionDescriptorExtensions
{
    private const string AttributeRoutedPropertyKey = "MS_IsAttributeRouted";

    /// <summary>
    /// Gets the API version information associated with an action.
    /// </summary>
    /// <param name="action">The <see cref="HttpActionDescriptor">action</see> to evaluate.</param>
    /// <returns>The <see cref="ApiVersionMetadata">API version information</see> for the action.</returns>
    public static ApiVersionMetadata GetApiVersionMetadata( this HttpActionDescriptor action )
    {
        ArgumentNullException.ThrowIfNull( action );

        if ( action.Properties.TryGetValue( typeof( ApiVersionMetadata ), out ApiVersionMetadata? value ) )
        {
            return value!;
        }

        return ApiVersionMetadata.Empty;
    }

    /// <summary>
    /// Sets the API version information associated with an action.
    /// </summary>
    /// <param name="action">The <see cref="HttpActionDescriptor">action</see> to evaluate.</param>'
    /// <param name="value">The <see cref="ApiVersionMetadata">API version information</see> for the action.</param>
    /// <remarks>This API is meant for infrastructure and should not be used by application code.</remarks>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static void SetApiVersionMetadata( this HttpActionDescriptor action, ApiVersionMetadata value )
    {
        ArgumentNullException.ThrowIfNull( action );
        action.Properties.AddOrUpdate( typeof( ApiVersionMetadata ), value, ( key, oldValue ) => value );
    }

    internal static bool IsAttributeRouted( this HttpActionDescriptor action ) =>
        action.Properties.TryGetValue( AttributeRoutedPropertyKey, out bool? value ) && ( value ?? false );
}