// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Asp.Versioning;
using Asp.Versioning.Conventions;
using Asp.Versioning.OData;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Runtime.CompilerServices;
using Opts = Microsoft.Extensions.Options.Options;

/// <summary>
/// Represents an API description provider for partial OData support.
/// </summary>
[CLSCompliant( false )]
public class PartialODataDescriptionProvider : IApiDescriptionProvider
{
    private static readonly int BeforeOData = ODataOrder() + 10;
    private readonly IOptionsFactory<ODataOptions> odataOptionsFactory;
    private readonly IOptions<ODataApiExplorerOptions> options;
    private bool markedAdHoc;
    private IODataQueryOptionsConvention[]? conventions;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartialODataDescriptionProvider"/> class.
    /// </summary>
    /// <param name="odataOptionsFactory">The <see cref="IOptionsFactory{TOptions}">factory</see> used to create
    /// <see cref="ODataOptions">OData options</see>.</param>
    /// <param name="options">The <see cref="IOptions{TOptions}">container</see> of configured
    /// <see cref="ODataApiExplorerOptions">API explorer options</see>.</param>
    public PartialODataDescriptionProvider(
        IOptionsFactory<ODataOptions> odataOptionsFactory,
        IOptions<ODataApiExplorerOptions> options )
    {
        this.odataOptionsFactory = odataOptionsFactory ?? throw new ArgumentNullException( nameof( odataOptionsFactory ) );
        this.options = options ?? throw new ArgumentNullException( nameof( options ) );
    }

    /// <summary>
    /// Gets the associated OData API explorer options.
    /// </summary>
    /// <value>The current <see cref="ODataApiExplorerOptions">OData API explorer options</see>.</value>
    protected ODataApiExplorerOptions Options
    {
        get
        {
            var value = options.Value;

            if ( !markedAdHoc )
            {
                value.AdHocModelBuilder.OnModelCreated += MarkAsAdHoc;
                markedAdHoc = true;
            }

            return value;
        }
    }

    /// <summary>
    /// Gets the builder used to create ad hoc Entity Data Models (EDMs).
    /// </summary>
    /// <value>The associated <see cref="VersionedODataModelBuilder">model builder</see>.</value>
    protected VersionedODataModelBuilder ModelBuilder => Options.AdHocModelBuilder;

    /// <summary>
    /// Gets associated the OData query option conventions.
    /// </summary>
    /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of
    /// <see cref="IODataQueryOptionsConvention">OData query option conventions</see>.</value>
    protected IReadOnlyList<IODataQueryOptionsConvention> Conventions =>
        conventions ??= Options.AdHocModelBuilder.ModelConfigurations.OfType<IODataQueryOptionsConvention>().ToArray();

    /// <summary>
    /// Gets or sets the order precedence of the current API description provider.
    /// </summary>
    /// <value>The order precedence of the current API description provider.</value>
    public int Order { get; protected set; } = BeforeOData;

    /// <inheritdoc />
    public virtual void OnProvidersExecuting( ApiDescriptionProviderContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var results = FilterResults( context.Results, Conventions );

        if ( results.Length == 0 )
        {
            return;
        }

        var models = ModelBuilder.GetEdmModels();

        for ( var i = 0; i < models.Count; i++ )
        {
            var model = models[i];
            var version = model.GetApiVersion();
            var odata = odataOptionsFactory.Create( Opts.DefaultName );

            odata.AddRouteComponents( model );

            for ( var j = 0; j < results.Length; j++ )
            {
                var result = results[j];
                var metadata = result.ActionDescriptor.GetApiVersionMetadata();

                if ( metadata.IsMappedTo( version ) )
                {
                    result.ActionDescriptor.EndpointMetadata.Add( ODataMetadata.New( model ) );
                }
            }
        }
    }

    /// <inheritdoc />
    public virtual void OnProvidersExecuted( ApiDescriptionProviderContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var actions = context.Actions;

        for ( var i = 0; i < actions.Count; i++ )
        {
            var metadata = actions[i].EndpointMetadata;

            for ( var j = metadata.Count - 1; j >= 0; j-- )
            {
                if ( metadata[j] is IODataRoutingMetadata routing && routing.Model.IsAdHoc() )
                {
                    metadata.Remove( j );
                }
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static int ODataOrder() =>
        new ODataApiDescriptionProvider(
            new StubModelMetadataProvider(),
            new StubModelTypeBuilder(),
            new OptionsFactory<ODataOptions>( [], [] ),
            Opts.Create(
                new ODataApiExplorerOptions(
                    new( new StubODataApiVersionCollectionProvider(), [] ) ) ) ).Order;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void MarkAsAdHoc( ODataModelBuilder builder, IEdmModel model ) =>
        model.SetAnnotationValue( model, AdHocAnnotation.Instance );

    private static ApiDescription[] FilterResults(
        IList<ApiDescription> results,
        IReadOnlyList<IODataQueryOptionsConvention> conventions )
    {
        var filtered = default( List<ApiDescription> );

        for ( var i = 0; i < results.Count; i++ )
        {
            var result = results[i];
            var metadata = result.ActionDescriptor.EndpointMetadata;
            var odata = false;

            for ( var j = 0; j < metadata.Count; j++ )
            {
                if ( metadata[j] is IODataRoutingMetadata )
                {
                    odata = true;
                    break;
                }
            }

            if ( odata || !result.IsODataLike() )
            {
                continue;
            }

            filtered ??= new( capacity: results.Count );
            filtered.Add( result );

            for ( var j = 0; j < conventions.Count; j++ )
            {
                conventions[j].ApplyTo( result );
            }
        }

        return filtered?.ToArray() ?? [];
    }

    private sealed class StubModelMetadataProvider : IModelMetadataProvider
    {
        public IEnumerable<ModelMetadata> GetMetadataForProperties( Type modelType ) =>
            throw new NotImplementedException();

        public ModelMetadata GetMetadataForType( Type modelType ) =>
            throw new NotImplementedException();
    }

    private sealed class StubModelTypeBuilder : IModelTypeBuilder
    {
        public Type NewActionParameters( IEdmModel model, IEdmAction action, string controllerName, ApiVersion apiVersion ) =>
            throw new NotImplementedException();

        public Type NewStructuredType( IEdmModel model, IEdmStructuredType structuredType, Type clrType, ApiVersion apiVersion ) =>
            throw new NotImplementedException();
    }

    private sealed class StubODataApiVersionCollectionProvider : IODataApiVersionCollectionProvider
    {
        public IReadOnlyList<ApiVersion> ApiVersions
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }

    private static class ODataMetadata
    {
        private const string ArbitrarySegment = "52459ff8-bca1-4a26-b7f2-08c7da04472d";

        // metadata (~/$metadata) and service (~/) doc have special handling.
        // make sure we don't match the service doc
        private static readonly ODataPathTemplate AdHocODataTemplate =
            new( new DynamicSegmentTemplate( new( ArbitrarySegment ) ) );

        public static ODataRoutingMetadata New( IEdmModel model ) => new( string.Empty, model, AdHocODataTemplate );
    }
}