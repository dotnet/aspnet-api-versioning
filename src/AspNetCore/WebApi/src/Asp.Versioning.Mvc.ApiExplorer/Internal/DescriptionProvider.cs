// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer.Internal;

using static Asp.Versioning.ApiVersionMapping;
using static System.Globalization.CultureInfo;

internal static class DescriptionProvider
{
    internal static ApiVersionDescription[] Describe<T>(
        IReadOnlyList<T> metadata,
        ISunsetPolicyManager sunsetPolicyManager,
        IDeprecationPolicyManager deprecationPolicyManager,
        ApiExplorerOptions options )
        where T : IGroupedApiVersionMetadata, IEquatable<T>
    {
        var descriptions = new SortedSet<ApiVersionDescription>( new ApiVersionDescriptionComparer() );
        var supported = new HashSet<GroupedApiVersion>();
        var deprecated = new HashSet<GroupedApiVersion>();

        BucketizeApiVersions( metadata, supported, deprecated, options );
        AppendDescriptions( descriptions, supported, sunsetPolicyManager, deprecationPolicyManager, options, deprecated: false );
        AppendDescriptions( descriptions, deprecated, sunsetPolicyManager, deprecationPolicyManager, options, deprecated: true );

        return [.. descriptions];
    }

    private static void BucketizeApiVersions<T>(
        IReadOnlyList<T> list,
        HashSet<GroupedApiVersion> supported,
        HashSet<GroupedApiVersion> deprecated,
        ApiExplorerOptions options )
        where T : IGroupedApiVersionMetadata
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
        IDeprecationPolicyManager deprecationPolicyManager,
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

            sunsetPolicyManager.TryGetPolicy( version, out var sunsetPolicy );
            deprecationPolicyManager.TryGetPolicy( version, out var deprecationPolicy );

            descriptions.Add( new( version, formattedGroupName, deprecated, sunsetPolicy, deprecationPolicy ) );
        }
    }
}