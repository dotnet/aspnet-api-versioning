namespace System.Web.Http
{
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using System.Reflection;
    using static System.Linq.Expressions.Expression;

    static class IContainerBuilderExtensions
    {
        private static readonly Lazy<Action<IContainerBuilder>> addDefaultWebApiServices = new Lazy<Action<IContainerBuilder>>( NewAddDefaultWebApiServicesFunc );

        internal static void InitializeAttributeRouting( this IServiceProvider serviceProvider ) => serviceProvider.GetServices<IODataRoutingConvention>();

        internal static void AddDefaultWebApiServices( this IContainerBuilder builder ) => addDefaultWebApiServices.Value( builder );

        static Action<IContainerBuilder> NewAddDefaultWebApiServicesFunc()
        {
            var type = Type.GetType( "Microsoft.AspNet.OData.Extensions.ContainerBuilderExtensions, Microsoft.AspNet.OData", throwOnError: true );
            var method = type.GetRuntimeMethod( nameof( AddDefaultWebApiServices ), new[] { typeof( IContainerBuilder ) } );
            var builder = Parameter( typeof( IContainerBuilder ), "builder" );
            var body = Call( null, method, builder );
            var lambda = Lambda<Action<IContainerBuilder>>( body, builder );

            return lambda.Compile();
        }
    }
}