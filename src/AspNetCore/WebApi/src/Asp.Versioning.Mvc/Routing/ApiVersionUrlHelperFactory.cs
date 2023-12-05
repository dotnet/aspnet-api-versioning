// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

/// <summary>
/// Represents an API version aware <see cref="IUrlHelperFactory">URL helper factory</see>.
/// </summary>
[CLSCompliant( false )]
public class ApiVersionUrlHelperFactory : IUrlHelperFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionUrlHelperFactory"/> class.
    /// </summary>
    /// <param name="factory">The inner <see cref="IUrlHelperFactory">URL helper factory</see>.</param>
    public ApiVersionUrlHelperFactory( IUrlHelperFactory factory ) => Factory = factory;

    /// <summary>
    /// Gets the inner factory used to create URL helpers.
    /// </summary>
    /// <value>The inner <see cref="IUrlHelperFactory">URL helper factory</see>.</value>
    protected IUrlHelperFactory Factory { get; }

    /// <inheritdoc />
    public virtual IUrlHelper GetUrlHelper( ActionContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var items = context.HttpContext.Items;

        // REF: https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Core/src/Routing/UrlHelperFactory.cs#L44
        if ( !items.TryGetValue( typeof( IUrlHelper ), out var value ) )
        {
            var urlHelper = new ApiVersionUrlHelper( context, Factory.GetUrlHelper( context ) );
            items[typeof( IUrlHelper )] = urlHelper;
            return urlHelper;
        }

        if ( value is not ApiVersionUrlHelper outer )
        {
            var inner = value as IUrlHelper ?? Factory.GetUrlHelper( context );
            items[typeof( IUrlHelper )] = outer = new ApiVersionUrlHelper( context, inner );
        }

        return outer;
    }
}