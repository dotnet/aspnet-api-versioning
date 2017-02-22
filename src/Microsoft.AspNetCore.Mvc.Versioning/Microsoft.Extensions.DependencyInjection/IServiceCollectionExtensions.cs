namespace Microsoft.Extensions.DependencyInjection
{
    using AspNetCore.Mvc;
    using AspNetCore.Mvc.Infrastructure;
    using AspNetCore.Mvc.Routing;
    using AspNetCore.Mvc.Versioning;
    using Extensions;
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
            Contract.Ensures( Contract.Result<IServiceCollection>() != null );

            var options = new ApiVersioningOptions();

            setupAction( options );
            services.Add( new ServiceDescriptor( typeof( IApiVersionReader ), options.ApiVersionReader ) );
            services.Add( new ServiceDescriptor( typeof( IApiVersionSelector ), options.ApiVersionSelector ) );
            services.Add( Singleton<IOptions<ApiVersioningOptions>>( new OptionsWrapper<ApiVersioningOptions>( options ) ) );
            services.Replace( Singleton<IActionSelector, ApiVersionActionSelector>() );

            if ( options.ReportApiVersions )
            {
                services.AddSingleton<ReportApiVersionsAttribute>();
            }

            services.AddMvc(
                mvcOptions =>
                {
                    if ( options.ReportApiVersions )
                    {
                        mvcOptions.Filters.Add( typeof( ReportApiVersionsAttribute ) );
                    }

                    mvcOptions.Conventions.Add( new ApiVersionConvention( options.DefaultApiVersion, options.Conventions ) );
                } );

            services.AddRouting( mvcOptions => mvcOptions.ConstraintMap.Add( "apiVersion", typeof( ApiVersionRouteConstraint ) ) );

            return services;
        }
    }
}