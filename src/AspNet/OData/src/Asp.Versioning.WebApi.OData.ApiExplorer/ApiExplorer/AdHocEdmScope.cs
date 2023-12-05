// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.Conventions;
using Asp.Versioning.Description;
using Asp.Versioning.OData;
using Microsoft.OData.Edm;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;

internal sealed class AdHocEdmScope : IDisposable
{
    private readonly IReadOnlyList<VersionedApiDescription> results;
    private bool disposed;

    internal AdHocEdmScope(
        IReadOnlyList<VersionedApiDescription> apiDescriptions,
        VersionedODataModelBuilder builder )
    {
        var conventions = builder.ModelConfigurations.OfType<IODataQueryOptionsConvention>().ToArray();

        results = FilterResults( apiDescriptions, conventions );

        if ( results.Count > 0 )
        {
            ApplyAdHocEdm( builder.GetEdmModels(), results );
        }
    }

    public void Dispose()
    {
        if ( disposed )
        {
            return;
        }

        disposed = true;

        for ( var i = 0; i < results.Count; i++ )
        {
            results[i].SetProperty( default( IEdmModel ) );
        }
    }

    private static IReadOnlyList<VersionedApiDescription> FilterResults(
        IReadOnlyList<VersionedApiDescription> apiDescriptions,
        IReadOnlyList<IODataQueryOptionsConvention> conventions )
    {
        if ( conventions.Count == 0 )
        {
            return Array.Empty<VersionedApiDescription>();
        }

        var results = default( List<VersionedApiDescription> );

        for ( var i = 0; i < apiDescriptions.Count; i++ )
        {
            var apiDescription = apiDescriptions[i];

            if ( apiDescription.EdmModel() != null || !apiDescription.IsODataLike() )
            {
                continue;
            }

            results ??= [];
            results.Add( apiDescription );

            for ( var j = 0; j < conventions.Count; j++ )
            {
                conventions[j].ApplyTo( apiDescription );
            }
        }

        return results?.ToArray() ?? [];
    }

    private static void ApplyAdHocEdm(
        IReadOnlyList<IEdmModel> models,
        IReadOnlyList<VersionedApiDescription> results )
    {
        for ( var i = 0; i < models.Count; i++ )
        {
            var model = models[i];
            var version = model.GetApiVersion();

            for ( var j = 0; j < results.Count; j++ )
            {
                var result = results[j];
                var metadata = result.ActionDescriptor.GetApiVersionMetadata();

                if ( metadata.IsMappedTo( version ) )
                {
                    result.SetProperty( model );
                }
            }
        }
    }
}