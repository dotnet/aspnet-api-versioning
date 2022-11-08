// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.Conventions;
using Microsoft.Extensions.Options;

/// <summary>
/// Represents a factory to create API versioning options specific to ASP.NET Core MVC.
/// </summary>
/// <typeparam name="T">The type of options to create.</typeparam>
[CLSCompliant( false )]
public class MvcApiVersioningOptionsFactory<T> : OptionsFactory<T> where T : MvcApiVersioningOptions, new()
{
    private readonly IApiVersionConventionBuilder conventionBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="MvcApiVersioningOptionsFactory{T}"/> class.
    /// </summary>
    /// <param name="conventionBuilder">The configured <see cref="IApiVersionConventionBuilder">convention builder</see>.</param>
    /// <param name="setups">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IConfigureOptions{TOptions}">configuration actions</see> to run.</param>
    /// <param name="postConfigures">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IPostConfigureOptions{TOptions}">initialization actions</see> to run.</param>
    public MvcApiVersioningOptionsFactory(
        IApiVersionConventionBuilder conventionBuilder,
        IEnumerable<IConfigureOptions<T>> setups,
        IEnumerable<IPostConfigureOptions<T>> postConfigures )
        : base( setups, postConfigures ) =>
        this.conventionBuilder = conventionBuilder ?? throw new ArgumentNullException( nameof( conventionBuilder ) );

    /// <inheritdoc />
    protected override T CreateInstance( string name ) => new() { Conventions = conventionBuilder };
}