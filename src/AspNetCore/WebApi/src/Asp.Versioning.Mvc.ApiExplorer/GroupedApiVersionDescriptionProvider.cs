// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.Extensions.Options;
using System.Buffers;
using static Asp.Versioning.ApiVersionMapping;
using static System.Globalization.CultureInfo;

/// <summary>
/// Represents the default implementation of an object that discovers and describes the API version information within an application.
/// </summary>
[CLSCompliant( false )]
public class GroupedApiVersionDescriptionProvider : IApiVersionDescriptionProvider
{
    private readonly ApiVersionDescriptionCollection collection;
    private readonly IOptions<ApiExplorerOptions> options;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupedApiVersionDescriptionProvider"/> class.
    /// </summary>
    /// <param name="providers">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IApiVersionMetadataCollationProvider">API version metadata collation providers.</see>.</param>
    /// <param name="sunsetPolicyManager">The <see cref="ISunsetPolicyManager">manager</see> used to resolve sunset policies.</param>
    /// <param name="apiExplorerOptions">The <see cref="IOptions{TOptions}">container</see> of configured
    /// <see cref="ApiExplorerOptions">API explorer options</see>.</param>
    public GroupedApiVersionDescriptionProvider(
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
    /// <param name="metadata">The <see cref="IReadOnlyList{T}">read-only list</see> of
    /// <see cref="GroupedApiVersionMetadata">grouped API version metadata</see> within the application.</param>
    /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersionDescription">API
    /// version descriptions</see>.</returns>
    protected virtual IReadOnlyList<ApiVersionDescription> Describe( IReadOnlyList<GroupedApiVersionMetadata> metadata )
    {
        ArgumentNullException.ThrowIfNull( metadata );

        var descriptions = new SortedSet<ApiVersionDescription>( new ApiVersionDescriptionComparer() );
        var supported = new HashSet<GroupedApiVersion>();
        var deprecated = new HashSet<GroupedApiVersion>();

        BucketizeApiVersions( metadata, supported, deprecated );
        AppendDescriptions( descriptions, supported, deprecated: false );
        AppendDescriptions( descriptions, deprecated, deprecated: true );

        return descriptions.ToArray();
    }

    private void BucketizeApiVersions(
        IReadOnlyList<GroupedApiVersionMetadata> list,
        ISet<GroupedApiVersion> supported,
        ISet<GroupedApiVersion> deprecated )
    {
        var declared = new HashSet<GroupedApiVersion>();
        var advertisedSupported = new HashSet<GroupedApiVersion>();
        var advertisedDeprecated = new HashSet<GroupedApiVersion>();

        for ( var i = 0; i < list.Count; i++ )
        {
            var metadata = list[i];
            var groupName = metadata.GroupName;
            var model = metadata.Map( Explicit | Implicit );
            var versions = model.DeclaredApiVersions;

            for ( var j = 0; j < versions.Count; j++ )
            {
                declared.Add( new( groupName, versions[j] ) );
            }

            versions = model.SupportedApiVersions;

            for ( var j = 0; j < versions.Count; j++ )
            {
                var version = versions[j];
                supported.Add( new( groupName, version ) );
                advertisedSupported.Add( new( groupName, version ) );
            }

            versions = model.DeprecatedApiVersions;

            for ( var j = 0; j < versions.Count; j++ )
            {
                var version = versions[j];
                deprecated.Add( new( groupName, version ) );
                advertisedDeprecated.Add( new( groupName, version ) );
            }
        }

        advertisedSupported.ExceptWith( declared );
        advertisedDeprecated.ExceptWith( declared );
        supported.ExceptWith( advertisedSupported );
        deprecated.ExceptWith( supported.Concat( advertisedDeprecated ) );

        if ( supported.Count == 0 && deprecated.Count == 0 )
        {
            supported.Add( new( default, Options.DefaultApiVersion ) );
        }
    }

    private void AppendDescriptions(
        ICollection<ApiVersionDescription> descriptions,
        IEnumerable<GroupedApiVersion> versions,
        bool deprecated )
    {
        var format = Options.GroupNameFormat;
        var formatGroupName = Options.FormatGroupName;

        foreach ( var (groupName, version) in versions )
        {
            var formattedVersion = version.ToString( format, CurrentCulture );
            var formattedGroupName =
                string.IsNullOrEmpty( groupName ) || formatGroupName is null
                ? formattedVersion
                : formatGroupName( groupName, formattedVersion );

            var sunsetPolicy = SunsetPolicyManager.TryGetPolicy( version, out var policy ) ? policy : default;
            descriptions.Add( new( version, formattedGroupName, deprecated, sunsetPolicy ) );
        }
    }

    private sealed class ApiVersionDescriptionCollection(
            GroupedApiVersionDescriptionProvider provider,
            IEnumerable<IApiVersionMetadataCollationProvider> collators )
    {
        private readonly object syncRoot = new();
        private readonly GroupedApiVersionDescriptionProvider provider = provider;
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

                    var results = context.Results;
                    var metadata = new GroupedApiVersionMetadata[results.Count];

                    for ( var i = 0; i < metadata.Length; i++ )
                    {
                        metadata[i] = new( context.Results.GroupName( i ), results[i] );
                    }

                    items = provider.Describe( metadata );
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

    private sealed class ApiVersionDescriptionComparer : IComparer<ApiVersionDescription>
    {
        public int Compare( ApiVersionDescription? x, ApiVersionDescription? y )
        {
            if ( x is null )
            {
                return y is null ? 0 : -1;
            }

            if ( y is null )
            {
                return 1;
            }

            var result = x.ApiVersion.CompareTo( y.ApiVersion );

            if ( result == 0 )
            {
                result = StringComparer.Ordinal.Compare( x.GroupName, y.GroupName );
            }

            return result;
        }
    }

    /// <summary>
    /// Represents the API version metadata applied to an endpoint with an optional group name.
    /// </summary>
    protected class GroupedApiVersionMetadata : ApiVersionMetadata, IEquatable<GroupedApiVersionMetadata>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupedApiVersionMetadata"/> class.
        /// </summary>
        /// <param name="groupName">The associated group name.</param>
        /// <param name="metadata">The existing metadata to initialize from.</param>
        public GroupedApiVersionMetadata( string? groupName, ApiVersionMetadata metadata )
            : base( metadata ) => GroupName = groupName;

        /// <summary>
        /// Gets the associated group name.
        /// </summary>
        /// <value>The associated group name, if any.</value>
        public string? GroupName { get; }

        /// <inheritdoc />
        public bool Equals( GroupedApiVersionMetadata? other ) =>
            other is not null && other.GetHashCode() == GetHashCode();

        /// <inheritdoc />
        public override bool Equals( object? obj ) =>
            obj is not null &&
            GetType().Equals( obj.GetType() ) &&
            GetHashCode() == obj.GetHashCode();

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = default( HashCode );

            if ( !string.IsNullOrEmpty( GroupName ) )
            {
                hash.Add( GroupName, StringComparer.Ordinal );
            }

            hash.Add( base.GetHashCode() );

            return hash.ToHashCode();
        }
    }

    private record struct GroupedApiVersion( string? GroupName, ApiVersion ApiVersion );
}