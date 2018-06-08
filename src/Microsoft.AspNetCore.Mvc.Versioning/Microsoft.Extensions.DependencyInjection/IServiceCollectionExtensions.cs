namespace Microsoft.Extensions.DependencyInjection
{
    using AspNetCore.Mvc;
    using AspNetCore.Mvc.Infrastructure;
    using AspNetCore.Mvc.Routing;
    using AspNetCore.Mvc.Versioning;
    using Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Options;
    using System;
    using System.Diagnostics.Contracts;
    using static ServiceDescriptor;

    /// <summary>
    /// Provides extension methods for the <see cref="IServiceCollection"/> interface.
    /// </summary>
    [CLSCompliant( false )]
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds service API versioning to the specified services collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection">services</see> available in the application.</param>
        /// <returns>The original <paramref name="services"/> object.</returns>
        public static IServiceCollection AddApiVersioning( this IServiceCollection services ) => services.AddApiVersioning( _ => { } );

        /// <summary>
        /// Adds service API versioning to the specified services collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection">services</see> available in the application.</param>
        /// <param name="setupAction">An <see cref="Action{T}">action</see> used to configure the provided options.</param>
        /// <returns>The original <paramref name="services"/> object.</returns>
        public static IServiceCollection AddApiVersioning( this IServiceCollection services, Action<ApiVersioningOptions> setupAction )
        {
            Arg.NotNull( services, nameof( services ) );
            Arg.NotNull( setupAction, nameof( setupAction ) );
            Contract.Ensures( Contract.Result<IServiceCollection>() != null );

            var options = new ApiVersioningOptions();

            setupAction( options );
            services.Add( new ServiceDescriptor( typeof( IApiVersionReader ), options.ApiVersionReader ) );
            services.Add( new ServiceDescriptor( typeof( IApiVersionSelector ), options.ApiVersionSelector ) );
            services.Add( new ServiceDescriptor( typeof( IErrorResponseProvider ), options.ErrorResponses ) );
            services.Add( Singleton<IOptions<ApiVersioningOptions>>( new OptionsWrapper<ApiVersioningOptions>( options ) ) );
            services.Replace( Singleton<IActionSelector, ApiVersionActionSelector>() );
            services.TryAddSingleton<ReportApiVersionsAttribute>();
            services.AddTransient<IStartupFilter, AutoRegisterMiddleware>();
            services.AddMvcCore( mvcOptions => AddMvcOptions( mvcOptions, options ) );
            services.AddRouting( routeOptions => routeOptions.ConstraintMap.Add( options.RouteConstraintName, typeof( ApiVersionRouteConstraint ) ) );

            if ( options.RegisterMiddleware )
            {
                services.TryAddSingleton<IApiVersionRoutePolicy, DefaultApiVersionRoutePolicy>();
            }

            if ( options.ReportApiVersions )
            {
                services.TryAddSingleton<IReportApiVersions, DefaultApiVersionReporter>();
                services.AddTransient<IActionDescriptorProvider, ApiVersionCollator>();
            }
            else
            {
                services.TryAddSingleton<IReportApiVersions, DoNotReportApiVersions>();
            }

            return services;
        }

        static void AddMvcOptions( MvcOptions mvcOptions, ApiVersioningOptions options )
        {
            Contract.Requires( mvcOptions != null );
            Contract.Requires( options != null );

            if ( options.ReportApiVersions )
            {
                mvcOptions.Filters.AddService<ReportApiVersionsAttribute>();
            }

            mvcOptions.Conventions.Add( new ApiVersionConvention( options.DefaultApiVersion, options.Conventions ) );
        }

        sealed class AutoRegisterMiddleware : IStartupFilter
        {
            readonly IApiVersionRoutePolicy routePolicy;

            public AutoRegisterMiddleware( IApiVersionRoutePolicy routePolicy ) => this.routePolicy = routePolicy;

            public Action<IApplicationBuilder> Configure( Action<IApplicationBuilder> configure )
            {
                Contract.Requires( configure != null );
                Contract.Ensures( Contract.Result<Action<IApplicationBuilder>>() != null );

                return app =>
                {
                    app.UseApiVersioning();
                    configure( app );
                    app.UseRouter( builder => builder.Routes.Add( routePolicy ) );
                };
            }
        }
    }
}