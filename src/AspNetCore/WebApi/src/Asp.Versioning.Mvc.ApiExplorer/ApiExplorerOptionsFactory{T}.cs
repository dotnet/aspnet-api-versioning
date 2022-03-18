// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.Extensions.Options;
using Option = Microsoft.Extensions.Options.Options;

/// <summary>
/// Represents a factory to create API explorer options.
/// </summary>
/// <typeparam name="T">The type of options to create.</typeparam>
[CLSCompliant( false )]
public class ApiExplorerOptionsFactory<T> : IOptionsFactory<T> where T : ApiExplorerOptions, new()
{
    private readonly IOptions<ApiVersioningOptions> optionsHolder;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiExplorerOptionsFactory{T}"/> class.
    /// </summary>
    /// <param name="options">The <see cref="ApiVersioningOptions">API versioning options</see>
    /// used to create API explorer options.</param>
    /// <param name="setups">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IConfigureOptions{TOptions}">configuration actions</see> to run.</param>
    /// <param name="postConfigures">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IPostConfigureOptions{TOptions}">initialization actions</see> to run.</param>
    public ApiExplorerOptionsFactory(
        IOptions<ApiVersioningOptions> options,
        IEnumerable<IConfigureOptions<T>> setups,
        IEnumerable<IPostConfigureOptions<T>> postConfigures )
    {
        optionsHolder = options;
        Setups = setups;
        PostConfigures = postConfigures;
    }

    /// <summary>
    /// Gets the API versioning options associated with the factory.
    /// </summary>
    /// <value>The <see cref="ApiVersioningOptions">API versioning options</see> used to create API explorer options.</value>
    protected ApiVersioningOptions Options => optionsHolder.Value;

    /// <summary>
    /// Gets the associated configuration actions to run.
    /// </summary>
    /// <value>The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IConfigureOptions{TOptions}">configuration actions</see> to run.</value>
    protected IEnumerable<IConfigureOptions<T>> Setups { get; }

    /// <summary>
    /// Gets the associated initialization actions to run.
    /// </summary>
    /// <value>The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IPostConfigureOptions{TOptions}">initialization actions</see> to run.</value>
    protected IEnumerable<IPostConfigureOptions<T>> PostConfigures { get; }

    /// <inheritdoc />
    public virtual T Create( string name )
    {
        var apiVersioningOptions = Options;
        var options = new T()
        {
            DefaultApiVersion = apiVersioningOptions.DefaultApiVersion,
            ApiVersionParameterSource = apiVersioningOptions.ApiVersionReader,
            AssumeDefaultVersionWhenUnspecified = apiVersioningOptions.AssumeDefaultVersionWhenUnspecified,
        };

        foreach ( var setup in Setups )
        {
            if ( setup is IConfigureNamedOptions<T> namedSetup )
            {
                namedSetup.Configure( name, options );
            }
            else if ( name == Option.DefaultName )
            {
                setup.Configure( options );
            }
        }

        foreach ( var post in PostConfigures )
        {
            post.PostConfigure( name, options );
        }

        return options;
    }
}