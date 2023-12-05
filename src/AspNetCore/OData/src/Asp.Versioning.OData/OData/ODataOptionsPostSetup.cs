// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Asp.Versioning.Routing;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Represents the API versioning <see cref="IPostConfigureOptions{T}">post configuration</see> to
/// set up <see cref="ODataOptions">OData options</see>.
/// </summary>
[CLSCompliant( false )]
public class ODataOptionsPostSetup : IPostConfigureOptions<ODataOptions>
{
    private readonly ILoggerFactory loggerFactory;
    private readonly IODataPathTemplateParser parser;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataOptionsPostSetup"/> class.
    /// </summary>
    /// <param name="loggerFactory">The associated logger factory.</param>
    /// <param name="parser">The OData path template parser.</param>
    public ODataOptionsPostSetup(
        ILoggerFactory loggerFactory,
        IODataPathTemplateParser parser )
    {
        this.loggerFactory = loggerFactory;
        this.parser = parser;
    }

    /// <inheritdoc />
    public void PostConfigure( string? name, ODataOptions options )
    {
        ArgumentNullException.ThrowIfNull( options );

        var conventions = options.Conventions;
        var replacements = 0;

        for ( var i = 0; i < conventions.Count; i++ )
        {
            var convention = conventions[i];

            if ( convention is MetadataRoutingConvention )
            {
                if ( convention is not VersionedMetadataRoutingConvention )
                {
                    conventions[i] = new VersionedMetadataRoutingConvention();
                }

                if ( ++replacements >= 2 )
                {
                    break;
                }
            }
            else if ( convention is AttributeRoutingConvention )
            {
                if ( convention is not VersionedAttributeRoutingConvention )
                {
                    conventions[i] = new VersionedAttributeRoutingConvention(
                        loggerFactory.CreateLogger<AttributeRoutingConvention>(),
                        parser );
                }

                if ( ++replacements >= 2 )
                {
                    break;
                }
            }
        }
    }
}