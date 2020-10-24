﻿namespace Microsoft.AspNetCore.Mvc.Routing
{
    using System;

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
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            var urlHelper = new ApiVersionUrlHelper( context, Factory.GetUrlHelper( context ) );

            // REF: https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Core/src/Routing/UrlHelperFactory.cs#L44
            context.HttpContext.Items[typeof( IUrlHelper )] = urlHelper;

            return urlHelper;
        }
    }
}