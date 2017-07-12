namespace Microsoft.Web.Http.Description
{
    using FluentAssertions;
    using Moq;
    using System.Linq;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Description;
    using Xunit;
    using static Microsoft.Web.Http.Versioning.ApiVersionParameterLocation;
    using static System.Web.Http.Description.ApiParameterSource;

    public class ApiVersionParameterDescriptionContextTest
    {
        [Fact]
        public void add_parameter_should_add_descriptor_for_query_parameter()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var action = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            var description = new ApiDescription() { ActionDescriptor = action };
            var version = new ApiVersion( 1, 0 );
            var options = new ApiExplorerOptions( configuration );
            var context = new ApiVersionParameterDescriptionContext( description, version, options );

            action.Configuration = configuration;

            // act
            context.AddParameter( "api-version", Query );

            // assert
            description.ParameterDescriptions.Single().ShouldBeEquivalentTo(
                new
                {
                    Name = "api-version",
                    Documentation = options.DefaultApiVersionParameterDescription,
                    Source = FromUri,
                    ParameterDescriptor = new
                    {
                        ParameterName = "api-version",
                        DefaultValue = "1.0",
                        IsOptional = false,
                        Configuration = configuration,
                        ActionDescriptor = action
                    }
                },
                o => o.ExcludingMissingMembers() );
        }

        [Fact]
        public void add_parameter_should_add_descriptor_for_header()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var action = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            var description = new ApiDescription() { ActionDescriptor = action };
            var version = new ApiVersion( 1, 0 );
            var options = new ApiExplorerOptions( configuration );
            var context = new ApiVersionParameterDescriptionContext( description, version, options );

            action.Configuration = configuration;

            // act
            context.AddParameter( "api-version", Header );

            // assert
            description.ParameterDescriptions.Single().ShouldBeEquivalentTo(
                new
                {
                    Name = "api-version",
                    Documentation = options.DefaultApiVersionParameterDescription,
                    Source = Unknown,
                    ParameterDescriptor = new
                    {
                        ParameterName = "api-version",
                        DefaultValue = "1.0",
                        IsOptional = false,
                        Configuration = configuration,
                        ActionDescriptor = action
                    }
                },
                o => o.ExcludingMissingMembers() );
        }

        [Fact]
        public void add_parameter_should_add_descriptor_for_path()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var action = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            var description = new ApiDescription() { ActionDescriptor = action };
            var version = new ApiVersion( 1, 0 );
            var options = new ApiExplorerOptions( configuration );
            var context = new ApiVersionParameterDescriptionContext( description, version, options );

            action.Configuration = configuration;
            description.ParameterDescriptions.Add( new ApiParameterDescription() { Name = "api-version", Source = FromUri } );

            // act
            context.AddParameter( "api-version", Path );

            // assert
            description.ParameterDescriptions.Single().ShouldBeEquivalentTo(
                new
                {
                    Name = "api-version",
                    Documentation = options.DefaultApiVersionParameterDescription,
                    Source = FromUri,
                    ParameterDescriptor = new
                    {
                        ParameterName = "api-version",
                        DefaultValue = "1.0",
                        IsOptional = false,
                        Configuration = configuration,
                        ActionDescriptor = action
                    }
                },
                o => o.ExcludingMissingMembers() );
        }

        [Fact]
        public void add_parameter_should_remove_other_descriptors_after_path_parameter_is_added()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var action = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            var description = new ApiDescription() { ActionDescriptor = action };
            var version = new ApiVersion( 1, 0 );
            var options = new ApiExplorerOptions( configuration );
            var context = new ApiVersionParameterDescriptionContext( description, version, options );

            action.Configuration = configuration;
            description.ParameterDescriptions.Add( new ApiParameterDescription() { Name = "api-version", Source = FromUri } );

            // act
            context.AddParameter( "api-version", Query );
            context.AddParameter( "api-version", Path );

            // assert
            description.ParameterDescriptions.Should().HaveCount( 1 );
        }

        [Fact]
        public void add_parameter_should_not_add_query_parameter_after_path_parameter_has_been_added()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var action = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            var description = new ApiDescription() { ActionDescriptor = action };
            var version = new ApiVersion( 1, 0 );
            var options = new ApiExplorerOptions( configuration );
            var context = new ApiVersionParameterDescriptionContext( description, version, options );

            action.Configuration = configuration;
            description.ParameterDescriptions.Add( new ApiParameterDescription() { Name = "api-version", Source = FromUri } );

            // act
            context.AddParameter( "api-version", Path );
            context.AddParameter( "api-version", Query );

            // assert
            description.ParameterDescriptions.Should().HaveCount( 1 );
        }

        [Fact]
        public void add_parameter_should_add_descriptor_for_media_type_parameter()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var action = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            var json = new JsonMediaTypeFormatter();
            var formUrlEncoded = new FormUrlEncodedMediaTypeFormatter();

            configuration.Formatters.Clear();
            configuration.Formatters.Add( json );
            configuration.Formatters.Add( formUrlEncoded );
            action.Configuration = configuration;

            var description = new ApiDescription()
            {
                ActionDescriptor = action,
                SupportedRequestBodyFormatters = { json, formUrlEncoded }
            };
            var version = new ApiVersion( 1, 0 );
            var options = new ApiExplorerOptions( configuration );
            var context = new ApiVersionParameterDescriptionContext( description, version, options );

            // act
            context.AddParameter( "v", MediaTypeParameter );

            // assert
            var formatter = description.SupportedRequestBodyFormatters[0];

            foreach ( var mediaType in formatter.SupportedMediaTypes )
            {
                mediaType.Parameters.Single().Should().Be( new NameValueHeaderValue( "v", "1.0" ) );
            }

            formatter.Should().NotBeSameAs( json );
            formatter = description.SupportedRequestBodyFormatters[1];

            foreach ( var mediaType in formatter.SupportedMediaTypes )
            {
                mediaType.Parameters.Single().Should().Be( new NameValueHeaderValue( "v", "1.0" ) );
            }

            formatter.Should().NotBeSameAs( formUrlEncoded );
        }

        [Fact]
        public void add_parameter_should_add_optional_parameter_when_allowed()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var action = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            var description = new ApiDescription() { ActionDescriptor = action };
            var version = new ApiVersion( 1, 0 );
            var options = new ApiExplorerOptions( configuration );

            action.Configuration = configuration;
            configuration.AddApiVersioning( o => o.AssumeDefaultVersionWhenUnspecified = true );

            var context = new ApiVersionParameterDescriptionContext( description, version, options );

            // act
            context.AddParameter( "api-version", Query );

            // assert
            description.ParameterDescriptions.Single().ShouldBeEquivalentTo(
                new
                {
                    Name = "api-version",
                    Documentation = options.DefaultApiVersionParameterDescription,
                    Source = FromUri,
                    ParameterDescriptor = new
                    {
                        ParameterName = "api-version",
                        DefaultValue = "1.0",
                        IsOptional = true,
                        Configuration = configuration,
                        ActionDescriptor = action
                    }
                },
                o => o.ExcludingMissingMembers() );
        }

        [Fact]
        public void add_parameter_should_make_parameters_optional_after_first_parameter()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var action = new Mock<HttpActionDescriptor>() { CallBase = true }.Object;
            var description = new ApiDescription() { ActionDescriptor = action };
            var version = new ApiVersion( 1, 0 );
            var options = new ApiExplorerOptions( configuration );
            var context = new ApiVersionParameterDescriptionContext( description, version, options );

            action.Configuration = configuration;

            // act
            context.AddParameter( "api-version", Query );
            context.AddParameter( "api-version", Header );

            // assert
            description.ParameterDescriptions[0].ParameterDescriptor.IsOptional.Should().BeFalse();
            description.ParameterDescriptions[1].ParameterDescriptor.IsOptional.Should().BeTrue();
        }
    }
}