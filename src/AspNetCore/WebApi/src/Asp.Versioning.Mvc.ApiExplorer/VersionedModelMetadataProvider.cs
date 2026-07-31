// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

/// <summary>
/// Reports the model metadata of each API description for the API version it describes.
/// </summary>
internal sealed class VersionedModelMetadataProvider(
    IModelMetadataProvider modelMetadataProvider,
    IAnnotation<MemberInfo, ApiVersionRange> annotations ) : IApiDescriptionProvider
{
    // OnProvidersExecuting runs in ascending order, but OnProvidersExecuted runs in descending order. ordering
    // below every other provider means this runs after the versioned API Explorer has expanded each result into
    // one API description per version, and after any provider that reports its own versioned metadata, such as
    // gRPC or OData, has already replaced the metadata it owns
    public int Order => -1200;

    public void OnProvidersExecuting( ApiDescriptionProviderContext context ) { }

    public void OnProvidersExecuted( ApiDescriptionProviderContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var results = context.Results;

        for ( var i = 0; i < results.Count; i++ )
        {
            var result = results[i];

            if ( result.ApiVersion is not { } apiVersion )
            {
                continue;
            }

            // a minimal API is described by an action descriptor that is not a controller action
            var minimalApi = result.ActionDescriptor is not ControllerActionDescriptor;
            var parameters = result.ParameterDescriptions;

            for ( var j = 0; j < parameters.Count; j++ )
            {
                var parameter = parameters[j];

                if ( parameter.Source == BindingSource.Body )
                {
                    parameter.ModelMetadata = Describe( parameter.ModelMetadata, parameter.Type, apiVersion, minimalApi );
                }
            }

            var responseTypes = result.SupportedResponseTypes;

            for ( var j = 0; j < responseTypes.Count; j++ )
            {
                var responseType = responseTypes[j];

                responseType.ModelMetadata = Describe( responseType.ModelMetadata, responseType.Type, apiVersion, minimalApi );
            }
        }
    }

    [return: NotNullIfNotNull( nameof( metadata ) )]
    private ModelMetadata? Describe( ModelMetadata? metadata, Type? type, ApiVersion apiVersion, bool minimalApi )
    {
        // metadata that already describes an API version belongs to another provider
        if ( metadata?.DescribedApiVersion is not null )
        {
            return metadata;
        }

        // the metadata reported for a minimal API is a placeholder that never reports any members, so complete
        // metadata is resolved for the described type instead. this is specific to how minimal APIs are described
        // today and becomes a no-op if that ever reports real metadata
        if ( minimalApi && type is not null && metadata is null or { Properties.Count: 0, ElementMetadata: null } )
        {
            metadata = modelMetadataProvider.GetMetadataForType( type );
        }

        // a type with no members of its own has nothing to filter
        if ( metadata is null or { Properties.Count: 0, ElementMetadata: null } )
        {
            return metadata;
        }

        return new VersionedModelMetadata( metadata, annotations, apiVersion );
    }
}