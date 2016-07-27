namespace Microsoft.Web.Http.Dispatcher
{
    using Controllers;
    using FluentAssertions;
    using Moq;
    using Routing;
    using Simulators;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Routing;
    using Versioning;
    using Xunit;
    using static System.Net.Http.HttpMethod;
    using static System.Net.HttpStatusCode;
    using static System.Web.Http.IncludeErrorDetailPolicy;
    using static System.Web.Http.RouteParameter;

    public partial class ApiVersionControllerSelectorTest
    {
        private HttpConfiguration AttributeRoutingEnabledConfiguration
        {
            get
            {
                var configuration = new HttpConfiguration();
                configuration.MapHttpAttributeRoutes( new DefaultInlineConstraintResolver() { ConstraintMap = { ["apiVersion"] = typeof( ApiVersionRouteConstraint ) } } );
                return configuration;
            }
        }

        public static IEnumerable<object[]> ControllerNameData
        {
            get
            {
                yield return new object[] { new HttpRequestMessage(), null };

                var request = new HttpRequestMessage();
                var routeData = new HttpRouteData( new HttpRoute() );

                routeData.Values.Add( "controller", "Test" );
                request.SetRouteData( routeData );

                yield return new object[] { request, "Test" };
            }
        }

        [Theory]
        [MemberData( nameof( ControllerNameData ) )]
        public void get_controller_name_should_return_expected_value( HttpRequestMessage request, string expected )
        {
            // arrange
            var configuration = new HttpConfiguration();
            var options = new ApiVersioningOptions();
            var selector = new ApiVersionControllerSelector( configuration, options );

            // act
            var controllerName = selector.GetControllerName( request );

            // assert
            controllerName.Should().Be( expected );
        }

        [Fact]
        public void get_controller_mapping_should_return_expected_result()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var options = new ApiVersioningOptions();
            var selector = new ApiVersionControllerSelector( configuration, options );

            // act
            var mapping = selector.GetControllerMapping();

            // assert
            mapping.Values.Cast<HttpControllerDescriptorGroup>().Should().NotBeEmpty();
        }

        [Theory]
        [InlineData( "1.0", typeof( AttributeRoutedTestController ) )]
        [InlineData( "2.0", typeof( AttributeRoutedTest2Controller ) )]
        [InlineData( "3.0", typeof( AttributeRoutedTest2Controller ) )]
        public void select_controller_should_return_correct_versionedX2C_attributeX2Dbased_controller( string version, Type controllerType )
        {
            // arrange
            var supportedVersions = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ), new ApiVersion( 4, 0 ) };
            var configuration = AttributeRoutingEnabledConfiguration;
            var request = new HttpRequestMessage( Get, "http://localhost/api/test?api-version=" + version );

            configuration.AddApiVersioning();
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();

            // act
            var controller = selector.SelectController( request );

            // assert
            controller.ControllerType.Should().Be( controllerType );
            controller.GetSupportedApiVersions().Should().BeEquivalentTo( supportedVersions );
        }

        [Theory]
        [InlineData( "1.0", typeof( TestController ) )]
        [InlineData( "2.0", typeof( TestVersion2Controller ) )]
        [InlineData( "3.0", typeof( TestVersion2Controller ) )]
        public void select_controller_should_return_correct_versionedX2C_conventionX2Dbased_controller( string version, Type controllerType )
        {
            // arrange
            var supportedVersions = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) };
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( Get, "http://localhost/api/test?api-version=" + version );

            configuration.AddApiVersioning();
            configuration.Routes.MapHttpRoute( "Default", "api/{controller}/{id}", new { id = Optional } );
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();

            // act
            var controller = selector.SelectController( request );

            // assert
            controller.ControllerType.Should().Be( controllerType );
            controller.GetSupportedApiVersions().Should().BeEquivalentTo( supportedVersions );
        }

        [Theory]
        [InlineData( "http://localhost/api/neutral" )]
        [InlineData( "http://localhost/api/neutral?api-version=2.0" )]
        public void select_controller_should_return_correct_versionX2DneutralX2C_attributeX2Dbased_controller( string requestUri )
        {
            // arrange
            var controllerType = typeof( TestVersionNeutralController );
            var configuration = AttributeRoutingEnabledConfiguration;
            var request = new HttpRequestMessage( Get, requestUri );

            configuration.AddApiVersioning();
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();

            // act
            var controller = selector.SelectController( request );

            // assert
            controller.ControllerType.Should().Be( controllerType );
        }

        [Theory]
        [InlineData( "http://localhost/api/neutral" )]
        [InlineData( "http://localhost/api/neutral?api-version=2.0" )]
        public void select_controller_should_return_correct_versionX2DneutralX2C_conventionX2Dbased_controller( string requestUri )
        {
            // arrange
            var controllerType = typeof( NeutralController );
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( Get, requestUri );

            configuration.AddApiVersioning();
            configuration.Routes.MapHttpRoute( "Default", "api/{controller}/{id}", new { id = Optional } );
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();

            // act
            var controller = selector.SelectController( request );

            // assert
            controller.ControllerType.Should().Be( controllerType );
        }

        [Fact]
        public async Task select_controller_should_return_400_for_unmatchedX2C_attributeX2Dbased_controller_version()
        {
            // arrange
            var message = "The HTTP resource that matches the request URI 'http://localhost/api/test?api-version=42.0' does not support the API version '42.0'.";
            var messageDetail = "No route providing a controller name with API version '42.0' was found to match request URI 'http://localhost/api/test?api-version=42.0'.";
            var configuration = AttributeRoutingEnabledConfiguration;
            var request = new HttpRequestMessage( Get, "http://localhost/api/test?api-version=42.0" );

            configuration.IncludeErrorDetailPolicy = Always;
            configuration.AddApiVersioning();
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();
            Action selectController = () => selector.SelectController( request );

            // act
            var response = selectController.ShouldThrow<HttpResponseException>().Subject.Single().Response;
            var error = await response.Content.ReadAsAsync<HttpError>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            error.Message.Should().Be( message );
            error.MessageDetail.Should().Be( messageDetail );
        }

        [Fact]
        public async Task select_controller_should_return_400_for_attributeX2Dbased_controller_with_bad_version()
        {
            // arrange
            var message = "The HTTP resource that matches the request URI 'http://localhost/api/test?api-version=2016-06-32' does not support the API version '2016-06-32'.";
            var messageDetail = "No route providing a controller name with API version '2016-06-32' was found to match request URI 'http://localhost/api/test?api-version=2016-06-32'.";
            var configuration = AttributeRoutingEnabledConfiguration;
            var request = new HttpRequestMessage( Get, "http://localhost/api/test?api-version=2016-06-32" );

            configuration.IncludeErrorDetailPolicy = Always;
            configuration.AddApiVersioning();
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();
            Action selectController = () => selector.SelectController( request );

            // act
            var response = selectController.ShouldThrow<HttpResponseException>().Subject.Single().Response;
            var error = await response.Content.ReadAsAsync<HttpError>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            error.Message.Should().Be( message );
            error.MessageDetail.Should().Be( messageDetail );
        }

        [Fact]
        public async Task select_controller_should_return_400_for_unmatchedX2C_conventionX2Dbased_controller_version()
        {
            // arrange
            var message = "The HTTP resource that matches the request URI 'http://localhost/api/test?api-version=4.0' does not support the API version '4.0'.";
            var messageDetail = "No route providing a controller name with API version '4.0' was found to match request URI 'http://localhost/api/test?api-version=4.0'.";
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( Get, "http://localhost/api/test?api-version=4.0" );

            configuration.IncludeErrorDetailPolicy = Always;
            configuration.AddApiVersioning();
            configuration.Routes.MapHttpRoute( "Default", "api/{controller}/{id}", new { id = Optional } );
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();
            Action selectController = () => selector.SelectController( request );

            // act
            var response = selectController.ShouldThrow<HttpResponseException>().Subject.Single().Response;
            var error = await response.Content.ReadAsAsync<HttpError>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            error.Message.Should().Be( message );
            error.MessageDetail.Should().Be( messageDetail );
        }

        [Fact]
        public async Task select_controller_should_return_400_for_conventionX2Dbased_controller_with_bad_version()
        {
            // arrange
            var message = "The HTTP resource that matches the request URI 'http://localhost/api/test?api-version=2016-06-32' does not support the API version '2016-06-32'.";
            var messageDetail = "No route providing a controller name with API version '2016-06-32' was found to match request URI 'http://localhost/api/test?api-version=2016-06-32'.";
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( Get, "http://localhost/api/test?api-version=2016-06-32" );

            configuration.IncludeErrorDetailPolicy = Always;
            configuration.AddApiVersioning();
            configuration.Routes.MapHttpRoute( "Default", "api/{controller}/{id}", new { id = Optional } );
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();
            Action selectController = () => selector.SelectController( request );

            // act
            var response = selectController.ShouldThrow<HttpResponseException>().Subject.Single().Response;
            var error = await response.Content.ReadAsAsync<HttpError>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            error.Message.Should().Be( message );
            error.MessageDetail.Should().Be( messageDetail );
        }

        [Theory]
        [InlineData( "http://localhost/api/random" )]
        [InlineData( "http://localhost/api/random?api-version=10.0" )]
        public async Task select_controller_should_return_404_for_unmatched_controller( string requestUri )
        {
            // arrange
            var message = "No HTTP resource was found that matches the request URI '" + requestUri + "'.";
            var messageDetail = "No type was found that matches the controller named 'random'.";
            var configuration = AttributeRoutingEnabledConfiguration;
            var request = new HttpRequestMessage( Get, requestUri );

            configuration.IncludeErrorDetailPolicy = Always;
            configuration.AddApiVersioning();
            configuration.Routes.MapHttpRoute( "Default", "api/{controller}/{id}", new { id = Optional } );
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();
            Action selectController = () => selector.SelectController( request );

            // act
            var response = selectController.ShouldThrow<HttpResponseException>().Subject.Single().Response;
            var error = await response.Content.ReadAsAsync<HttpError>();

            // assert
            response.StatusCode.Should().Be( NotFound );
            error.Message.Should().Be( message );
            error.MessageDetail.Should().Be( messageDetail );
        }

        [Fact]
        public void select_controller_should_assume_1X2E0_for_attributeX2Dbased_controller_when_allowed()
        {
            // arrange
            var controllerType = typeof( AttributeRoutedTestController );
            var configuration = AttributeRoutingEnabledConfiguration;
            var request = new HttpRequestMessage( Get, "http://localhost/api/test" );

            configuration.AddApiVersioning( o => o.AssumeDefaultVersionWhenUnspecified = true );
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();

            // act
            var controller = selector.SelectController( request );

            // assert
            controller.ControllerType.Should().Be( controllerType );
        }

        [Fact]
        public void select_controller_should_assume_configured_default_api_version_for_attributeX2Dbased_controller()
        {
            // arrange
            var controllerType = typeof( AttributeRoutedTestController );
            var configuration = AttributeRoutingEnabledConfiguration;
            var request = new HttpRequestMessage( Get, "http://localhost/api/test?api-version=42.0" );

            configuration.AddApiVersioning( o => o.DefaultApiVersion = new ApiVersion( 42, 0 ) );
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();

            // act
            var controller = selector.SelectController( request );

            // assert
            controller.ControllerType.Should().Be( controllerType );
        }

        [Fact]
        public void select_controller_should_assume_1X2E0_for_conventionX2Dbased_controller_when_allowed()
        {
            // arrange
            var controllerType = typeof( TestController );
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( Get, "http://localhost/api/test" );

            configuration.AddApiVersioning( o => o.AssumeDefaultVersionWhenUnspecified = true );
            configuration.Routes.MapHttpRoute( "Default", "api/{controller}/{id}", new { id = Optional } );
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();

            // act
            var controller = selector.SelectController( request );

            // assert
            controller.ControllerType.Should().Be( controllerType );
        }

        [Fact]
        public void select_controller_should_assume_configured_default_api_version_for_conventionX2Dbased_controller()
        {
            // arrange
            var controllerType = typeof( TestController );
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( Get, "http://localhost/api/test?api-version=42.0" );

            configuration.AddApiVersioning( o => o.DefaultApiVersion = new ApiVersion( 42, 0 ) );
            configuration.Routes.MapHttpRoute( "Default", "api/{controller}/{id}", new { id = Optional } );
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();

            // act
            var controller = selector.SelectController( request );

            // assert
            controller.ControllerType.Should().Be( controllerType );
        }

        [Fact]
        public void select_controller_should_use_api_version_selector_for_conventionX2Dbased_controller_when_allowed()
        {
            // arrange
            var controllerType = typeof( OrdersController );
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( Get, "http://localhost/api/orders" );

            configuration.AddApiVersioning( o =>
                {
                    o.AssumeDefaultVersionWhenUnspecified = true;
                    o.ApiVersionSelector = new ConstantApiVersionSelector( new ApiVersion( new DateTime( 2015, 11, 15 ) ) );
                } );
            configuration.Routes.MapHttpRoute( "Default", "api/{controller}/{id}", new { id = Optional } );
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();

            // act
            var controller = selector.SelectController( request );

            // assert
            controller.ControllerType.Should().Be( controllerType );
        }

        [Fact]
        public void select_controller_should_use_api_version_selector_for_attributeX2Dbased_controller_when_allowed()
        {
            // arrange
            var controllerType = typeof( OrdersController );
            var configuration = AttributeRoutingEnabledConfiguration;
            var request = new HttpRequestMessage( Get, "http://localhost/orders" );

            configuration.AddApiVersioning( o =>
                {
                    o.AssumeDefaultVersionWhenUnspecified = true;
                    o.ApiVersionSelector = new LowestImplementedApiVersionSelector();
                } );
            configuration.Routes.MapHttpRoute( "Default", "{controller}/{id}", new { id = Optional } );
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var controllerSelector = configuration.Services.GetHttpControllerSelector();
            var actionSelector = configuration.Services.GetActionSelector();
            var controllerDescriptor = controllerSelector.SelectController( request );
            var controllerContext = new HttpControllerContext( configuration, routeData, request )
            {
                ControllerDescriptor = controllerDescriptor,
                RequestContext = new HttpRequestContext()
                {
                    Configuration = configuration,
                    RouteData = routeData
                }
            };

            // act
            var action = actionSelector.SelectAction( controllerContext );

            // assert
            action.ActionName.Should().Be( nameof( OrdersController.Get_2015_11_15 ) );
        }

        [Fact]
        public void select_controller_should_throw_exception_for_ambiguously_versionedX2C_attributeX2Dbased_controller()
        {
            // arrange
            var request = new HttpRequestMessage( Get, "http://localhost/api/test?api-version=1.0" );
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            var controllerTypes = new Collection<Type>()
            {
                typeof( Ambiguous1Controller ),
                typeof( Ambiguous2Controller ),
            };
            var message =
@"Multiple controller types were found that match the URL. This can happen if attribute routes on multiple controllers match the requested URL.

The request has found the following matching controller types: 
Microsoft.Web.Http.Dispatcher.ApiVersionControllerSelectorTest+Ambiguous1Controller
Microsoft.Web.Http.Dispatcher.ApiVersionControllerSelectorTest+Ambiguous2Controller";

            controllerTypeResolver.Setup( r => r.GetControllerTypes( It.IsAny<IAssembliesResolver>() ) ).Returns( controllerTypes );

            var configuration = new HttpConfiguration();

            configuration.IncludeErrorDetailPolicy = Always;
            configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver.Object );
            configuration.AddApiVersioning();
            configuration.MapHttpAttributeRoutes();
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();
            Action selectController = () => selector.SelectController( request );

            // act

            // assert
            selectController.ShouldThrow<InvalidOperationException>().WithMessage( message );
        }

        [Fact]
        public void select_controller_should_throw_exception_for_ambiguously_versionedX2C_conventionX2Dbased_controller()
        {
            // arrange
            var request = new HttpRequestMessage( Get, "http://localhost/api/ambiguous?api-version=1.0" );
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            var controllerTypes = new Collection<Type>()
            {
                typeof( AmbiguousController ),
                typeof( Ambiguous3Controller ),
            };
            var message =
@"Multiple types were found that match the controller named 'ambiguous'. This can happen if the route that services this request ('api/{controller}/{id}') found multiple controllers defined with the same name but differing namespaces, which is not supported.

The request for 'ambiguous' has found the following matching controllers:
Microsoft.Web.Http.Dispatcher.ApiVersionControllerSelectorTest+AmbiguousController
Microsoft.Web.Http.Dispatcher.ApiVersionControllerSelectorTest+Ambiguous3Controller";

            controllerTypeResolver.Setup( r => r.GetControllerTypes( It.IsAny<IAssembliesResolver>() ) ).Returns( controllerTypes );

            var configuration = new HttpConfiguration();

            configuration.IncludeErrorDetailPolicy = Always;
            configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver.Object );
            configuration.AddApiVersioning();
            configuration.Routes.MapHttpRoute( "Default", "api/{controller}/{id}", new { id = Optional } );
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();
            Action selectController = () => selector.SelectController( request );

            // act

            // assert
            selectController.ShouldThrow<InvalidOperationException>().WithMessage( message );
        }

        [Fact]
        public void select_controller_should_throw_exception_for_ambiguous_neutral_and_versionedX2C_attributeX2Dbased_controller()
        {
            // arrange
            var request = new HttpRequestMessage( Get, "http://localhost/api/test" );
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            var controllerTypes = new Collection<Type>()
            {
                typeof( Ambiguous1Controller ),
                typeof( AmbiguousNeutralController ),
            };
            var message =
@"Multiple controller types were found that match the URL. This can happen if attribute routes on multiple controllers match the requested URL.

The request has found the following matching controller types: 
Microsoft.Web.Http.Dispatcher.ApiVersionControllerSelectorTest+AmbiguousNeutralController
Microsoft.Web.Http.Dispatcher.ApiVersionControllerSelectorTest+Ambiguous1Controller";

            controllerTypeResolver.Setup( r => r.GetControllerTypes( It.IsAny<IAssembliesResolver>() ) ).Returns( controllerTypes );

            var configuration = new HttpConfiguration();

            configuration.IncludeErrorDetailPolicy = Always;
            configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver.Object );
            configuration.AddApiVersioning( o => o.AssumeDefaultVersionWhenUnspecified = true );
            configuration.MapHttpAttributeRoutes();
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();
            Action selectController = () => selector.SelectController( request );

            // act

            // assert
            selectController.ShouldThrow<InvalidOperationException>().WithMessage( message );
        }

        [Fact]
        public void select_controller_should_throw_exception_for_ambiguous_neutral_and_versionedX2C_conventionX2Dbased_controller()
        {
            // arrange
            var request = new HttpRequestMessage( Get, "http://localhost/api/ambiguous" );
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            var controllerTypes = new Collection<Type>()
            {
                typeof( AmbiguousController ),
                typeof( AmbiguousNeutralController ),
            };
            var message =
@"Multiple types were found that match the controller named 'ambiguous'. This can happen if the route that services this request ('api/{controller}/{id}') found multiple controllers defined with the same name but differing namespaces, which is not supported.

The request for 'ambiguous' has found the following matching controllers:
Microsoft.Web.Http.Dispatcher.ApiVersionControllerSelectorTest+AmbiguousController
Microsoft.Web.Http.Dispatcher.ApiVersionControllerSelectorTest+AmbiguousNeutralController";

            controllerTypeResolver.Setup( r => r.GetControllerTypes( It.IsAny<IAssembliesResolver>() ) ).Returns( controllerTypes );

            var configuration = new HttpConfiguration();

            configuration.IncludeErrorDetailPolicy = Always;
            configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver.Object );
            configuration.AddApiVersioning( o => o.AssumeDefaultVersionWhenUnspecified = true );
            configuration.Routes.MapHttpRoute( "Default", "api/{controller}/{id}", new { id = Optional } );
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();
            Action selectController = () => selector.SelectController( request );

            // act

            // assert
            selectController.ShouldThrow<InvalidOperationException>().WithMessage( message );
        }

        [Fact]
        public void select_controller_should_assume_current_version_for_attributeX2Dbased_controller_when_allowed()
        {
            // arrange
            var currentVersion = new ApiVersion( 3, 0 );
            var controllerType = typeof( AttributeRoutedTest2Controller );
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            var controllerTypes = new Collection<Type>()
            {
                typeof( AttributeRoutedTestController ),
                typeof( AttributeRoutedTest2Controller ),
            };

            controllerTypeResolver.Setup( r => r.GetControllerTypes( It.IsAny<IAssembliesResolver>() ) ).Returns( controllerTypes );

            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( Get, "http://localhost/api/test" );

            configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver.Object );
            configuration.AddApiVersioning( o =>
                {
                    o.AssumeDefaultVersionWhenUnspecified = true;
                    o.ApiVersionSelector = new CurrentImplementationApiVersionSelector();
                } );
            configuration.MapHttpAttributeRoutes();
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();

            // act
            var controller = selector.SelectController( request );

            // assert
            controller.ControllerType.Should().Be( controllerType );
            request.GetRequestedApiVersion().Should().Be( currentVersion );
        }

        [Fact]
        public void select_controller_should_assume_current_version_for_conventionX2Dbased_controller_when_allowed()
        {
            // arrange
            var currentVersion = new ApiVersion( 3, 0 );
            var controllerType = typeof( TestVersion2Controller );
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            var controllerTypes = new Collection<Type>()
            {
                typeof( TestController ),
                typeof( TestVersion2Controller ),
            };

            controllerTypeResolver.Setup( r => r.GetControllerTypes( It.IsAny<IAssembliesResolver>() ) ).Returns( controllerTypes );

            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( Get, "http://localhost/api/test" );

            configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver.Object );
            configuration.AddApiVersioning( o =>
                {
                    o.AssumeDefaultVersionWhenUnspecified = true;
                    o.ApiVersionSelector = new CurrentImplementationApiVersionSelector();
                } );
            configuration.Routes.MapHttpRoute( "Default", "api/{controller}/{id}", new { id = Optional } );
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            var selector = configuration.Services.GetHttpControllerSelector();

            // act
            var controller = selector.SelectController( request );

            // assert
            controller.ControllerType.Should().Be( controllerType );
            request.GetRequestedApiVersion().Should().Be( currentVersion );
        }

        [Theory]
        [InlineData( "v1", typeof( ApiVersionedRouteController ), "Get", "1.0,2.0,3.0" )]
        [InlineData( "v1.0", typeof( ApiVersionedRouteController ), "Get", "1.0,2.0,3.0" )]
        [InlineData( "v2", typeof( ApiVersionedRouteController ), "Get", "1.0,2.0,3.0" )]
        [InlineData( "v3.0", typeof( ApiVersionedRouteController ), "Get", "1.0,2.0,3.0" )]
        [InlineData( "v4", typeof( ApiVersionedRoute2Controller ), "GetV4", "4.0,5.0" )]
        [InlineData( "v5", typeof( ApiVersionedRoute2Controller ), "Get", "4.0,5.0" )]
        public void select_controller_should_return_correct_controller_for_versioned_route_attribute( string versionSegment, Type controllerType, string actionName, string declaredVersionsValue )
        {
            // arrange
            var declared = declaredVersionsValue.Split( ',' ).Select( v => ApiVersion.Parse( v ) );
            var supported = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ), new ApiVersion( 5, 0 ) };
            var deprecated = new[] { new ApiVersion( 4, 0 ) };
            var implemented = supported.Union( deprecated ).OrderBy( v => v ).ToArray();
            var requestUri = "http://localhost/api/" + versionSegment + "/test";
            var configuration = AttributeRoutingEnabledConfiguration;
            var request = new HttpRequestMessage( Get, requestUri );

            configuration.AddApiVersioning();
            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );
            var requestContext = new HttpRequestContext
            {
                IsLocal = true,
                Configuration = configuration,
                RouteData = routeData,
                Url = new UrlHelper( request )
            };
            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );
            request.SetRequestContext( requestContext );

            var httpControllerSelector = configuration.Services.GetHttpControllerSelector();
            var actionSelector = configuration.Services.GetActionSelector();

            // act
            var controller = httpControllerSelector.SelectController( request );
            var context = new HttpControllerContext( requestContext, request, controller, controller.CreateController( request ) );
            var action = actionSelector.SelectAction( context );

            // assert
            controller.ControllerType.Should().Be( controllerType );
            action.ActionName.Should().Be( actionName );
            controller.GetApiVersionModel().ShouldBeEquivalentTo(
                new
                {
                    IsApiVersionNeutral = false,
                    DeclaredApiVersions = declared,
                    ImplementedApiVersions = implemented,
                    SupportedApiVersions = supported,
                    DeprecatedApiVersions = deprecated
                } );
        }
    }
}