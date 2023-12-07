// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

#if NETFRAMEWORK
using Microsoft.AspNet.OData.Builder;
#else
using Microsoft.OData.ModelBuilder;
#endif
using Microsoft.OData.Edm;

/// <summary>
/// Represents a versioned variant of the <see cref="ODataModelBuilder"/>.
/// </summary>
public partial class VersionedODataModelBuilder
{
    private List<IModelConfiguration>? modelConfigurations;

    /// <summary>
    /// Gets or sets the factory method used to create model builders.
    /// </summary>
    /// <value>The factory <see cref="Func{TResult}">method</see> used to create
    /// <see cref="ODataModelBuilder">model builders</see>.</value>
    /// <remarks>The default implementation creates default instances of the
    /// <see cref="ODataConventionModelBuilder"/> class.</remarks>
    public Func<ODataModelBuilder> ModelBuilderFactory { get; set; } =
        static () => new ODataConventionModelBuilder().EnableLowerCamelCase();

    /// <summary>
    /// Gets or sets the default model configuration.
    /// </summary>
    /// <value>The <see cref="Action{T1, T2, T3}">method</see> for the default model configuration.
    /// The default value is <c>null</c>.</value>
    public Action<ODataModelBuilder, ApiVersion, string?>? DefaultModelConfiguration { get; set; }

    /// <summary>
    /// Gets the list of model configurations associated with the builder.
    /// </summary>
    /// <value>A <see cref="IList{T}">list</see> of model configurations associated with the builder.</value>
    public IList<IModelConfiguration> ModelConfigurations => modelConfigurations ??= [];

    /// <summary>
    /// Gets or sets the action that is invoked after the <see cref="IEdmModel">EDM model</see> has been created.
    /// </summary>
    /// <value>The <see cref="Action{T1,T2}">action</see> to run after the model has been created. The default
    /// value is <c>null</c>.</value>
    public Action<ODataModelBuilder, IEdmModel>? OnModelCreated { get; set; }

    /// <summary>
    /// Builds and returns a read-only list of EDM models based on the defined model configurations.
    /// </summary>
    /// <param name="routePrefix">The route prefix associated with the configuration, if any.</param>
    /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="IEdmModel">EDM models</see>.</returns>
    public virtual IReadOnlyList<IEdmModel> GetEdmModels( string? routePrefix = default )
    {
        var configurations = GetMergedConfigurations();

        if ( configurations.Count == 0 )
        {
            return Array.Empty<IEdmModel>();
        }

        var apiVersions = GetApiVersions();
        var models = new List<IEdmModel>( capacity: apiVersions.Count );

        BuildModelPerApiVersion( apiVersions, configurations, models, routePrefix );

        return models;
    }

    private IReadOnlyList<IModelConfiguration> GetMergedConfigurations()
    {
        var defaultConfiguration = DefaultModelConfiguration;

        if ( defaultConfiguration == null )
        {
            if ( modelConfigurations == null )
            {
                return Array.Empty<IModelConfiguration>();
            }

            return modelConfigurations;
        }

        var delegatingConfiguration = new DelegatingModelConfiguration( defaultConfiguration );

        if ( modelConfigurations == null || modelConfigurations.Count == 0 )
        {
            return new[] { delegatingConfiguration };
        }

        var configurations = new IModelConfiguration[modelConfigurations.Count + 1];

        configurations[0] = delegatingConfiguration;
        modelConfigurations.CopyTo( configurations, 1 );

        return configurations;
    }

    private void BuildModelPerApiVersion(
        IReadOnlyList<ApiVersion> apiVersions,
        IReadOnlyList<IModelConfiguration> configurations,
        List<IEdmModel> models,
        string? routePrefix )
    {
        for ( var i = 0; i < apiVersions.Count; i++ )
        {
            var apiVersion = apiVersions[i];
            var builder = ModelBuilderFactory();

            for ( var j = 0; j < configurations.Count; j++ )
            {
                configurations[j].Apply( builder, apiVersion, routePrefix );
            }

            const int EntityContainerOnly = 1;
            var model = builder.GetEdmModel();
            var container = model.EntityContainer;
            var empty = model.SchemaElements.Count() == EntityContainerOnly;

            if ( empty )
            {
                continue;
            }

            model.SetAnnotationValue( model, new ApiVersionAnnotation( apiVersion ) );
            OnModelCreated?.Invoke( builder, model );
            models.Add( model );
        }
    }
}