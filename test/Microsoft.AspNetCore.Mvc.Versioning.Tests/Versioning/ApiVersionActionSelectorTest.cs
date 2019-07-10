namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
#if !NETCOREAPP
    using Microsoft.AspNetCore.Mvc.Internal;
#endif
    using Microsoft.AspNetCore.Mvc.Simulators;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using Xunit;
    using static ApiVersion;
    using static System.Environment;
    using static System.Net.Http.HttpMethod;
    using static System.Net.HttpStatusCode;

    public partial class ApiVersionActionSelectorTest
    {
        [Theory]
        [InlineData( "1.0", typeof( AttributeRoutedTestController ) )]
        [InlineData( "2.0", typeof( AttributeRoutedTest2Controller ) )]
        [InlineData( "3.0", typeof( AttributeRoutedTest2Controller ) )]
        public async Task select_best_candidate_should_return_correct_versionedX2C_attributeX2Dbased_controller( string version, Type controllerType )
        {
            // arrange
            using ( var server = new WebServer() )
            {
                await server.Client.GetAsync( $"api/attributed?api-version={version}" );

                // act
                var action = ( (TestApiVersionActionSelector) server.Services.GetRequiredService<IActionSelector>() ).SelectedCandidate;

                // assert
                action.GetProperty<ApiVersionModel>().SupportedApiVersions.Should().Contain( Parse( version ) );
                action.As<ControllerActionDescriptor>().ControllerTypeInfo.Should().Be( controllerType.GetTypeInfo() );
            }
        }

        [Theory]
        [InlineData( "1.0", typeof( TestController ) )]
        [InlineData( "2.0", typeof( TestVersion2Controller ) )]
        [InlineData( "3.0", typeof( TestVersion2Controller ) )]
        public async Task select_best_candidate_should_return_correct_versionedX2C_conventionX2Dbased_controller( string version, Type controllerType )
        {
            // arrange
            void ConfigureOptions( ApiVersioningOptions options ) => options.UseApiBehavior = false;

            using ( var server = new WebServer( ConfigureOptions, r => r.MapRoute( "default", "api/{controller}/{action=Get}/{id?}" ) ) )
            {
                var response = await server.Client.GetAsync( $"api/test?api-version={version}" );

                // act
                var action = ( (TestApiVersionActionSelector) server.Services.GetRequiredService<IActionSelector>() ).SelectedCandidate;

                // assert
                action.GetProperty<ApiVersionModel>().SupportedApiVersions.Should().Contain( Parse( version ) );
                action.As<ControllerActionDescriptor>().ControllerTypeInfo.Should().Be( controllerType.GetTypeInfo() );
            }
        }

        [Theory]
        [InlineData( "http://localhost/api/attributed-neutral" )]
        [InlineData( "http://localhost/api/attributed-neutral?api-version=2.0" )]
        public async Task select_best_candidate_should_return_correct_versionX2DneutralX2C_attributeX2Dbased_controller( string requestUri )
        {
            // arrange
            var controllerType = typeof( AttributeRoutedVersionNeutralController ).GetTypeInfo();

            using ( var server = new WebServer() )
            {
                await server.Client.GetAsync( requestUri );

                // act
                var action = ( (TestApiVersionActionSelector) server.Services.GetRequiredService<IActionSelector>() ).SelectedCandidate;

                // assert
                action.As<ControllerActionDescriptor>().ControllerTypeInfo.Should().Be( controllerType );
            }
        }

        [Theory]
        [InlineData( "http://localhost/api/neutral" )]
        [InlineData( "http://localhost/api/neutral?api-version=2.0" )]
        public async Task select_best_candidate_should_return_correct_versionX2DneutralX2C_conventionX2Dbased_controller( string requestUri )
        {
            // arrange
            var controllerType = typeof( NeutralController ).GetTypeInfo();

            using ( var server = new WebServer( setupRoutes: routes => routes.MapRoute( "default", "api/{controller}/{action=Get}/{id?}" ) ) )
            {
                await server.Client.GetAsync( requestUri );

                // act
                var action = ( (TestApiVersionActionSelector) server.Services.GetRequiredService<IActionSelector>() ).SelectedCandidate;

                // assert
                action.As<ControllerActionDescriptor>().ControllerTypeInfo.Should().Be( controllerType );
            }
        }

        [Fact]
        public async Task select_best_candidate_should_return_400_for_unmatchedX2C_attributeX2Dbased_controller_version()
        {
            // arrange
            using ( var server = new WebServer( options => options.ReportApiVersions = true ) )
            {
                // act
                var response = await server.Client.GetAsync( "http://localhost/api/attributed?api-version=42.0" );

                // assert
                response.StatusCode.Should().Be( BadRequest );
                response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0, 4.0" );
                response.Headers.GetValues( "api-deprecated-versions" ).Single().Should().Be( "3.0-Alpha" );
            }
        }

        [Fact]
        public async Task select_best_candidate_should_return_400_for_attributeX2Dbased_controller_with_bad_version()
        {
            // arrange
            using ( var server = new WebServer( options => options.ReportApiVersions = true ) )
            {
                // act
                var response = await server.Client.GetAsync( "http://localhost/api/attributed?api-version=2016-06-32" );

                // assert
                response.StatusCode.Should().Be( BadRequest );
                response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0, 4.0" );
                response.Headers.GetValues( "api-deprecated-versions" ).Single().Should().Be( "3.0-Alpha" );
            }
        }

        [Fact]
        public async Task select_best_candidate_should_return_400_for_unmatchedX2C_conventionX2Dbased_controller_version()
        {
            // arrange
            void ConfigureOptions( ApiVersioningOptions options )
            {
                options.ReportApiVersions = true;
                options.UseApiBehavior = false;
            }

            using ( var server = new WebServer( ConfigureOptions, routes => routes.MapRoute( "default", "api/{controller}/{action=Get}/{id?}" ) ) )
            {
                // act
                var response = await server.Client.GetAsync( "http://localhost/api/test?api-version=4.0" );

                // assert
                response.StatusCode.Should().Be( BadRequest );
                response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
                response.Headers.GetValues( "api-deprecated-versions" ).Single().Should().Be( "1.8, 1.9" );
            }
        }

        [Fact]
        public async Task select_best_candidate_should_return_400_for_conventionX2Dbased_controller_with_bad_version()
        {
            // arrange
            void ConfigureOptions( ApiVersioningOptions options )
            {
                options.ReportApiVersions = true;
                options.UseApiBehavior = false;
            }

            using ( var server = new WebServer( ConfigureOptions, routes => routes.MapRoute( "default", "api/{controller}/{action=Get}/{id?}" ) ) )
            {
                // act
                var response = await server.Client.GetAsync( "http://localhost/api/test?api-version=2016-06-32" );

                // assert
                response.StatusCode.Should().Be( BadRequest );
                response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
                response.Headers.GetValues( "api-deprecated-versions" ).Single().Should().Be( "1.8, 1.9" );
            }
        }

        [Theory]
        [InlineData( "http://localhost/api/random" )]
        [InlineData( "http://localhost/api/random?api-version=10.0" )]
        public async Task select_best_candidate_should_return_404_for_unmatched_controller( string requestUri )
        {
            // arrange
            using ( var server = new WebServer( setupRoutes: routes => routes.MapRoute( "default", "api/{controller}/{action=Get}/{id?}" ) ) )
            {
                // act
                var response = await server.Client.GetAsync( requestUri );

                // assert
                response.StatusCode.Should().Be( NotFound );
            }
        }

        [Fact]
        public async Task select_best_candidate_should_return_405_for_unmatched_action()
        {
            // arrange
            var request = new HttpRequestMessage( Post, "api/attributed?api-version=1.0" );

            using ( var server = new WebServer( options => options.ReportApiVersions = true ) )
            {
                // act
                var response = await server.Client.SendAsync( request );

                // assert
                response.StatusCode.Should().Be( MethodNotAllowed );
                response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0, 4.0" );
                response.Headers.GetValues( "api-deprecated-versions" ).Single().Should().Be( "3.0-Alpha" );
            }
        }

        [Fact]
        public async Task return_only_path_for_unmatched_action()
        {
            // arrange
            var request = new HttpRequestMessage( Post, "api/attributed?api-version=1.0" );

            using ( var server = new WebServer( options => options.ReportApiVersions = true ) )
            {
                // act
                var response = await server.Client.SendAsync( request );

                // assert
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain( "api/attributed" );
                content.Should().NotContain( "?api-version=1.0" );
            }
        }

        [Fact]
        public async Task select_best_candidate_should_assume_1X2E0_for_attributeX2Dbased_controller_when_allowed()
        {
            // arrange
            var controllerType = typeof( AttributeRoutedTestController ).GetTypeInfo();

            using ( var server = new WebServer( o => o.AssumeDefaultVersionWhenUnspecified = true ) )
            {
                await server.Client.GetAsync( "api/attributed" );

                // act
                var action = ( (TestApiVersionActionSelector) server.Services.GetRequiredService<IActionSelector>() ).SelectedCandidate;

                // assert
                action.As<ControllerActionDescriptor>().ControllerTypeInfo.Should().Be( controllerType );
            }
        }

        [Fact]
        public async Task select_best_candidate_should_assume_configured_default_api_version_for_attributeX2Dbased_controller()
        {
            // arrange
            var controllerType = typeof( AttributeRoutedTestController ).GetTypeInfo();

            using ( var server = new WebServer( options => options.DefaultApiVersion = new ApiVersion( 42, 0 ) ) )
            {
                await server.Client.GetAsync( "api/attributed?api-version=42.0" );

                // act
                var action = ( (TestApiVersionActionSelector) server.Services.GetRequiredService<IActionSelector>() ).SelectedCandidate;

                // assert
                action.As<ControllerActionDescriptor>().ControllerTypeInfo.Should().Be( controllerType );
            }
        }

        [Fact]
        public async Task select_best_candidate_should_assume_1X2E0_for_conventionX2Dbased_controller_when_allowed()
        {
            // arrange
            var controllerType = typeof( TestController ).GetTypeInfo();
            Action<ApiVersioningOptions> versioningSetup = options =>
            {
                options.UseApiBehavior = false;
                options.AssumeDefaultVersionWhenUnspecified = true;
            };
            Action<IRouteBuilder> routesSetup = r => r.MapRoute( "default", "api/{controller}/{action=Get}/{id?}" );

            using ( var server = new WebServer( versioningSetup, routesSetup ) )
            {
                await server.Client.GetAsync( "api/test" );

                // act
                var action = ( (TestApiVersionActionSelector) server.Services.GetRequiredService<IActionSelector>() ).SelectedCandidate;

                // assert
                action.As<ControllerActionDescriptor>().ControllerTypeInfo.Should().Be( controllerType );
            }
        }

        [Fact]
        public async Task select_best_candidate_should_assume_configured_default_api_version_for_conventionX2Dbased_controller()
        {
            // arrange
            var controllerType = typeof( TestController ).GetTypeInfo();

            void ConfigureOptions( ApiVersioningOptions options )
            {
                options.UseApiBehavior = false;
                options.DefaultApiVersion = new ApiVersion( 42, 0 );
            }

            using ( var server = new WebServer( ConfigureOptions, r => r.MapRoute( "default", "api/{controller}/{action=Get}/{id?}" ) ) )
            {
                await server.Client.GetAsync( "api/test?api-version=42.0" );

                // act
                var action = ( (TestApiVersionActionSelector) server.Services.GetRequiredService<IActionSelector>() ).SelectedCandidate;

                // assert
                action.As<ControllerActionDescriptor>().ControllerTypeInfo.Should().Be( controllerType );
            }
        }

        [Fact]
        public async Task select_best_candidate_should_use_api_version_selector_for_conventionX2Dbased_controller_when_allowed()
        {
            // arrange
            var controllerType = typeof( OrdersController ).GetTypeInfo();

            void ConfigureOptions( ApiVersioningOptions options )
            {
                options.UseApiBehavior = false;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionSelector = new LowestImplementedApiVersionSelector( options );

            }

            using ( var server = new WebServer( ConfigureOptions, r => r.MapRoute( "default", "api/{controller}/{action=Get}/{id?}" ) ) )
            {
                await server.Client.GetAsync( "api/orders" );

                // act
                var action = ( (TestApiVersionActionSelector) server.Services.GetRequiredService<IActionSelector>() ).SelectedCandidate;

                // assert
                action.As<ControllerActionDescriptor>().Should().BeEquivalentTo(
                    new
                    {
                        ControllerTypeInfo = controllerType,
                        ActionName = nameof( OrdersController.Get )
                    },
                    options => options.ExcludingMissingMembers() );
            }
        }

        [Fact]
        public async Task select_best_candidate_should_throw_exception_for_ambiguously_versionedX2C_attributeX2Dbased_controller()
        {
            // arrange
            var message = $"Multiple actions matched. The following actions matched route data and had all constraints satisfied:{NewLine}{NewLine}" +
                          $"Microsoft.AspNetCore.Mvc.Simulators.AttributeRoutedAmbiguous2Controller.Get() (Microsoft.AspNetCore.Mvc.Versioning.Tests){NewLine}" +
                          $"Microsoft.AspNetCore.Mvc.Simulators.AttributeRoutedAmbiguous3Controller.Get() (Microsoft.AspNetCore.Mvc.Versioning.Tests)";

            using ( var server = new WebServer( o => o.AssumeDefaultVersionWhenUnspecified = true ) )
            {
                Func<Task> test = () => server.Client.GetAsync( "api/attributed/ambiguous" );

                // act

                // assert
                await test.Should().ThrowAsync<AmbiguousActionException>().WithMessage( message );
            }
        }

        [Fact]
        public async Task select_best_candidate_should_throw_exception_for_ambiguously_versionedX2C_conventionX2Dbased_controller()
        {
            // arrange
            void ConfigureOptions( ApiVersioningOptions options )
            {
                options.UseApiBehavior = false;
                options.AssumeDefaultVersionWhenUnspecified = true;
            }
            var message = $"Multiple actions matched. The following actions matched route data and had all constraints satisfied:{NewLine}{NewLine}" +
                          $"Microsoft.AspNetCore.Mvc.Simulators.AmbiguousToo2Controller.Get() (Microsoft.AspNetCore.Mvc.Versioning.Tests){NewLine}" +
                          $"Microsoft.AspNetCore.Mvc.Simulators.AmbiguousTooController.Get() (Microsoft.AspNetCore.Mvc.Versioning.Tests)";

            using ( var server = new WebServer( ConfigureOptions, r => r.MapRoute( "default", "api/{controller}/{action=Get}/{id?}" ) ) )
            {
                Func<Task> test = () => server.Client.GetAsync( "api/ambiguoustoo" );

                // act

                // assert
                await test.Should().ThrowAsync<AmbiguousActionException>().WithMessage( message );
            }
        }

        [Fact]
        public async Task select_best_candidate_should_throw_exception_for_ambiguous_neutral_and_versionedX2C_attributeX2Dbased_controller()
        {
            // arrange
            var message = $"Multiple actions matched. The following actions matched route data and had all constraints satisfied:{NewLine}{NewLine}" +
                          $"Microsoft.AspNetCore.Mvc.Simulators.AttributeRoutedAmbiguousNeutralController.Get() (Microsoft.AspNetCore.Mvc.Versioning.Tests){NewLine}" +
                          $"Microsoft.AspNetCore.Mvc.Simulators.AttributeRoutedAmbiguousController.Get() (Microsoft.AspNetCore.Mvc.Versioning.Tests)";

            using ( var server = new WebServer( options => options.AssumeDefaultVersionWhenUnspecified = true ) )
            {
                Func<Task> test = () => server.Client.GetAsync( "api/attributed-ambiguous" );

                // act

                // assert
                await test.Should().ThrowAsync<AmbiguousActionException>().WithMessage( message );
            }
        }

        [Fact]
        public async Task select_best_candidate_should_throw_exception_for_ambiguous_neutral_and_versionedX2C_conventionX2Dbased_controller()
        {
            // arrange
            void ConfigureOptions( ApiVersioningOptions options )
            {
                options.UseApiBehavior = false;
                options.AssumeDefaultVersionWhenUnspecified = true;
            }

            var message = $"Multiple actions matched. The following actions matched route data and had all constraints satisfied:{NewLine}{NewLine}" +
                          $"Microsoft.AspNetCore.Mvc.Simulators.AmbiguousNeutralController.Get() (Microsoft.AspNetCore.Mvc.Versioning.Tests){NewLine}" +
                          $"Microsoft.AspNetCore.Mvc.Simulators.AmbiguousController.Get() (Microsoft.AspNetCore.Mvc.Versioning.Tests)";

            using ( var server = new WebServer( ConfigureOptions, r => r.MapRoute( "default", "api/{controller}/{action=Get}/{id?}" ) ) )
            {
                Func<Task> test = () => server.Client.GetAsync( "api/ambiguous" );

                // act

                // assert
                await test.Should().ThrowAsync<AmbiguousActionException>().WithMessage( message );
            }
        }

        [Fact]
        public async Task select_best_candidate_should_assume_current_version_for_attributeX2Dbased_controller_when_allowed()
        {
            // arrange
            var currentVersion = new ApiVersion( 4, 0 );
            var controllerType = typeof( AttributeRoutedTest4Controller ).GetTypeInfo();

            void ConfigureOptions( ApiVersioningOptions options )
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionSelector = new CurrentImplementationApiVersionSelector( options );
            }

            using ( var server = new WebServer( setupApiVersioning: ConfigureOptions ) )
            {
                await server.Client.GetAsync( "api/attributed" );

                // act
                var action = ( (TestApiVersionActionSelector) server.Services.GetRequiredService<IActionSelector>() ).SelectedCandidate;

                // assert
                action.As<ControllerActionDescriptor>().ControllerTypeInfo.Should().Be( controllerType );
            }
        }

        [Fact]
        public async Task select_best_candidate_should_assume_current_version_for_conventionX2Dbased_controller_when_allowed()
        {
            // arrange
            var currentVersion = new ApiVersion( 3, 0 );
            var controllerType = typeof( TestVersion2Controller ).GetTypeInfo();

            void ConfigureOptions( ApiVersioningOptions options )
            {
                options.UseApiBehavior = false;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionSelector = new CurrentImplementationApiVersionSelector( options );
            }

            using ( var server = new WebServer( ConfigureOptions, r => r.MapRoute( "default", "api/{controller}/{action=Get}/{id?}" ) ) )
            {
                await server.Client.GetAsync( "api/test" );

                // act
                var action = ( (TestApiVersionActionSelector) server.Services.GetRequiredService<IActionSelector>() ).SelectedCandidate;

                // assert
                action.As<ControllerActionDescriptor>().ControllerTypeInfo.Should().Be( controllerType );
            }
        }

        [Theory]
        [InlineData( "v1", typeof( ApiVersionedRouteController ), "Get", null )]
        [InlineData( "v1.0", typeof( ApiVersionedRouteController ), "Get", null )]
        [InlineData( "v2", typeof( ApiVersionedRouteController ), "Get", null )]
        [InlineData( "v3.0", typeof( ApiVersionedRouteController ), "Get", null )]
        [InlineData( "v4", typeof( ApiVersionedRoute2Controller ), "GetV4", "4.0" )]
        [InlineData( "v5", typeof( ApiVersionedRoute2Controller ), "Get", null )]
        public async Task select_best_candidate_should_return_correct_controller_for_versioned_route_attribute( string versionSegment, Type controllerType, string actionName, string declaredVersionsValue )
        {
            // arrange
            var declared = declaredVersionsValue == null ? Array.Empty<ApiVersion>() : declaredVersionsValue.Split( ',' ).Select( Parse ).ToArray();
            var supported = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ), new ApiVersion( 5, 0 ) };
            var deprecated = new[] { new ApiVersion( 4, 0 ) };
            var implemented = supported.Union( deprecated ).OrderBy( v => v ).ToArray();

            using ( var server = new WebServer( options => options.ReportApiVersions = true ) )
            {
                await server.Client.GetAsync( $"api/{versionSegment}/attributed" );

                // act
                var action = ( (TestApiVersionActionSelector) server.Services.GetRequiredService<IActionSelector>() ).SelectedCandidate;

                // assert
                action.As<ControllerActionDescriptor>().Should().BeEquivalentTo(
                    new
                    {
                        ActionName = actionName,
                        ControllerTypeInfo = controllerType.GetTypeInfo(),
                    },
                    options => options.ExcludingMissingMembers() );
                action.GetProperty<ApiVersionModel>().Should().BeEquivalentTo(
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

        [Fact]
        public async Task select_controller_should_return_400_when_requested_api_version_is_ambiguous()
        {
            // arrange
            void ConfigureOptions( ApiVersioningOptions options ) =>
                options.ApiVersionReader = ApiVersionReader.Combine( new QueryStringApiVersionReader(), new HeaderApiVersionReader( "api-version" ) );

            using ( var server = new WebServer( ConfigureOptions ) )
            {
                server.Client.DefaultRequestHeaders.TryAddWithoutValidation( "api-version", "1.0" );

                // act
                var response = await server.Client.GetAsync( $"api/attributed?api-version=2.0" );

                // assert
                response.StatusCode.Should().Be( BadRequest );
            }
        }

        [Fact]
        public async Task select_controller_should_resolve_controller_action_using_api_versioning_conventions()
        {
            // arrange
            void ConfigureOptions( ApiVersioningOptions options ) =>
                options.Conventions.Controller<ConventionsController>()
                                   .HasApiVersion( 1, 0 )
                                   .HasApiVersion( 2, 0 )
                                   .AdvertisesApiVersion( 3, 0 )
                                   .Action( c => c.GetV2() ).MapToApiVersion( 2, 0 )
                                   .Action( c => c.GetV2( default ) ).MapToApiVersion( 2, 0 );

            using ( var server = new WebServer( ConfigureOptions ) )
            {
                var response = await server.Client.GetAsync( $"api/conventions/1?api-version=2.0" );

                // act
                var action = ( (TestApiVersionActionSelector) server.Services.GetRequiredService<IActionSelector>() ).SelectedCandidate;

                // assert
                action.As<ControllerActionDescriptor>().ControllerTypeInfo.Should().Be( typeof( ConventionsController ).GetTypeInfo() );
                action.As<ControllerActionDescriptor>().ActionName.Should().Be( nameof( ConventionsController.GetV2 ) );
                action.Parameters.Count.Should().Be( 1 );
                action.GetProperty<ApiVersionModel>().Should().BeEquivalentTo(
                     new
                     {
                         IsApiVersionNeutral = false,
                         DeclaredApiVersions = new[] { new ApiVersion( 2, 0 ) },
                         ImplementedApiVersions = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) },
                         SupportedApiVersions = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) },
                         DeprecatedApiVersions = new ApiVersion[0]
                     } );
            }
        }
    }
}