// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi;

using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.OpenApi.Extensions;

#pragma warning disable

internal sealed class OpenApiVersionedDocumentNamesProvider : IAdditionalOpenApiDocumentNameProvider
{
    private readonly IApiVersionDescriptionProvider provider;

    public OpenApiVersionedDocumentNamesProvider( IApiVersionDescriptionProvider provider )
    {
        this.provider = provider;
    }

    public IEnumerable<string> DocumentNames
    {
        get
        {
            var descriptions = provider.ApiVersionDescriptions;
            var names = new string[descriptions.Count];
            for (var i = 0; i < names.Length; i++)
            {
                names[i] = descriptions[i].GroupName;
            }

            return names;
        }
    }
}