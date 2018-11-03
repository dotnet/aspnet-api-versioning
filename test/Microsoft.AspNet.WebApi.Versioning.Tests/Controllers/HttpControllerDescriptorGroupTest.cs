namespace Microsoft.Web.Http.Controllers
{
    using FluentAssertions;
    using Moq;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    using Versioning;
    using Xunit;
    using static Moq.Times;

    public class HttpControllerDescriptorGroupTest
    {
        [Fact]
        public void get_enumerator_should_iterate_over_expected_items()
        {
            // arrange
            var expected = NewControllerDescriptors( 3 );

            // act
            var group = new HttpControllerDescriptorGroup( expected );

            // assert
            group.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void indexer_should_return_expected_item()
        {
            // arrange
            var expected = NewControllerDescriptors( 3 );
            var group = new HttpControllerDescriptorGroup( expected );
            var list = new List<HttpControllerDescriptor>();

            // act
            for ( var i = 0; i < group.Count; i++ )
            {
                list.Add( group[i] );
            }

            // assert
            list.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void get_custom_attributes_should_aggregate_attributes()
        {
            // arrange
            var descriptor1 = new Mock<HttpControllerDescriptor>() { CallBase = true };
            var descriptor2 = new Mock<HttpControllerDescriptor>() { CallBase = true };
            var configuration = new HttpConfiguration();

            descriptor1.Setup( d => d.GetCustomAttributes<ApiVersionAttribute>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<ApiVersionAttribute>() { new ApiVersionAttribute( "1.0" ) } );
            descriptor1.Setup( d => d.GetCustomAttributes<IApiVersionProvider>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionProvider>() );
            descriptor1.Setup( d => d.GetCustomAttributes<IApiVersionNeutral>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionNeutral>() );
            descriptor1.Object.Configuration = configuration;
            descriptor1.Object.Properties[typeof( ApiVersionModel )] = new ApiVersionModel( new ApiVersion( 1, 0 ) );

            descriptor2.Setup( d => d.GetCustomAttributes<ApiVersionAttribute>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<ApiVersionAttribute>() { new ApiVersionAttribute( "2.0" ) } );
            descriptor2.Setup( d => d.GetCustomAttributes<IApiVersionProvider>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionProvider>() );
            descriptor2.Setup( d => d.GetCustomAttributes<IApiVersionNeutral>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionNeutral>() );
            descriptor2.Object.Configuration = configuration;
            descriptor2.Object.Properties[typeof( ApiVersionModel )] = new ApiVersionModel( new ApiVersion( 2, 0 ) );

            var group = new HttpControllerDescriptorGroup( descriptor1.Object, descriptor2.Object );
            var expected = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ) };

            // act
            var attributes = group.GetCustomAttributes<ApiVersionAttribute>();

            // assert
            attributes.SelectMany( a => a.Versions ).Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void get_filters_should_aggregate_filters()
        {
            // arrange
            var filter1 = new Mock<IFilter>().Object;
            var filter2 = new Mock<IFilter>().Object;
            var descriptor1 = new Mock<HttpControllerDescriptor>() { CallBase = true };
            var descriptor2 = new Mock<HttpControllerDescriptor>() { CallBase = true };
            var configuration = new HttpConfiguration();

            descriptor1.Setup( d => d.GetFilters() ).Returns( () => new Collection<IFilter>() { filter1 } );
            descriptor1.Setup( d => d.GetCustomAttributes<IApiVersionProvider>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionProvider>() );
            descriptor1.Setup( d => d.GetCustomAttributes<IApiVersionNeutral>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionNeutral>() );
            descriptor1.Object.Configuration = configuration;
            descriptor1.Object.Properties[typeof( ApiVersionModel )] = ApiVersionModel.Neutral;

            descriptor2.Setup( d => d.GetFilters() ).Returns( () => new Collection<IFilter>() { filter2 } );
            descriptor2.Setup( d => d.GetCustomAttributes<IApiVersionProvider>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionProvider>() );
            descriptor2.Setup( d => d.GetCustomAttributes<IApiVersionNeutral>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionNeutral>() );
            descriptor2.Object.Configuration = configuration;
            descriptor2.Object.Properties[typeof( ApiVersionModel )] = ApiVersionModel.Neutral;

            var group = new HttpControllerDescriptorGroup( descriptor1.Object, descriptor2.Object );

            // act
            var filters = group.GetFilters();

            // assert
            filters.Should().BeEquivalentTo( new[] { filter1, filter2 } );
        }

        [Fact]
        public void create_controller_should_return_expected_instance_when_count_eq_1()
        {
            // arrange
            var expected = new Mock<IHttpController>().Object;
            var descriptor = new Mock<HttpControllerDescriptor>();

            descriptor.Setup( d => d.CreateController( It.IsAny<HttpRequestMessage>() ) ).Returns( expected );

            var group = new HttpControllerDescriptorGroup( descriptor.Object );
            var request = new HttpRequestMessage();

            request.ApiVersionProperties().SelectedController = descriptor.Object;

            // act
            var controller = group.CreateController( request );

            // assert
            controller.Should().Be( expected );
        }

        [Fact]
        public void create_controller_should_return_first_instance_when_version_is_unspecified()
        {
            // arrange
            var expected = new Mock<IHttpController>().Object;
            var controller2 = new Mock<IHttpController>().Object;
            var descriptor1 = new Mock<HttpControllerDescriptor>() { CallBase = true };
            var descriptor2 = new Mock<HttpControllerDescriptor>() { CallBase = true };
            var configuration = new HttpConfiguration();

            descriptor1.Setup( d => d.CreateController( It.IsAny<HttpRequestMessage>() ) ).Returns( expected );
            descriptor1.Setup( d => d.GetCustomAttributes<IApiVersionProvider>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionProvider>() );
            descriptor1.Setup( d => d.GetCustomAttributes<IApiVersionNeutral>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionNeutral>() );
            descriptor1.Object.Configuration = configuration;
            descriptor1.Object.Properties[typeof( ApiVersionModel )] = ApiVersionModel.Neutral;

            descriptor2.Setup( d => d.CreateController( It.IsAny<HttpRequestMessage>() ) ).Returns( controller2 );
            descriptor2.Setup( d => d.GetCustomAttributes<IApiVersionProvider>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionProvider>() );
            descriptor2.Setup( d => d.GetCustomAttributes<IApiVersionNeutral>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionNeutral>() );
            descriptor2.Object.Configuration = configuration;
            descriptor2.Object.Properties[typeof( ApiVersionModel )] = ApiVersionModel.Neutral;

            var group = new HttpControllerDescriptorGroup( descriptor1.Object, descriptor2.Object );
            var request = new HttpRequestMessage();

            request.ApiVersionProperties().SelectedController = descriptor1.Object;

            // act
            var controller = group.CreateController( request );

            // assert
            controller.Should().Be( expected );
            descriptor1.Verify( d => d.CreateController( request ), Once() );
            descriptor2.Verify( d => d.CreateController( request ), Never() );
        }

        [Fact]
        public void create_controller_should_return_versioned_controller_instance()
        {
            // arrange
            var expected = new Mock<IHttpController>().Object;
            var configuration = new HttpConfiguration();
            var controller1 = new Mock<IHttpController>().Object;
            var descriptor1 = new Mock<HttpControllerDescriptor>() { CallBase = true };
            var descriptor2 = new Mock<HttpControllerDescriptor>() { CallBase = true };

            descriptor1.Setup( d => d.GetCustomAttributes<IApiVersionProvider>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionProvider>() { new ApiVersionAttribute( "2.0" ) } );
            descriptor1.Setup( d => d.GetCustomAttributes<IApiVersionNeutral>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionNeutral>() );
            descriptor1.Setup( d => d.CreateController( It.IsAny<HttpRequestMessage>() ) ).Returns( controller1 );
            descriptor1.Object.Configuration = configuration;
            descriptor1.Object.Properties[typeof( ApiVersionModel )] = new ApiVersionModel( new ApiVersion( 2, 0 ) );

            descriptor2.Setup( d => d.GetCustomAttributes<IApiVersionProvider>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionProvider>() { new ApiVersionAttribute( "1.0" ) } );
            descriptor2.Setup( d => d.GetCustomAttributes<IApiVersionNeutral>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionNeutral>() );
            descriptor2.Setup( d => d.CreateController( It.IsAny<HttpRequestMessage>() ) ).Returns( expected );
            descriptor2.Object.Configuration = configuration;
            descriptor2.Object.Properties[typeof( ApiVersionModel )] = new ApiVersionModel( new ApiVersion( 1, 0 ) );

            var group = new HttpControllerDescriptorGroup( descriptor1.Object, descriptor2.Object ) { Configuration = configuration };
            var request = new HttpRequestMessage( HttpMethod.Get, "http://localhost/api/test?api-version=1.0" );

            request.ApiVersionProperties().SelectedController = descriptor2.Object;

            // act
            var controller = group.CreateController( request );

            // assert
            controller.Should().Be( expected );
            descriptor1.Verify( d => d.CreateController( request ), Never() );
            descriptor2.Verify( d => d.CreateController( request ), Once() );
        }

        [Fact]
        public void create_controller_should_return_default_instance_when_versioned_controller_instance_is_not_found()
        {
            // arrange
            var expected = new Mock<IHttpController>().Object;
            var configuration = new HttpConfiguration();
            var controller2 = new Mock<IHttpController>().Object;
            var descriptor1 = new Mock<HttpControllerDescriptor>() { CallBase = true };
            var descriptor2 = new Mock<HttpControllerDescriptor>() { CallBase = true };

            descriptor1.Setup( d => d.GetCustomAttributes<IApiVersionProvider>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionProvider>() { new ApiVersionAttribute( "1.0" ) } );
            descriptor1.Setup( d => d.GetCustomAttributes<IApiVersionNeutral>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionNeutral>() );
            descriptor1.Setup( d => d.CreateController( It.IsAny<HttpRequestMessage>() ) ).Returns( expected );
            descriptor1.Object.Configuration = configuration;
            descriptor1.Object.Properties[typeof( ApiVersionModel )] = new ApiVersionModel( new ApiVersion( 1, 0 ) );

            descriptor2.Setup( d => d.GetCustomAttributes<IApiVersionProvider>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionProvider>() { new ApiVersionAttribute( "2.0" ) } );
            descriptor2.Setup( d => d.GetCustomAttributes<IApiVersionNeutral>( It.IsAny<bool>() ) )
                       .Returns( () => new Collection<IApiVersionNeutral>() );
            descriptor2.Setup( d => d.CreateController( It.IsAny<HttpRequestMessage>() ) ).Returns( controller2 );
            descriptor2.Object.Configuration = configuration;
            descriptor2.Object.Properties[typeof( ApiVersionModel )] = new ApiVersionModel( new ApiVersion( 2, 0 ) );

            var group = new HttpControllerDescriptorGroup( descriptor1.Object, descriptor2.Object ) { Configuration = configuration };
            var request = new HttpRequestMessage( HttpMethod.Get, "http://localhost/api/test?api-version=3.0" );

            request.ApiVersionProperties().SelectedController = descriptor1.Object;

            // act
            var controller = group.CreateController( request );

            // assert
            controller.Should().Be( expected );
            descriptor1.Verify( d => d.CreateController( request ), Once() );
            descriptor2.Verify( d => d.CreateController( request ), Never() );
        }

        static IReadOnlyList<HttpControllerDescriptor> NewControllerDescriptors( int count )
        {
            var configuration = new HttpConfiguration();
            var list = new List<HttpControllerDescriptor>();

            for ( var i = 0; i < count; i++ )
            {
                list.Add( NewControllerDescriptor( configuration ) );
            }

            return list;
        }

        static HttpControllerDescriptor NewControllerDescriptor( HttpConfiguration configuration ) =>
                new HttpControllerDescriptor()
                {
                    Configuration = configuration,
                    ControllerType = typeof( IHttpController ),
                };
    }
}