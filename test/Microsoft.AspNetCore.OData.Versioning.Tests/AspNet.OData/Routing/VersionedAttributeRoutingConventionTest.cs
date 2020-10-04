﻿namespace Microsoft.AspNet.OData.Routing
{
    using Builder;
    using FluentAssertions;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Interfaces;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Simulators;
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
            var routeContext = NewRouteContext( "http://localhost/NeutralTests/1", typeof( VersionNeutralController ) );
            var serviceProvider = routeContext.HttpContext.RequestServices;
            var convention = new VersionedAttributeRoutingConvention( "odata", serviceProvider );

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
            var apiVersion = majorVersion + ".0";
            var routeContext = NewRouteContext( $"http://localhost/Tests(1)?api-version={apiVersion}", typeof( TestsController ), apiVersion );
            var serviceProvider = routeContext.HttpContext.RequestServices;
            var convention = new VersionedAttributeRoutingConvention( "odata", serviceProvider );

            // act
            var actionName = convention.SelectAction( routeContext )?.SingleOrDefault()?.ActionName;

            // assert
            actionName.Should().Be( expected );
        }

        static IActionDescriptorCollectionProvider NewActionDescriptorProvider( MethodInfo method )
        {
            var controllerType = method.DeclaringType.GetTypeInfo();
            var provider = new Mock<IActionDescriptorCollectionProvider>();
            var attribute = method.DeclaringType.GetCustomAttributes<ApiVersionAttribute>().FirstOrDefault();
            var model = attribute == null ? ApiVersionModel.Neutral : new ApiVersionModel( attribute.Versions.First() );
            var items = new ActionDescriptor[]
            {
                new ControllerActionDescriptor()
                {
                    ActionName = "Get",
                    ControllerName = "Tests",
                    ControllerTypeInfo = controllerType,
                    DisplayName = $"{controllerType.FullName}.{method.Name} ({controllerType.Assembly.GetName().Name})",
                    MethodInfo = method,
                    Properties = { [typeof( ApiVersionModel )] = model }
                }
            };

            provider.SetupGet( p => p.ActionDescriptors ).Returns( new ActionDescriptorCollection( items, 0 ) );

            return provider.Object;
        }

        static RouteContext NewRouteContext( string requestUri, Type controllerType, string apiVersion = default )
        {
            var url = new Uri( requestUri );
            var store = new Dictionary<string, StringValues>( capacity: 1 );
            var features = new Mock<IFeatureCollection>();
            var odataFeature = Mock.Of<IODataFeature>();
            var entitySet = Test.Model.EntityContainer.FindEntitySet( "Tests" );
            var httpRequest = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();
            var services = new ServiceCollection();

            if ( !string.IsNullOrEmpty( apiVersion ) )
            {
                store["api-version"] = new StringValues( apiVersion );
            }

            services.AddLogging();
            services.Add( Singleton( new DiagnosticListener( "test" ) ) );
            services.AddMvcCore( options => options.EnableEndpointRouting = false );
            services.AddApiVersioning();
            services.AddOData().EnableApiVersioning();
            services.Replace( Singleton( NewActionDescriptorProvider( controllerType.GetRuntimeMethod( "Get", new[] { typeof( int ) } ) ) ) );

            var serviceProvider = services.BuildServiceProvider();
            var app = new ApplicationBuilder( serviceProvider );
            var modelBuilder = serviceProvider.GetRequiredService<VersionedODataModelBuilder>();

            modelBuilder.ModelConfigurations.Add( new TestModelConfiguration() );
            app.UseMvc( rb => rb.MapVersionedODataRoute( "odata", null, modelBuilder ) );

            var rootContainer = serviceProvider.GetRequiredService<IPerRouteContainer>().GetODataRootContainer( "odata" );

            odataFeature.Path = new DefaultODataPathHandler().Parse(
                url.GetLeftPart( UriPartial.Authority ),
                url.GetComponents( Path, Unescaped ),
                rootContainer );
            odataFeature.RoutingConventionsStore = new Dictionary<string, object>();
            odataFeature.RequestContainer = rootContainer;
            features.SetupGet( f => f[typeof( IODataFeature )] ).Returns( odataFeature );
            features.Setup( f => f.Get<IODataFeature>() ).Returns( odataFeature );
            features.Setup( f => f.Get<IApiVersioningFeature>() ).Returns( () => new ApiVersioningFeature( httpContext.Object ) );
            httpContext.SetupGet( c => c.Features ).Returns( features.Object );
            httpContext.SetupProperty( c => c.RequestServices, serviceProvider );
            httpContext.SetupGet( c => c.Request ).Returns( () => httpRequest.Object );
            httpRequest.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
            httpRequest.SetupProperty( r => r.Query, new QueryCollection( store ) );
            httpRequest.SetupProperty( r => r.Method, "GET" );
            httpRequest.SetupProperty( r => r.Protocol, url.Scheme );
            httpRequest.SetupProperty( r => r.Host, new HostString( url.Host ) );
            httpRequest.SetupProperty( r => r.Path, new PathString( '/' + url.GetComponents( Path, Unescaped ) ) );
            httpRequest.SetupProperty( r => r.QueryString, new QueryString( url.Query ) );

            return new RouteContext( httpContext.Object );
        }
    }
};