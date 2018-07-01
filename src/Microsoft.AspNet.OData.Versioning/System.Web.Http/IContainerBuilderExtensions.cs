namespace System.Web.Http
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Formatter.Deserialization;
    using Microsoft.AspNet.OData.Formatter.Serialization;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNet.OData.Query.Expressions;
    using Microsoft.AspNet.OData.Query.Validators;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData;
    using static Microsoft.OData.ServiceLifetime;

    static class IContainerBuilderExtensions
    {
        internal static void InitializeAttributeRouting( this IServiceProvider serviceProvider ) => serviceProvider.GetServices<IODataRoutingConvention>();

        internal static void AddDefaultWebApiServices( this IContainerBuilder builder )
        {
            builder.AddService<IODataPathHandler, DefaultODataPathHandler>( Singleton );
            AddServicePrototypes( builder );
            AddQueryValidators( builder );
            AddSerializationProviders( builder );
            AddDeserializerServices( builder );
            AddSerializerServices( builder );
            AddBinders( builder );
            AddHttpRequestScope( builder );
        }

        static void AddServicePrototypes( IContainerBuilder builder )
        {
            builder.AddServicePrototype( new ODataMessageReaderSettings() { EnableMessageStreamDisposal = false, MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = long.MaxValue } } );
            builder.AddServicePrototype( new ODataMessageWriterSettings() { EnableMessageStreamDisposal = false, MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = long.MaxValue } } );
        }

        static void AddQueryValidators( IContainerBuilder builder )
        {
            builder.AddService<CountQueryValidator>( Singleton );
            builder.AddService<FilterQueryValidator>( Scoped );
            builder.AddService<ODataQueryValidator>( Singleton );
            builder.AddService<OrderByQueryValidator>( Singleton );
            builder.AddService<SelectExpandQueryValidator>( Singleton );
            builder.AddService<SkipQueryValidator>( Singleton );
            builder.AddService<TopQueryValidator>( Singleton );
        }

        static void AddSerializationProviders( IContainerBuilder builder )
        {
            builder.AddService<ODataSerializerProvider, DefaultODataSerializerProvider>( Singleton );
            builder.AddService<ODataDeserializerProvider, DefaultODataDeserializerProvider>( Singleton );
        }

        static void AddDeserializerServices( IContainerBuilder builder )
        {
            builder.AddService<ODataResourceDeserializer>( Singleton );
            builder.AddService<ODataEnumDeserializer>( Singleton );
            builder.AddService<ODataPrimitiveDeserializer>( Singleton );
            builder.AddService<ODataResourceSetDeserializer>( Singleton );
            builder.AddService<ODataCollectionDeserializer>( Singleton );
            builder.AddService<ODataEntityReferenceLinkDeserializer>( Singleton );
            builder.AddService<ODataActionPayloadDeserializer>( Singleton );
        }

        static void AddSerializerServices( IContainerBuilder builder )
        {
            builder.AddService<ODataEnumSerializer>( Singleton );
            builder.AddService<ODataPrimitiveSerializer>( Singleton );
            builder.AddService<ODataDeltaFeedSerializer>( Singleton );
            builder.AddService<ODataResourceSetSerializer>( Singleton );
            builder.AddService<ODataCollectionSerializer>( Singleton );
            builder.AddService<ODataResourceSerializer>( Singleton );
            builder.AddService<ODataServiceDocumentSerializer>( Singleton );
            builder.AddService<ODataEntityReferenceLinkSerializer>( Singleton );
            builder.AddService<ODataEntityReferenceLinksSerializer>( Singleton );
            builder.AddService<ODataErrorSerializer>( Singleton );
            builder.AddService<ODataMetadataSerializer>( Singleton );
            builder.AddService<ODataRawValueSerializer>( Singleton );
        }

        static void AddBinders( IContainerBuilder builder )
        {
            builder.AddService<ODataQuerySettings>( Scoped );
            builder.AddService<FilterBinder>( Transient );
        }

        static void AddHttpRequestScope( IContainerBuilder builder )
        {
            builder.AddService<HttpRequestScope>( Scoped );
            builder.AddService( Scoped, sp => sp.GetRequiredService<HttpRequestScope>().HttpRequest );
        }
    }
}