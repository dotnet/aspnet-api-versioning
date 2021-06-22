namespace Microsoft.OData
{
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.Edm;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using static Microsoft.AspNet.OData.Routing.VersionedODataRoutingConventions;
    using static Microsoft.OData.ServiceLifetime;

    /// <summary>
    /// Provides extension methods for the <see cref="IContainerBuilder"/> interface.
    /// </summary>
    public static class IContainerBuilderExtensions
    {
        /// <summary>
        /// Adds service API versioning to the specified container builder.
        /// </summary>
        /// <param name="builder">The extended <see cref="IContainerBuilder">container builder</see>.</param>
        /// <param name="routeName">The name of the route to add API versioning to.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static IContainerBuilder AddApiVersioning( this IContainerBuilder builder, string routeName, IEnumerable<IEdmModel> models ) =>
            builder
                .AddService( Transient, sp => sp.GetRequiredService<IEdmModelSelector>().SelectModel( sp ) )
                .AddService(
                    Singleton,
                    sp =>
                    {
                        var options = sp.GetRequiredService<HttpConfiguration>().GetApiVersioningOptions();
                        return (IEdmModelSelector) new EdmModelSelector( models, options.DefaultApiVersion, options.ApiVersionSelector );
                    } )
                .AddService(
                    Singleton,
                    sp => CreateDefaultWithAttributeRouting(
                        routeName,
                        sp.GetRequiredService<HttpConfiguration>() ).AsEnumerable() );

        /// <summary>
        /// Adds service API versioning to the specified container builder.
        /// </summary>
        /// <param name="builder">The extended <see cref="IContainerBuilder">container builder</see>.</param>
        /// <param name="models">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IEdmModel">EDM models</see> to use for parsing OData paths.</param>
        /// <param name="routingConventions">The OData routing conventions to use for controller and action selection.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static IContainerBuilder AddApiVersioning(
            this IContainerBuilder builder,
            IEnumerable<IEdmModel> models,
            IEnumerable<IODataRoutingConvention> routingConventions ) =>
            builder
                .AddService( Transient, sp => sp.GetRequiredService<IEdmModelSelector>().SelectModel( sp ) )
                .AddService(
                    Singleton,
                    sp =>
                    {
                        var options = sp.GetRequiredService<HttpConfiguration>().GetApiVersioningOptions();
                        return (IEdmModelSelector) new EdmModelSelector( models, options.DefaultApiVersion, options.ApiVersionSelector );
                    } )
                .AddService( Singleton, sp => AddOrUpdate( routingConventions.ToList() ).AsEnumerable() );
    }
}