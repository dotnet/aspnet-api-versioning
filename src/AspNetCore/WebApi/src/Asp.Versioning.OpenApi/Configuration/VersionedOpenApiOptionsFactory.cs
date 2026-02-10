// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1812

namespace Asp.Versioning.OpenApi.Configuration;

using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

// OpenApiOptions is sealed so we can't inherit from it, but we need to get in front of it. this factory allows
// configuring VersionedOpenApiOptions registered when the services are added. IOptions<VersionedOpenApiOptions> isn't
// ever directly resolved and never uses a name. whenever OpenApiOptions are created and configured, this factory is
// invoked to create the VersionedOpenApiOptions, which will be passed down to transformers, etc.
internal sealed class VersionedOpenApiOptionsFactory(
    IEnumerable<IConfigureOptions<VersionedOpenApiOptions>> setups,
    IEnumerable<IPostConfigureOptions<VersionedOpenApiOptions>> postConfigures,
    IEnumerable<IValidateOptions<VersionedOpenApiOptions>> validations )
    : IOptionsFactory<VersionedOpenApiOptions>
{
    private readonly IConfigureOptions<VersionedOpenApiOptions>[] setups = [.. setups];
    private readonly IPostConfigureOptions<VersionedOpenApiOptions>[] postConfigures = [.. postConfigures];
    private readonly IValidateOptions<VersionedOpenApiOptions>[] validations = [.. validations];
    private Context? context;

    internal VersionedOpenApiOptions CreateAndConfigure( Context newContext )
    {
        context = newContext;
        var instance = Create( newContext.Name );
        context = default;
        return instance;
    }

    public VersionedOpenApiOptions Create( string name )
    {
        if ( string.IsNullOrEmpty( name ) || context is null )
        {
            return DefaultOptions();
        }

        if ( name != context.Name )
        {
            return DefaultOptions();
        }

        var options = new VersionedOpenApiOptions()
        {
            Description = context.Description,
            Document = context.Options,
            DocumentDescription = new(),
        };

        context.OnCreated( options );

        for ( var i = 0; i < setups.Length; i++ )
        {
            setups[i].Configure( options );
        }

        for ( var i = 0; i < postConfigures.Length; i++ )
        {
            postConfigures[i].PostConfigure( Options.DefaultName, options );
        }

        if ( validations.Length > 0 )
        {
            var failures = new List<string>();

            for ( var i = 0; i < validations.Length; i++ )
            {
                var result = validations[i].Validate( Options.DefaultName, options );

                if ( result is not null && result.Failed )
                {
                    failures.AddRange( result.Failures );
                }
            }

            if ( failures.Count > 0 )
            {
                throw new OptionsValidationException( name, typeof( VersionedOpenApiOptions ), failures );
            }
        }

        return options;
    }

    private static VersionedOpenApiOptions DefaultOptions() => new()
    {
        Description = new( ApiVersion.Neutral, string.Empty ),
        Document = new(),
        DocumentDescription = new(),
    };

    internal sealed class Context
    {
        public required string Name { get; init; }

        public required ApiVersionDescription Description { get; init; }

        public required OpenApiOptions Options { get; init; }

        public required Action<VersionedOpenApiOptions> OnCreated { get; init; }
    }
}