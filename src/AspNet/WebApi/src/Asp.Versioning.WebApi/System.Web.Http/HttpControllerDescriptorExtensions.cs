// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http;

using Asp.Versioning;
using Asp.Versioning.Controllers;
using Backport;
using System.ComponentModel;
using System.Web.Http.Controllers;
using System.Web.Http.Description;

/// <summary>
/// Provides extension methods for the <see cref="HttpControllerDescriptor"/> class.
/// </summary>
public static class HttpControllerDescriptorExtensions
{
    private const string AttributeRoutedPropertyKey = "MS_IsAttributeRouted";
    private const string PossibleControllerCandidatesKey = "MS_PossibleControllerCandidates";

    /// <summary>
    /// Gets the API version information associated with a controller.
    /// </summary>
    /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller</see> to evaluate.</param>
    /// <returns>The <see cref="ApiVersionModel">API version information</see> for the controller.</returns>
    /// <remarks>
    /// <para>
    /// A controller only contains implicitly declared API versions relative to an action. Most scenarios
    /// should use <see cref="HttpActionDescriptorExtensions.GetApiVersionMetadata(HttpActionDescriptor)"/> instead. Components
    /// such as the <see cref="IApiExplorer"/> may need to know API versions declared by an action's defining controller.
    /// </para>
    /// <para>
    /// This API is meant for infrastructure and should not be used by application code.
    /// </para>
    /// </remarks>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static ApiVersionModel GetApiVersionModel( this HttpControllerDescriptor controllerDescriptor )
    {
        ArgumentNullException.ThrowIfNull( controllerDescriptor );

        if ( controllerDescriptor.Properties.TryGetValue( typeof( ApiVersionModel ), out ApiVersionModel? value ) )
        {
            return value!;
        }

        return ApiVersionModel.Empty;
    }

    /// <summary>
    /// Sets the API version information associated with a controller.
    /// </summary>
    /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller</see> to evaluate.</param>
    /// <param name="value">The <see cref="ApiVersionModel">API version information</see> for the controller.</param>
    /// <remarks>This API is meant for infrastructure and should not be used by application code.</remarks>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static void SetApiVersionModel( this HttpControllerDescriptor controllerDescriptor, ApiVersionModel value )
    {
        ArgumentNullException.ThrowIfNull( controllerDescriptor );

        controllerDescriptor.Properties.AddOrUpdate( typeof( ApiVersionModel ), value, ( key, oldValue ) => value );

        if ( controllerDescriptor is IEnumerable<HttpControllerDescriptor> grouped )
        {
            foreach ( var controller in grouped )
            {
                controller.Properties.AddOrUpdate( typeof( ApiVersionModel ), value, ( key, oldValue ) => value );
            }
        }
    }

    /// <summary>
    /// Enumerates a controller descriptor as a sequence of descriptors.
    /// </summary>
    /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller descriptor</see> to enumerate.</param>
    /// <returns>A <see cref="IEnumerable{T}">sequence</see> of <see cref="HttpControllerDescriptor">controller descriptors</see>.</returns>
    /// <remarks>This method will flatten a sequence of composite descriptors such as <see cref="HttpControllerDescriptorGroup"/>.
    /// If the <paramref name="controllerDescriptor">controller descriptor</paramref> is not a composite, it yields itself.</remarks>
    public static IEnumerable<HttpControllerDescriptor> AsEnumerable( this HttpControllerDescriptor controllerDescriptor ) =>
        AsEnumerable( controllerDescriptor, includeCandidates: false );

    internal static IEnumerable<HttpControllerDescriptor> AsEnumerable( this HttpControllerDescriptor controllerDescriptor, bool includeCandidates )
    {
        ArgumentNullException.ThrowIfNull( controllerDescriptor );

        var visited = new HashSet<HttpControllerDescriptor>();

        if ( controllerDescriptor is IEnumerable<HttpControllerDescriptor> groupedDescriptors )
        {
            foreach ( var groupedDescriptor in groupedDescriptors )
            {
                if ( visited.Add( groupedDescriptor ) )
                {
                    yield return groupedDescriptor;
                }
            }
        }
        else
        {
            visited.Add( controllerDescriptor );
            yield return controllerDescriptor;
        }

        if ( !includeCandidates || !controllerDescriptor.Properties.TryGetValue( PossibleControllerCandidatesKey, out IEnumerable<HttpControllerDescriptor>? candidates ) )
        {
            yield break;
        }

        foreach ( var candidate in candidates! )
        {
            if ( visited.Add( candidate ) )
            {
                yield return candidate;
            }
        }

        visited.Clear();
    }

    internal static bool IsAttributeRouted( this HttpControllerDescriptor controller )
    {
        controller.Properties.TryGetValue( AttributeRoutedPropertyKey, out bool? value );
        return value ?? false;
    }

    internal static void SetPossibleCandidates( this HttpControllerDescriptor controllerDescriptor, IEnumerable<HttpControllerDescriptor> value ) =>
        controllerDescriptor.Properties.AddOrUpdate( PossibleControllerCandidatesKey, value, ( key, oldValue ) => value );
}