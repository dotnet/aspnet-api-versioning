// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.OData.Edm;

/// <summary>
/// Represents a type substitution context.
/// </summary>
public class TypeSubstitutionContext
{
    private ApiVersion? apiVersion;

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeSubstitutionContext"/> class.
    /// </summary>
    /// <param name="model">The <see cref="IEdmModel">EDM model</see> to compare against.</param>
    /// <param name="modelTypeBuilder">The associated <see cref="IModelTypeBuilder">model type builder</see>.</param>
    /// <param name="apiVersion">The optional <see cref="ApiVersion">API version</see>.</param>
    /// <remarks>If <paramref name="apiVersion"/> is unspecified, it will be derived from the <see cref="ApiVersionAnnotation"/>
    /// in the <paramref name="model"/>.</remarks>
    public TypeSubstitutionContext( IEdmModel model, IModelTypeBuilder modelTypeBuilder, ApiVersion? apiVersion = default )
    {
        Model = model;
        ModelTypeBuilder = modelTypeBuilder;
        this.apiVersion = apiVersion;
    }

    /// <summary>
    /// Gets the source Entity Data Model (EDM).
    /// </summary>
    /// <value>The associated <see cref="IEdmModel">EDM model</see> compared against for substitutions.</value>
    public IEdmModel Model { get; }

    /// <summary>
    /// Gets API version associated with the source model.
    /// </summary>
    /// <value>The associated <see cref="ApiVersion">API version</see>.</value>
    public ApiVersion ApiVersion => apiVersion ??= Model.GetApiVersion() ?? ApiVersion.Neutral;

    /// <summary>
    /// Gets the model type builder used to create substitution types.
    /// </summary>
    /// <value>The associated <see cref="IModelTypeBuilder">model type builder</see>.</value>
    public IModelTypeBuilder ModelTypeBuilder { get; }
}