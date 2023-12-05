// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Asp.Versioning.ApplicationModels;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;

/// <summary>
/// Represents an <see cref="IApplicationModelProvider">application model provider</see>, which
/// applies convention-based API versions controllers and their actions.
/// </summary>
[CLSCompliant( false )]
public class ApiVersioningApplicationModelProvider : IApplicationModelProvider
{
    private readonly IOptions<ApiVersioningOptions> options;
    private readonly IOptions<MvcApiVersioningOptions> mvcOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersioningApplicationModelProvider"/> class.
    /// </summary>
    /// <param name="controllerFilter">The <see cref="IApiControllerFilter">filter</see> used for API controllers.</param>
    /// <param name="namingConvention">The <see cref="IControllerNameConvention">controller naming convention</see>.</param>
    /// <param name="options">The current <see cref="ApiVersioningOptions">API versioning options</see>.</param>
    /// <param name="mvcOptions">The MVC <see cref="MvcApiVersioningOptions">API versioning options</see>.</param>
    public ApiVersioningApplicationModelProvider(
        IApiControllerFilter controllerFilter,
        IControllerNameConvention namingConvention,
        IOptions<ApiVersioningOptions> options,
        IOptions<MvcApiVersioningOptions> mvcOptions )
    {
        ControllerFilter = controllerFilter;
        NamingConvention = namingConvention;
        this.options = options;
        this.mvcOptions = mvcOptions;
    }

    /// <inheritdoc />
    public int Order { get; protected set; }

    /// <summary>
    /// Gets the filter used for API controllers.
    /// </summary>
    /// <value>The <see cref="IApiControllerFilter"/> used to filter API controllers.</value>
    protected IApiControllerFilter ControllerFilter { get; }

    /// <summary>
    /// Gets the controller naming convention associated with the collator.
    /// </summary>
    /// <value>The <see cref="IControllerNameConvention">controller naming convention</see>.</value>
    protected IControllerNameConvention NamingConvention { get; }

    /// <summary>
    /// Gets the builder used to define API version conventions.
    /// </summary>
    /// <value>An <see cref="IApiVersionConventionBuilder">API version convention builder</see>.</value>
    protected IApiVersionConventionBuilder ConventionBuilder => mvcOptions.Value.Conventions;

    /// <summary>
    /// Gets the API versioning options associated with the model provider.
    /// </summary>
    /// <value>The current <see cref="ApiVersioningOptions">API versioning options</see>.</value>
    protected ApiVersioningOptions Options => options.Value;

    /// <inheritdoc />
    public virtual void OnProvidersExecuted( ApplicationModelProviderContext context ) { }

    /// <inheritdoc />
    public virtual void OnProvidersExecuting( ApplicationModelProviderContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var application = context.Result;
        var controllers = ControllerFilter.Apply( application.Controllers );

        for ( var i = 0; i < controllers.Count; i++ )
        {
            var controller = controllers[i];

            if ( controller.RouteValues.TryGetValue( "controller", out var name ) )
            {
                controller.ControllerName = name!;
            }
            else
            {
                controller.ControllerName = NamingConvention.NormalizeName( controller.ControllerName );
            }

            if ( !ConventionBuilder.ApplyTo( controller ) )
            {
                ApplyImplicitConventions( controller );
            }
        }
    }

    private static bool IsDecoratedWithAttributes( ControllerModel controller )
    {
        var attributes = controller.Attributes;

        for ( var i = 0; i < attributes.Count; i++ )
        {
            var attribute = attributes[i];

            if ( attribute is IApiVersionProvider || attribute is IApiVersionNeutral )
            {
                return true;
            }
        }

        return false;
    }

    private void ApplyImplicitConventions( ControllerModel controller )
    {
        var conventions = new ControllerApiVersionConventionBuilder( controller.ControllerType, NamingConvention );

        if ( !IsDecoratedWithAttributes( controller ) )
        {
            conventions.HasApiVersion( Options.DefaultApiVersion );
        }

        conventions.ApplyTo( controller );
    }
}