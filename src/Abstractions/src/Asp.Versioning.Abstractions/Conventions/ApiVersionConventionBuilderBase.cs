// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using static Asp.Versioning.ApiVersionProviderOptions;

/// <summary>
/// Represents the base implementation of an API version convention builder.
/// </summary>
public abstract class ApiVersionConventionBuilderBase
{
    private HashSet<ApiVersion>? supported;
    private HashSet<ApiVersion>? deprecated;
    private HashSet<ApiVersion>? advertised;
    private HashSet<ApiVersion>? deprecatedAdvertised;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionConventionBuilderBase"/> class.
    /// </summary>
    protected ApiVersionConventionBuilderBase() { }

    /// <summary>
    /// Gets or sets a value indicating whether the current controller is API version-neutral.
    /// </summary>
    /// <value>True if the current controller is API version-neutral; otherwise, false. The default value is <c>false</c>.</value>
    protected bool VersionNeutral { get; set; }

    /// <summary>
    /// Gets a value indicating whether the builder is empty.
    /// </summary>
    /// <value>True if the builder does not have any API versions defined; otherwise, false.</value>
    protected virtual bool IsEmpty =>
        ( supported is null || supported.Count == 0 ) &&
        ( deprecated is null || deprecated.Count == 0 ) &&
        ( advertised is null || advertised.Count == 0 ) &&
        ( deprecatedAdvertised is null || deprecatedAdvertised.Count == 0 );

    /// <summary>
    /// Gets the collection of API versions supported by the current controller.
    /// </summary>
    /// <value>A <see cref="ICollection{T}">collection</see> of supported <see cref="ApiVersion">API versions</see>.</value>
    protected ICollection<ApiVersion> SupportedVersions => supported ??= [];

    /// <summary>
    /// Gets the collection of API versions deprecated by the current controller.
    /// </summary>
    /// <value>A <see cref="ICollection{T}">collection</see> of deprecated <see cref="ApiVersion">API versions</see>.</value>
    protected ICollection<ApiVersion> DeprecatedVersions => deprecated ??= [];

    /// <summary>
    /// Gets the collection of API versions advertised by the current controller.
    /// </summary>
    /// <value>A <see cref="ICollection{T}">collection</see> of advertised <see cref="ApiVersion">API versions</see>.</value>
    protected ICollection<ApiVersion> AdvertisedVersions => advertised ??= [];

    /// <summary>
    /// Gets the collection of API versions advertised and deprecated by the current controller.
    /// </summary>
    /// <value>A <see cref="ICollection{T}">collection</see> of advertised and deprecated <see cref="ApiVersion">API versions</see>.</value>
    protected ICollection<ApiVersion> DeprecatedAdvertisedVersions => deprecatedAdvertised ??= [];

    /// <summary>
    /// Merges API version information from the specified attributes with the current conventions.
    /// </summary>
    /// <param name="attributes">The <see cref="IEnumerable{T}">sequence</see> of attributes to merge.</param>
    protected virtual void MergeAttributesWithConventions( IEnumerable<object> attributes ) =>
        MergeAttributesWithConventions( ( attributes as IReadOnlyList<object> ) ?? attributes.ToArray() );

    /// <summary>
    /// Merges API version information from the specified attributes with the current conventions.
    /// </summary>
    /// <param name="attributes">The <see cref="IReadOnlyList{T}">read-only list</see> of attributes to merge.</param>
    protected virtual void MergeAttributesWithConventions( IReadOnlyList<object> attributes )
    {
        ArgumentNullException.ThrowIfNull( attributes );

        if ( VersionNeutral )
        {
            return;
        }

        const ApiVersionProviderOptions DeprecatedAdvertised = Deprecated | Advertised;
        var newSupported = default( HashSet<ApiVersion> );
        var newDeprecated = default( HashSet<ApiVersion> );
        var newAdvertised = default( HashSet<ApiVersion> );
        var newDeprecatedAdvertised = default( HashSet<ApiVersion> );

        for ( var i = 0; i < attributes.Count; i++ )
        {
            switch ( attributes[i] )
            {
                case IApiVersionNeutral:
                    VersionNeutral = true;
                    return;
                case IApiVersionProvider provider:
                    HashSet<ApiVersion> target;
                    IReadOnlyList<ApiVersion> source;

                    switch ( provider.Options )
                    {
                        case None:
                            target = newSupported ??= [];
                            source = provider.Versions;
                            break;
                        case Deprecated:
                            target = newDeprecated ??= [];
                            source = provider.Versions;
                            break;
                        case Advertised:
                            target = newAdvertised ??= [];
                            source = provider.Versions;
                            break;
                        case DeprecatedAdvertised:
                            target = newDeprecatedAdvertised ??= [];
                            source = provider.Versions;
                            break;
                        default:
                            continue;
                    }

                    for ( var j = 0; j < source.Count; j++ )
                    {
                        target.Add( source[j] );
                    }

                    break;
            }
        }

        if ( newSupported is not null && newSupported.Count > 0 )
        {
            SupportedVersions.UnionWith( newSupported );
        }

        if ( newDeprecated is not null && newDeprecated.Count > 0 )
        {
            DeprecatedVersions.UnionWith( newDeprecated );
        }

        if ( newAdvertised is not null && newAdvertised.Count > 0 )
        {
            AdvertisedVersions.UnionWith( newAdvertised );
        }

        if ( newDeprecatedAdvertised is not null && newDeprecatedAdvertised.Count > 0 )
        {
            DeprecatedAdvertisedVersions.UnionWith( newDeprecatedAdvertised );
        }
    }
}