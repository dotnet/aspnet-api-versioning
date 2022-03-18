// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

internal sealed class DefaultEndpointConventionBuilder : IEndpointConventionBuilder
{
    private readonly EndpointBuilder endpointBuilder;
    private List<Action<EndpointBuilder>>? original = new();

    public DefaultEndpointConventionBuilder( EndpointBuilder endpointBuilder ) => this.endpointBuilder = endpointBuilder;

    public void Add( Action<EndpointBuilder> convention )
    {
        var conventions = original ?? throw new InvalidOperationException( SR.ConventionAddedAfterEndpointBuilt );
        conventions.Add( convention );
    }

    public Endpoint Build()
    {
        if ( Interlocked.Exchange( ref original, null ) is List<Action<EndpointBuilder>> conventions )
        {
            for ( var i = 0; i < conventions.Count; i++ )
            {
                conventions[i]( endpointBuilder );
            }
        }

        return endpointBuilder.Build();
    }
}