namespace Microsoft.Web.Http.Description
{
    using Microsoft.Web.Http.Versioning.Conventions;
    using Models;
    using Simulators;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Tracing;
    using static System.Web.Http.RouteParameter;

    public class TestConfigurations : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { NewConventionRouteConfiguration() };
            yield return new object[] { NewDirectRouteConfiguration() };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        static HttpConfiguration NewConventionRouteConfiguration()
        {
            var configuration = new HttpConfiguration();
            var controllerTypeResolver = new ControllerTypeCollection(
                typeof( ValuesController ),
                typeof( Values2Controller ),
                typeof( Values3Controller ) );

            configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver );
            configuration.Services.Replace( typeof( ITraceWriter ), new TraceWriter() );
            configuration.Routes.MapHttpRoute( "Default", "{controller}/{id}", new { id = Optional } );
            configuration.AddApiVersioning(
                options =>
                {
                    options.Conventions.Controller<ValuesController>()
                                       .HasApiVersion( 1, 0 );
                    options.Conventions.Controller<Values2Controller>()
                                       .HasApiVersion( 2, 0 )
                                       .HasDeprecatedApiVersion( 3, 0, "beta" )
                                       .HasApiVersion( 3, 0 )
                                       .Action( c => c.GetV3() ).MapToApiVersion( 3, 0 )
                                       .Action( c => c.Post( default( ClassWithId ) ) ).MapToApiVersion( 3, 0 );
                    options.Conventions.Controller<Values3Controller>()
                                       .HasApiVersion( 4, 0 )
                                       .AdvertisesApiVersion( 5, 0 );
                } );

            return configuration;
        }

        static HttpConfiguration NewDirectRouteConfiguration()
        {
            var configuration = new HttpConfiguration();
            var controllerTypeResolver = new ControllerTypeCollection(
                typeof( AttributeValues1Controller ),
                typeof( AttributeValues2Controller ),
                typeof( AttributeValues3Controller ) );

            configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver );
            configuration.Services.Replace( typeof( ITraceWriter ), new TraceWriter() );
            configuration.MapHttpAttributeRoutes();
            configuration.AddApiVersioning();

            return configuration;
        }

        sealed class TraceWriter : ITraceWriter
        {
            public void Trace( HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction ) { }
        }
    }
}