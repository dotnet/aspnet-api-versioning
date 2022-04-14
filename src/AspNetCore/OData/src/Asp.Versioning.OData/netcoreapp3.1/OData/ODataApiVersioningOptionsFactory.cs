// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.Extensions.Options;

/// <summary>
/// Represents a factory to create API versioning options specific to OData.
/// </summary>
[CLSCompliant( false )]
public class ODataApiVersioningOptionsFactory : IOptionsFactory<ODataApiVersioningOptions>
{
    private readonly IConfigureOptions<ODataApiVersioningOptions>[] setups;
    private readonly IPostConfigureOptions<ODataApiVersioningOptions>[] postConfigures;
    private readonly VersionedODataModelBuilder modelBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataApiVersioningOptionsFactory"/> class.
    /// </summary>
    /// <param name="modelBuilder">The associated <see cref="VersionedODataModelBuilder">model builder</see>.</param>
    /// <param name="setups">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IConfigureOptions{TOptions}">configuration actions</see> to run.</param>
    /// <param name="postConfigures">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IPostConfigureOptions{TOptions}">initialization actions</see> to run.</param>
    public ODataApiVersioningOptionsFactory(
        VersionedODataModelBuilder modelBuilder,
        IEnumerable<IConfigureOptions<ODataApiVersioningOptions>> setups,
        IEnumerable<IPostConfigureOptions<ODataApiVersioningOptions>> postConfigures )
    {
        this.setups = setups as IConfigureOptions<ODataApiVersioningOptions>[] ?? new List<IConfigureOptions<ODataApiVersioningOptions>>( setups ).ToArray();
        this.postConfigures = postConfigures as IPostConfigureOptions<ODataApiVersioningOptions>[] ?? new List<IPostConfigureOptions<ODataApiVersioningOptions>>( postConfigures ).ToArray();
        this.modelBuilder = modelBuilder ?? throw new ArgumentNullException( nameof( modelBuilder ) );
    }

    /// <inheritdoc />
    public virtual ODataApiVersioningOptions Create( string name )
    {
        var options = new ODataApiVersioningOptions( modelBuilder );

        foreach ( var setup in setups )
        {
            if ( setup is IConfigureNamedOptions<ODataApiVersioningOptions> namedSetup )
            {
                namedSetup.Configure( name, options );
            }
            else if ( name == Options.DefaultName )
            {
                setup.Configure( options );
            }
        }

        foreach ( var post in postConfigures )
        {
            post.PostConfigure( name, options );
        }

        return options;
    }
}