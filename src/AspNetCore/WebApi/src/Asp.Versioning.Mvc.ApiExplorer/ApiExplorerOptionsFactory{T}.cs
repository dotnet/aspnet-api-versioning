// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.Extensions.Options;

/// <summary>
/// Represents a factory to create API explorer options.
/// </summary>
/// <typeparam name="T">The type of options to create.</typeparam>
[CLSCompliant( false )]
public class ApiExplorerOptionsFactory<T> : OptionsFactory<T> where T : ApiExplorerOptions
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
        : base( setups, postConfigures ) => optionsHolder = options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiExplorerOptionsFactory{T}"/> class.
    /// </summary>
    /// <param name="options">The <see cref="ApiVersioningOptions">API versioning options</see>
    /// used to create API explorer options.</param>
    /// <param name="setups">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IConfigureOptions{TOptions}">configuration actions</see> to run.</param>
    /// <param name="postConfigures">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IPostConfigureOptions{TOptions}">initialization actions</see> to run.</param>
    /// <param name="validations">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IValidateOptions{TOptions}">validations</see> to run.</param>
    public ApiExplorerOptionsFactory(
        IOptions<ApiVersioningOptions> options,
        IEnumerable<IConfigureOptions<T>> setups,
        IEnumerable<IPostConfigureOptions<T>> postConfigures,
        IEnumerable<IValidateOptions<T>> validations )
        : base( setups, postConfigures, validations ) => optionsHolder = options;

    /// <summary>
    /// Gets the API versioning options associated with the factory.
    /// </summary>
    /// <value>The <see cref="ApiVersioningOptions">API versioning options</see> used to create API explorer options.</value>
    protected ApiVersioningOptions Options => optionsHolder.Value;

    /// <inheritdoc />
    protected override T CreateInstance( string name )
    {
        var options = base.CreateInstance( name );
        CopyOptions( Options, options );
        return options;
    }

    /// <summary>
    /// Copies the following source options to the target options.
    /// </summary>
    /// <param name="sourceOptions">The source options.</param>
    /// <param name="targetOptions">The target options.</param>
    protected static void CopyOptions( ApiVersioningOptions sourceOptions, T targetOptions )
    {
        ArgumentNullException.ThrowIfNull( targetOptions, nameof( targetOptions ) );
        ArgumentNullException.ThrowIfNull( sourceOptions, nameof( sourceOptions ) );

        targetOptions.AssumeDefaultVersionWhenUnspecified = sourceOptions.AssumeDefaultVersionWhenUnspecified;
        targetOptions.ApiVersionParameterSource = sourceOptions.ApiVersionReader;
        targetOptions.DefaultApiVersion = sourceOptions.DefaultApiVersion;
        targetOptions.RouteConstraintName = sourceOptions.RouteConstraintName;
        targetOptions.ApiVersionSelector = sourceOptions.ApiVersionSelector;
    }
}