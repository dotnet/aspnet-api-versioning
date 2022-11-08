// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
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
    /// <param name="endpointDataSource">The <see cref="EndpointDataSource">data source</see> for <see cref="Endpoint">endpoints</see>.</param>
    /// <param name="actionDescriptorCollectionProvider">The <see cref="IActionDescriptorCollectionProvider">provider</see>
    /// used to enumerate the actions within an application.</param>
    /// <param name="sunsetPolicyManager">The <see cref="ISunsetPolicyManager">manager</see> used to resolve sunset policies.</param>
    /// <param name="apiExplorerOptions">The <see cref="IOptions{TOptions}">container</see> of configured
    /// <see cref="ApiExplorerOptions">API explorer options</see>.</param>
    public GroupedApiVersionDescriptionProvider(
        EndpointDataSource endpointDataSource,
        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
        ISunsetPolicyManager sunsetPolicyManager,
        IOptions<ApiExplorerOptions> apiExplorerOptions )
    {
        collection = new( this, endpointDataSource, actionDescriptorCollectionProvider );
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
        if ( metadata == null )
        {
            throw new ArgumentNullException( nameof( metadata ) );
        }

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

    private sealed class ApiVersionDescriptionCollection
    {
        private readonly object syncRoot = new();
        private readonly GroupedApiVersionDescriptionProvider apiVersionDescriptionProvider;
        private readonly EndpointApiVersionMetadataCollection endpoints;
        private readonly ActionApiVersionMetadataCollection actions;
        private IReadOnlyList<ApiVersionDescription>? items;
        private long version;

        public ApiVersionDescriptionCollection(
            GroupedApiVersionDescriptionProvider apiVersionDescriptionProvider,
            EndpointDataSource endpointDataSource,
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider )
        {
            this.apiVersionDescriptionProvider = apiVersionDescriptionProvider;
            endpoints = new( endpointDataSource );
            actions = new( actionDescriptorCollectionProvider );
        }

        public IReadOnlyList<ApiVersionDescription> Items
        {
            get
            {
                if ( items is not null && version == CurrentVersion )
                {
                    return items;
                }

                lock ( syncRoot )
                {
                    var (items1, version1) = endpoints;
                    var (items2, version2) = actions;
                    var currentVersion = ComputeVersion( version1, version2 );

                    if ( items is not null && version == currentVersion )
                    {
                        return items;
                    }

                    var capacity = items1.Count + items2.Count;
                    var metadata = new List<GroupedApiVersionMetadata>( capacity );

                    for ( var i = 0; i < items1.Count; i++ )
                    {
                        metadata.Add( items1[i] );
                    }

                    for ( var i = 0; i < items2.Count; i++ )
                    {
                        metadata.Add( items2[i] );
                    }

                    items = apiVersionDescriptionProvider.Describe( metadata );
                    version = currentVersion;
                }

                return items;
            }
        }

        private long CurrentVersion
        {
            get
            {
                lock ( syncRoot )
                {
                    return ComputeVersion( endpoints.Version, actions.Version );
                }
            }
        }

        private static long ComputeVersion( int version1, int version2 ) => ( ( (long) version1 ) << 32 ) | (long) version2;
    }

    private sealed class EndpointApiVersionMetadataCollection
    {
        private readonly object syncRoot = new();
        private readonly EndpointDataSource endpointDataSource;
        private List<GroupedApiVersionMetadata>? list;
        private int version;
        private int currentVersion;

        public EndpointApiVersionMetadataCollection( EndpointDataSource endpointDataSource )
        {
            this.endpointDataSource = endpointDataSource ?? throw new ArgumentNullException( nameof( endpointDataSource ) );
            ChangeToken.OnChange( endpointDataSource.GetChangeToken, IncrementVersion );
        }

        public int Version => version;

        public IReadOnlyList<GroupedApiVersionMetadata> Items
        {
            get
            {
                if ( list is not null && version == currentVersion )
                {
                    return list;
                }

                lock ( syncRoot )
                {
                    if ( list is not null && version == currentVersion )
                    {
                        return list;
                    }

                    var endpoints = endpointDataSource.Endpoints;

                    if ( list == null )
                    {
                        list = new( capacity: endpoints.Count );
                    }
                    else
                    {
                        list.Clear();
                        list.Capacity = endpoints.Count;
                    }

                    for ( var i = 0; i < endpoints.Count; i++ )
                    {
                        var metadata = endpoints[i].Metadata;

                        if ( metadata.GetMetadata<ApiVersionMetadata>() is ApiVersionMetadata item )
                        {
#if NETCOREAPP3_1
                            // this code path doesn't appear to exist for netcoreapp3.1
                            // REF: https://github.com/dotnet/aspnetcore/blob/release/3.1/src/Mvc/Mvc.ApiExplorer/src/DefaultApiDescriptionProvider.cs#L74
                            list.Add( new( default, item ) );
#else
                            var groupName = metadata.OfType<IEndpointGroupNameMetadata>().LastOrDefault()?.EndpointGroupName;
                            list.Add( new( groupName, item ) );
#endif
                        }
                    }

                    version = currentVersion;
                }

                return list;
            }
        }

        public void Deconstruct( out IReadOnlyList<GroupedApiVersionMetadata> items, out int version )
        {
            lock ( syncRoot )
            {
                version = this.version;
                items = Items;
            }
        }

        private void IncrementVersion()
        {
            lock ( syncRoot )
            {
                currentVersion++;
            }
        }
    }

    private sealed class ActionApiVersionMetadataCollection
    {
        private readonly object syncRoot = new();
        private readonly IActionDescriptorCollectionProvider provider;
        private List<GroupedApiVersionMetadata>? list;
        private int version;

        public ActionApiVersionMetadataCollection( IActionDescriptorCollectionProvider actionDescriptorCollectionProvider ) =>
            provider = actionDescriptorCollectionProvider ?? throw new ArgumentNullException( nameof( actionDescriptorCollectionProvider ) );

        public int Version => version;

        public IReadOnlyList<GroupedApiVersionMetadata> Items
        {
            get
            {
                var collection = provider.ActionDescriptors;

                if ( list is not null && collection.Version == version )
                {
                    return list;
                }

                lock ( syncRoot )
                {
                    if ( list is not null && collection.Version == version )
                    {
                        return list;
                    }

                    var actions = collection.Items;

                    if ( list == null )
                    {
                        list = new( capacity: actions.Count );
                    }
                    else
                    {
                        list.Clear();
                        list.Capacity = actions.Count;
                    }

                    for ( var i = 0; i < actions.Count; i++ )
                    {
                        var action = actions[i];
                        list.Add( new( GetGroupName( action ), action.GetApiVersionMetadata() ) );
                    }

                    version = collection.Version;
                }

                return list;
            }
        }

        // REF: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.ApiExplorer/src/DefaultApiDescriptionProvider.cs
        private static string? GetGroupName( ActionDescriptor action )
        {
#if NETCOREAPP3_1
            return action.GetProperty<ApiDescriptionActionData>()?.GroupName;
#else
            var endpointGroupName = action.EndpointMetadata.OfType<IEndpointGroupNameMetadata>().LastOrDefault();

            if ( endpointGroupName is null )
            {
                return action.GetProperty<ApiDescriptionActionData>()?.GroupName;
            }

            return endpointGroupName.EndpointGroupName;
#endif
        }

        public void Deconstruct( out IReadOnlyList<GroupedApiVersionMetadata> items, out int version )
        {
            lock ( syncRoot )
            {
                version = this.version;
                items = Items;
            }
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