// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning.OData;
using Microsoft.Extensions.Options;
using static Asp.Versioning.ApiVersionMapping;

/// <summary>
/// Represents a factory to create OData API explorer options.
/// </summary>
[CLSCompliant( false )]
public class ODataApiExplorerOptionsFactory : ApiExplorerOptionsFactory<ODataApiExplorerOptions>
{
    private readonly IApiVersionMetadataCollationProvider[] providers;
    private readonly IEnumerable<IModelConfiguration> modelConfigurations;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataApiExplorerOptionsFactory"/> class.
    /// </summary>
    /// <param name="providers">A <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IApiVersionMetadataCollationProvider">providers</see> used to collate API version metadata.</param>
    /// <param name="modelConfigurations">A <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IModelConfiguration">configurations</see> used to configure Entity Data Models.</param>
    /// <param name="options">The <see cref="ApiVersioningOptions">API versioning options</see>
    /// used to create API explorer options.</param>
    /// <param name="setups">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IConfigureOptions{TOptions}">configuration actions</see> to run.</param>
    /// <param name="postConfigures">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IPostConfigureOptions{TOptions}">initialization actions</see> to run.</param>
    public ODataApiExplorerOptionsFactory(
        IEnumerable<IApiVersionMetadataCollationProvider> providers,
        IEnumerable<IModelConfiguration> modelConfigurations,
        IOptions<ApiVersioningOptions> options,
        IEnumerable<IConfigureOptions<ODataApiExplorerOptions>> setups,
        IEnumerable<IPostConfigureOptions<ODataApiExplorerOptions>> postConfigures )
        : base( options, setups, postConfigures )
    {
        this.providers = ( providers ?? throw new ArgumentNullException( nameof( providers ) ) ).ToArray();
        this.modelConfigurations = modelConfigurations ?? throw new ArgumentNullException( nameof( modelConfigurations ) );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataApiExplorerOptionsFactory"/> class.
    /// </summary>
    /// <param name="providers">A <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IApiVersionMetadataCollationProvider">providers</see> used to collate API version metadata.</param>
    /// <param name="modelConfigurations">A <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IModelConfiguration">configurations</see> used to configure Entity Data Models.</param>
    /// <param name="options">The <see cref="ApiVersioningOptions">API versioning options</see>
    /// used to create API explorer options.</param>
    /// <param name="setups">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IConfigureOptions{TOptions}">configuration actions</see> to run.</param>
    /// <param name="postConfigures">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IPostConfigureOptions{TOptions}">initialization actions</see> to run.</param>
    /// <param name="validations">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IValidateOptions{TOptions}">validations</see> to run.</param>
    public ODataApiExplorerOptionsFactory(
        IEnumerable<IApiVersionMetadataCollationProvider> providers,
        IEnumerable<IModelConfiguration> modelConfigurations,
        IOptions<ApiVersioningOptions> options,
        IEnumerable<IConfigureOptions<ODataApiExplorerOptions>> setups,
        IEnumerable<IPostConfigureOptions<ODataApiExplorerOptions>> postConfigures,
        IEnumerable<IValidateOptions<ODataApiExplorerOptions>> validations )
        : base( options, setups, postConfigures, validations )
    {
        this.providers = ( providers ?? throw new ArgumentNullException( nameof( providers ) ) ).ToArray();
        this.modelConfigurations = modelConfigurations ?? throw new ArgumentNullException( nameof( modelConfigurations ) );
    }

    /// <inheritdoc />
    protected override ODataApiExplorerOptions CreateInstance( string name )
    {
        var options = new ODataApiExplorerOptions( new( CollateApiVersions( providers, Options ), modelConfigurations ) );
        CopyOptions( Options, options );
        return options;
    }

    private static ODataApiVersionCollectionProvider CollateApiVersions(
        IApiVersionMetadataCollationProvider[] providers,
        ApiVersioningOptions options )
    {
        var context = new ApiVersionMetadataCollationContext();

        for ( var i = 0; i < providers.Length; i++ )
        {
            providers[i].Execute( context );
        }

        var results = context.Results;
        var versions = new SortedSet<ApiVersion>();

        for ( var i = 0; i < results.Count; i++ )
        {
            var model = results[i].Map( Implicit );
            var declared = model.DeclaredApiVersions;

            for ( var j = 0; j < declared.Count; j++ )
            {
                versions.Add( declared[j] );
            }
        }

        if ( versions.Count == 0 )
        {
            versions.Add( options.DefaultApiVersion );
        }

        return new() { ApiVersions = versions.ToArray() };
    }

    private sealed class ODataApiVersionCollectionProvider : IODataApiVersionCollectionProvider
    {
        public required IReadOnlyList<ApiVersion> ApiVersions { get; set; }
    }
}