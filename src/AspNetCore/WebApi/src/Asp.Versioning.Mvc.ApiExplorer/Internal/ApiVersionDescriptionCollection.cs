// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer.Internal;

internal sealed class ApiVersionDescriptionCollection<T>(
    Func<IReadOnlyList<T>, IReadOnlyList<ApiVersionDescription>> describe,
    IEnumerable<IApiVersionMetadataCollationProvider> collators )
    where T : IGroupedApiVersionMetadata, IGroupedApiVersionMetadataFactory<T>
{
    private readonly Lock syncRoot = new();
    private readonly Func<IReadOnlyList<T>, IReadOnlyList<ApiVersionDescription>> describe = describe;
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
                var metadata = new T[results.Count];

                for ( var i = 0; i < metadata.Length; i++ )
                {
                    metadata[i] = T.New( context.Results.GroupName( i ), results[i] );
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