// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.Extensions.Options;
using static Asp.Versioning.ApiVersionMapping;
using static System.Globalization.CultureInfo;

/// <summary>
/// Represents the default implementation of an object that discovers and describes the API version information within
/// an application.
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
    /// <param name="sunsetPolicyManager">The <see cref="IPolicyManager{TPolicy}">manager</see> used to resolve sunset policies.</param>
    /// <param name="deprecationPolicyManager">The <see cref="IPolicyManager{TPolicy}">manager</see> used to resolve deprecation policies.</param>
    /// <param name="apiExplorerOptions">The <see cref="IOptions{TOptions}">container</see> of configured
    /// <see cref="ApiExplorerOptions">API explorer options</see>.</param>
    public GroupedApiVersionDescriptionProvider(
        IEnumerable<IApiVersionMetadataCollationProvider> providers,
        IPolicyManager<SunsetPolicy> sunsetPolicyManager,
        IPolicyManager<DeprecationPolicy> deprecationPolicyManager,
        IOptions<ApiExplorerOptions> apiExplorerOptions )
    {
        collection = new( Describe, providers ?? throw new ArgumentNullException( nameof( providers ) ) );
        SunsetPolicyManager = sunsetPolicyManager;
        DeprecationPolicyManager = deprecationPolicyManager;
        options = apiExplorerOptions;
    }

    /// <summary>
    /// Gets the manager used to resolve sunset policies.
    /// </summary>
    /// <value>The associated <see cref="IPolicyManager{TPolicy}">sunset policy manager</see>.</value>
    protected IPolicyManager<SunsetPolicy> SunsetPolicyManager { get; }

    /// <summary>
    /// Gets the manager used to resolve deprecation policies.
    /// </summary>
    /// <value>The associated <see cref="IPolicyManager{TPolicy}">deprecation policy manager</see>.</value>
    protected IPolicyManager<DeprecationPolicy> DeprecationPolicyManager { get; }

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
        return DescriptionProvider.Describe( metadata, SunsetPolicyManager, DeprecationPolicyManager, Options );
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

    private sealed class ApiVersionDescriptionCollection(
        Func<IReadOnlyList<GroupedApiVersionMetadata>, IReadOnlyList<ApiVersionDescription>> describe,
        IEnumerable<IApiVersionMetadataCollationProvider> collators )
    {
        private readonly Lock syncRoot = new();
        private readonly Func<IReadOnlyList<GroupedApiVersionMetadata>, IReadOnlyList<ApiVersionDescription>> describe = describe;
        private readonly IApiVersionMetadataCollationProvider[] collators = [.. collators];
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

                using ( syncRoot.EnterScope() )
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
                        metadata[i] = new GroupedApiVersionMetadata( context.Results.GroupName( i ), results[i] );
                    }

                    items = describe( metadata );
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

    private static class DescriptionProvider
    {
        internal static ApiVersionDescription[] Describe(
            IReadOnlyList<GroupedApiVersionMetadata> metadata,
            ISunsetPolicyManager sunsetPolicyManager,
            ApiExplorerOptions options )
        {
            var descriptions = new SortedSet<ApiVersionDescription>( new ApiVersionDescriptionComparer() );
            var supported = new HashSet<GroupedApiVersion>();
            var deprecated = new HashSet<GroupedApiVersion>();

            BucketizeApiVersions( metadata, supported, deprecated, options );
            AppendDescriptions( descriptions, supported, sunsetPolicyManager, options, deprecated: false );
            AppendDescriptions( descriptions, deprecated, sunsetPolicyManager, options, deprecated: true );

            return [.. descriptions];
        }

        private static void BucketizeApiVersions(
            IReadOnlyList<GroupedApiVersionMetadata> list,
            HashSet<GroupedApiVersion> supported,
            HashSet<GroupedApiVersion> deprecated,
            ApiExplorerOptions options )
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
                supported.Add( new( default, options.DefaultApiVersion ) );
            }
        }

        private static void AppendDescriptions(
            SortedSet<ApiVersionDescription> descriptions,
            HashSet<GroupedApiVersion> versions,
            ISunsetPolicyManager sunsetPolicyManager,
            ApiExplorerOptions options,
            bool deprecated )
        {
            var format = options.GroupNameFormat;
            var formatGroupName = options.FormatGroupName;

            foreach ( var (groupName, version) in versions )
            {
                var formattedGroupName = groupName;

                if ( string.IsNullOrEmpty( formattedGroupName ) )
                {
                    formattedGroupName = version.ToString( format, CurrentCulture );
                }
                else if ( formatGroupName is not null )
                {
                    formattedGroupName = formatGroupName( formattedGroupName, version.ToString( format, CurrentCulture ) );
                }

                var sunsetPolicy = sunsetPolicyManager.TryGetPolicy( version, out var policy ) ? policy : default;
                descriptions.Add( new( version, formattedGroupName, deprecated, sunsetPolicy ) );
            }
        }
    }
}