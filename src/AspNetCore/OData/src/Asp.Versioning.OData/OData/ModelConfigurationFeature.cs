// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Represents the model configurations in an application.
/// </summary>
/// <remarks>The <see cref="ModelConfigurationFeature"/> can be populated using the <see cref="ApplicationPartManager"/>
/// that is available during startup at <see cref="IMvcBuilder.PartManager"/> and <see cref="IMvcCoreBuilder.PartManager"/>
/// or at a later stage by requiring the <see cref="ApplicationPartManager"/> as a dependency in a component.
/// </remarks>
[CLSCompliant( false )]
public class ModelConfigurationFeature
{
    private HashSet<Type>? modelConfigurations;

    /// <summary>
    /// Gets the collection of model configurations in an application.
    /// </summary>
    /// <value>The <see cref="ICollection{T}">collection</see> of model configurations in an application.</value>
    public ICollection<Type> ModelConfigurations => modelConfigurations ??= [];
}