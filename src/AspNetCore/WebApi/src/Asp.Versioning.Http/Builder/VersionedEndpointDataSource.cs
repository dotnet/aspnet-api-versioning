// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

internal sealed class VersionedEndpointDataSource : EndpointDataSource
{
    private List<DefaultEndpointConventionBuilder>? builders;

    internal IEndpointConventionBuilder Add( EndpointBuilder endpointBuilder )
    {
        var builder = new DefaultEndpointConventionBuilder( endpointBuilder );

        builders ??= new();
        builders.Add( builder );

        return builder;
    }

    public override IReadOnlyList<Endpoint> Endpoints
    {
        get
        {
            if ( builders == null )
            {
                return Array.Empty<Endpoint>();
            }

            return builders.Select( b => b.Build() ).ToArray();
        }
    }

    public override IChangeToken GetChangeToken() => NullChangeToken.Singleton;
}