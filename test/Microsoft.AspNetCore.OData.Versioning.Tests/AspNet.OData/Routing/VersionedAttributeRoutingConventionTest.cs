namespace Microsoft.AspNet.OData.Routing
{
    using Builder;
    using FluentAssertions;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Interfaces;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Builder.Internal;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Xunit;
    using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
    using static System.UriComponents;
    using static System.UriFormat;

    public class VersionedAttributeRoutingConventionTest
    {
        [Fact]
        public void select_action_should_return_true_for_versionX2Dneutral_controller()
        {
            // arrange
            var routeContext = NewRouteContext( typeof( VersionNeutralController ) );
            var serviceProvider = routeContext.HttpContext.RequestServices;
            var convention = NewRoutingConvention( serviceProvider, new ApiVersion( 1, 0 ) );

            // act
            var result = convention.SelectAction( routeContext );

            // assert
            result.Single().ActionName.Should().Be( "Get" );
        }

        [Theory]
        [InlineData( 1, "Get" )]
        [InlineData( 2, null )]
        public void select_action_should_return_expected_result_for_controller_version( int majorVersion, string expected )
        {
            // arrange
            var routeContext = NewRouteContext( typeof( VersionedController ) );
            var serviceProvider = routeContext.HttpContext.RequestServices;
            var convention = NewRoutingConvention( serviceProvider, new ApiVersion( majorVersion, 0 ) );

            // act
            var actionName = convention.SelectAction( routeContext )?.SingleOrDefault().ActionName;

            // assert
            actionName.Should().Be( expected );
        }

        static VersionedAttributeRoutingConvention NewRoutingConvention( IServiceProvider serviceProvider, ApiVersion apiVersion ) =>
            new VersionedAttributeRoutingConvention( "Tests", serviceProvider, new DefaultODataPathHandler(), apiVersion );

        static IActionDescriptorCollectionProvider NewActionDescriptorProvider( MethodInfo method )
        {
            var controllerType = method.DeclaringType.GetTypeInfo();
            var provider = new Mock<IActionDescriptorCollectionProvider>();
            var items = new ActionDescriptor[]
            {
                new ControllerActionDescriptor()
                {
                    ActionName = "Get",
                    ControllerName = "Tests",
                    ControllerTypeInfo = controllerType,
                    DisplayName = $"{controllerType.FullName}.{method.Name} ({controllerType.Assembly.GetName().Name})",
                    MethodInfo = method,
                }
            };

            provider.SetupGet( p => p.ActionDescriptors ).Returns( new ActionDescriptorCollection( items, 0 ) );

            return provider.Object;
        }

        static RouteContext NewRouteContext( Type controllerType )
        {
            var url = new Uri( "http://localhost/Tests(1)?api-version=1.0" );
            var apiVersionProvider = new Mock<IODataApiVersionProvider>();
            var supported = new[] { new ApiVersion( 1, 0 ) };
            var deprecated = Array.Empty<ApiVersion>();
            var features = new Mock<IFeatureCollection>();
            var odataFeature = Mock.Of<IODataFeature>();
            var entitySet = Test.Model.EntityContainer.FindEntitySet( "Tests" );
            var httpRequest = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();
            var services = new ServiceCollection();

            apiVersionProvider.SetupGet( p => p.SupportedApiVersions ).Returns( supported );
            apiVersionProvider.SetupGet( p => p.DeprecatedApiVersions ).Returns( deprecated );
            services.AddLogging();
            services.Add( Singleton<DiagnosticSource>( new DiagnosticListener( "test" ) ) );
            services.Add( Singleton<IOptions<MvcOptions>>( new OptionsWrapper<MvcOptions>( new MvcOptions() ) ) );
            services.AddMvcCore();
            services.AddApiVersioning();
            services.AddOData().EnableApiVersioning();
            services.Replace( Singleton( apiVersionProvider.Object ) );
            services.Replace( Singleton( NewActionDescriptorProvider( controllerType.GetRuntimeMethod( "Get", Type.EmptyTypes ) ) ) );

            var serviceProvider = services.BuildServiceProvider();
            var app = new ApplicationBuilder( serviceProvider );
            var modelBuilder = serviceProvider.GetRequiredService<VersionedODataModelBuilder>();

            app.UseMvc( rb => rb.MapVersionedODataRoute( "odata", null, Test.Model, supported[0] ) );
            odataFeature.Path = new ODataPath( new EntitySetSegment( entitySet ), new KeySegment( new[] { new KeyValuePair<string, object>( "Id", 1 ) }, entitySet.EntityType(), null ) );
            features.SetupGet( f => f[typeof( IODataFeature )] ).Returns( odataFeature );
            features.Setup( f => f.Get<IODataFeature>() ).Returns( odataFeature );
            httpContext.SetupGet( c => c.Features ).Returns( features.Object );
            httpContext.SetupProperty( c => c.RequestServices, serviceProvider );
            httpContext.SetupGet( c => c.Request ).Returns( () => httpRequest.Object );
            httpRequest.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
            httpRequest.SetupProperty( r => r.Method, "GET" );
            httpRequest.SetupProperty( r => r.Protocol, url.Scheme );
            httpRequest.SetupProperty( r => r.Host, new HostString( url.Host ) );
            httpRequest.SetupProperty( r => r.Path, new PathString( '/' + url.GetComponents( Path, Unescaped ) ) );
            httpRequest.SetupProperty( r => r.QueryString, new QueryString( url.Query ) );

            var modelProviders = serviceProvider.GetServices<IApplicationModelProvider>();
            var context = new ApplicationModelProviderContext( new[] { controllerType.GetTypeInfo() } );

            foreach ( var provider in modelProviders )
            {
                provider.OnProvidersExecuting( context );
            }

            foreach ( var provider in modelProviders.Reverse() )
            {
                provider.OnProvidersExecuted( context );
            }

            return new RouteContext( httpContext.Object );
        }

        [ApiVersionNeutral]
        [ODataRoutePrefix( "Tests" )]
        sealed class VersionNeutralController : ODataController
        {
            [ODataRoute]
            public IActionResult Get() => Ok();
        }

        [ApiVersion( "1.0" )]
        [ODataRoutePrefix( "Tests" )]
        sealed class VersionedController : ODataController
        {
            [ODataRoute]
            public IActionResult Get() => Ok();
        }
    }
};