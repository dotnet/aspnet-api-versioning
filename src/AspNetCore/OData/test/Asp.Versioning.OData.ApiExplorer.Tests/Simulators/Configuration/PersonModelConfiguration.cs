// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators.Configuration;

using Asp.Versioning.OData;
using Asp.Versioning.Simulators.Models;
using Microsoft.OData.ModelBuilder;

/// <summary>
/// Represents the model configuration for people.
/// </summary>
public class PersonModelConfiguration : IModelConfiguration
{
    /// <inheritdoc />
    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
    {
        ArgumentNullException.ThrowIfNull( builder );

        var person = builder.EntitySet<Person>( "People" ).EntityType.HasKey( p => p.Id );

        if ( apiVersion < ApiVersions.V3 )
        {
            person.Ignore( p => p.Phone );
        }

        if ( apiVersion <= ApiVersions.V1 )
        {
            person.Ignore( p => p.Email );
        }

        if ( apiVersion > ApiVersions.V1 )
        {
            var function = person.Collection.Function( "NewHires" );

            function.Parameter<DateTime>( "Since" );
            function.ReturnsFromEntitySet<Person>( "People" );
        }

        if ( apiVersion > ApiVersions.V2 )
        {
            person.Action( "Promote" ).Parameter<string>( "title" );
        }
    }
}