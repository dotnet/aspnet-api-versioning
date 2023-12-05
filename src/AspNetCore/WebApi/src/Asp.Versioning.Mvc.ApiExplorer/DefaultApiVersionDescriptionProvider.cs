// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.Extensions.Options;
using static Asp.Versioning.ApiVersionMapping;
using static System.Globalization.CultureInfo;

/// <summary>
/// Represents the default implementation of an object that discovers and describes the API version information within an application.
/// </summary>
[CLSCompliant( false )]
public class DefaultApiVersionDescriptionProvider : IApiVersionDescriptionProvider
{
    private readonly ApiVersionDescriptionCollection collection;
    private readonly IOptions<ApiExplorerOptions> options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultApiVersionDescriptionProvider"/> class.
    /// </summary>
    /// <param name="providers">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IApiVersionMetadataCollationProvider">API version metadata collation providers.</see>.</param>
    /// <param name="sunsetPolicyManager">The <see cref="ISunsetPolicyManager">manager</see> used to resolve sunset policies.</param>
    /// <param name="apiExplorerOptions">The <see cref="IOptions{TOptions}">container</see> of configured
    /// <see cref="ApiExplorerOptions">API explorer options</see>.</param>
    public DefaultApiVersionDescriptionProvider(
        IEnumerable<IApiVersionMetadataCollationProvider> providers,
        ISunsetPolicyManager sunsetPolicyManager,
        IOptions<ApiExplorerOptions> apiExplorerOptions )
    {
        collection = new( this, providers ?? throw new ArgumentNullException( nameof( providers ) ) );
        SunsetPolicyManager = sunsetPolicyManager;
        options = apiExplorerOptions;
    }

    /// <summary>
    /// Gets the manager used to resolve sunset policies.
    /// </summary>
    /// <value>The associated <see cref="ISunsetPolicyManager">sunset policy manager</see>.</value>
    protected ISunsetPolicyManager SunsetPolicyManager { get; }

    /// <summary>
    /// Gets the options associated with the API explorer.
    /// </summary>
    /// <value>The current <see cref="ApiExplorerOptions">API explorer options</see>.</value>
    protected ApiExplorerOptions Options => options.Value;

    /// <inheritdoc />
    public IReadOnlyList<ApiVersionDescription> ApiVersionDescriptions => collection.Items;

    /// <summary>
    /// Provides a list of API version descriptions from a list of application API version metadata.
    /// </summary>
    /// <param name="metadata">The <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersionMetadata">API version metadata</see>
    /// within the application.</param>
    /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersionDescription">API version descriptions</see>.</returns>
    protected virtual IReadOnlyList<ApiVersionDescription> Describe( IReadOnlyList<ApiVersionMetadata> metadata )
    {
        ArgumentNullException.ThrowIfNull( metadata );

        var descriptions = new List<ApiVersionDescription>( capacity: metadata.Count );
        var supported = new HashSet<ApiVersion>();
        var deprecated = new HashSet<ApiVersion>();

        BucketizeApiVersions( metadata, supported, deprecated );
        AppendDescriptions( descriptions, supported, deprecated: false );
        AppendDescriptions( descriptions, deprecated, deprecated: true );

        return descriptions.OrderBy( d => d.ApiVersion ).ToArray();
    }

    private void BucketizeApiVersions( IReadOnlyList<ApiVersionMetadata> metadata, HashSet<ApiVersion> supported, HashSet<ApiVersion> deprecated )
    {
        var declared = new HashSet<ApiVersion>();
        var advertisedSupported = new HashSet<ApiVersion>();
        var advertisedDeprecated = new HashSet<ApiVersion>();

        for ( var i = 0; i < metadata.Count; i++ )
        {
            var model = metadata[i].Map( Explicit | Implicit );
            var versions = model.DeclaredApiVersions;

            for ( var j = 0; j < versions.Count; j++ )
            {
                declared.Add( versions[j] );
            }

            versions = model.SupportedApiVersions;

            for ( var j = 0; j < versions.Count; j++ )
            {
                var version = versions[j];
                supported.Add( version );
                advertisedSupported.Add( version );
            }

            versions = model.DeprecatedApiVersions;

            for ( var j = 0; j < versions.Count; j++ )
            {
                var version = versions[j];
                deprecated.Add( version );
                advertisedDeprecated.Add( version );
            }
        }

        advertisedSupported.ExceptWith( declared );
        advertisedDeprecated.ExceptWith( declared );
        supported.ExceptWith( advertisedSupported );
        deprecated.ExceptWith( supported.Concat( advertisedDeprecated ) );

        if ( supported.Count == 0 && deprecated.Count == 0 )
        {
            supported.Add( Options.DefaultApiVersion );
        }
    }

    private void AppendDescriptions( List<ApiVersionDescription> descriptions, IEnumerable<ApiVersion> versions, bool deprecated )
    {
        foreach ( var version in versions )
        {
            var groupName = version.ToString( Options.GroupNameFormat, CurrentCulture );
            var sunsetPolicy = SunsetPolicyManager.TryGetPolicy( version, out var policy ) ? policy : default;
            descriptions.Add( new( version, groupName, deprecated, sunsetPolicy ) );
        }
    }

    private sealed class ApiVersionDescriptionCollection(
        DefaultApiVersionDescriptionProvider provider,
        IEnumerable<IApiVersionMetadataCollationProvider> collators )
    {
        private readonly object syncRoot = new();
        private readonly DefaultApiVersionDescriptionProvider provider = provider;
        private readonly IApiVersionMetadataCollationProvider[] collators = collators.ToArray();
        private IReadOnlyList<ApiVersionDescription>? items;
        private int version;

        public IReadOnlyList<ApiVersionDescription> Items
        {
            get
            {
                if ( items is not null && version == ComputeVersion() )
                {
                    return items;
                }

                lock ( syncRoot )
                {
                    var currentVersion = ComputeVersion();

                    if ( items is not null && version == currentVersion )
                    {
                        return items;
                    }

                    var context = new ApiVersionMetadataCollationContext();

                    for ( var i = 0; i < collators.Length; i++ )
                    {
                        collators[i].Execute( context );
                    }

                    items = provider.Describe( context.Results );
                    version = currentVersion;
                }

                return items;
            }
        }

        private int ComputeVersion() =>
            collators.Length switch
            {
                0 => 0,
                1 => collators[0].Version,
                _ => ComputeVersion( collators ),
            };

        private static int ComputeVersion( IApiVersionMetadataCollationProvider[] providers )
        {
            var hash = default( HashCode );

            for ( var i = 0; i < providers.Length; i++ )
            {
                hash.Add( providers[i].Version );
            }

            return hash.ToHashCode();
        }
    }
}