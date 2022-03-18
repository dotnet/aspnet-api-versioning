// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if NETFRAMEWORK
using System.Web.Http.Controllers;
using ControllerModel = System.Web.Http.Controllers.HttpControllerDescriptor;
#else
using Microsoft.AspNetCore.Mvc.ApplicationModels;
#endif

/// <summary>
/// Defines the behavior of an API version convention builder.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public interface IApiVersionConventionBuilder
{
    /// <summary>
    /// Gets the count of configured conventions.
    /// </summary>
    /// <value>The total count of configured conventions.</value>
    int Count { get; }

    /// <summary>
    /// Gets or creates the convention builder for the specified controller.
    /// </summary>
    /// <param name="controllerType">The <see cref="Type">type</see> of controller to build conventions for.</param>
    /// <returns>A new or existing <see cref="IControllerConventionBuilder"/>.</returns>
    IControllerConventionBuilder Controller( Type controllerType );

    /// <summary>
    /// Gets or creates the convention builder for the specified controller.
    /// </summary>
    /// <typeparam name="TController">The <see cref="Type">type</see> of controller to build conventions for.</typeparam>
    /// <returns>A new or existing <see cref="IControllerConventionBuilder{T}"/>.</returns>
    IControllerConventionBuilder<TController> Controller<TController>()
#if NETFRAMEWORK
        where TController : notnull, IHttpController;
#else
        where TController : notnull;
#endif

    /// <summary>
    /// Applies the defined API version conventions to the specified controller.
    /// </summary>
    /// <param name="controller">The <see cref="ControllerModel">controller</see> to apply configured conventions to.</param>
    /// <returns>True if any conventions were applied to the <paramref name="controller">controller</paramref>;
    /// otherwise, false.</returns>
    bool ApplyTo( ControllerModel controller );

    /// <summary>
    /// Adds a new convention applied to all controllers.
    /// </summary>
    /// <param name="convention">The <see cref="IControllerConvention">convention</see> to be applied.</param>
    void Add( IControllerConvention convention );
}