namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Moq;
    using System.Linq;
    using Xunit;
    using static Microsoft.AspNetCore.Mvc.Versioning.ApiVersionReader;
    using static Microsoft.AspNetCore.Mvc.Versioning.ApiVersionParameterLocation;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Mvc.Routing;

    public class ApiVersionParameterDescriptionContextTest
    {
        [Fact]
        public void add_parameter_should_add_descriptor_for_query_parameter()
        {
            // arrange
            var description = new ApiDescription();
            var version = new ApiVersion( 1, 0 );
            var modelMetadata = new Mock<ModelMetadata>( ModelMetadataIdentity.ForType( typeof( string ) ) );
            var options = new ApiExplorerOptions()
            {
                DefaultApiVersion = version,
                ApiVersionParameterSource = new QueryStringApiVersionReader()
            };
            var context = new ApiVersionParameterDescriptionContext( description, version, modelMetadata.Object, options );

            // act
            context.AddParameter( "api-version", Query );

            // assert
            description.ParameterDescriptions.Single().ShouldBeEquivalentTo(
                new
                {
                    Name = "api-version",
                    ModelMetadata = modelMetadata,
                    Source = BindingSource.Query,
                    RouteInfo = new ApiParameterRouteInfo()
                    {
                        DefaultValue = "1.0",
                        IsOptional = false
                    },
                    Type = typeof( string )
                },
                o => o.ExcludingMissingMembers() );
        }

        [Fact]
        public void add_parameter_should_add_descriptor_for_header()
        {
            // arrange
            var description = new ApiDescription();
            var version = new ApiVersion( 1, 0 );
            var modelMetadata = new Mock<ModelMetadata>( ModelMetadataIdentity.ForType( typeof( string ) ) );
            var options = new ApiExplorerOptions()
            {
                DefaultApiVersion = version,
                ApiVersionParameterSource = new HeaderApiVersionReader()
            };
            var context = new ApiVersionParameterDescriptionContext( description, version, modelMetadata.Object, options );

            // act
            context.AddParameter( "api-version", Header );

            // assert
            description.ParameterDescriptions.Single().ShouldBeEquivalentTo(
                new
                {
                    Name = "api-version",
                    ModelMetadata = modelMetadata,
                    Source = BindingSource.Header,
                    RouteInfo = new ApiParameterRouteInfo()
                    {
                        DefaultValue = "1.0",
                        IsOptional = false
                    },
                    Type = typeof( string )
                },
                o => o.ExcludingMissingMembers() );
        }

        [Fact]
        public void add_parameter_should_add_descriptor_for_path()
        {
            // arrange
            var parameter = new ApiParameterDescription()
            {
                Name = "api-version",
                RouteInfo = new ApiParameterRouteInfo()
                {
                    Constraints = new IRouteConstraint[] { new ApiVersionRouteConstraint() }
                },
                Source = BindingSource.Path
            };
            var description = new ApiDescription() { ParameterDescriptions = { parameter } };
            var version = new ApiVersion( 1, 0 );
            var modelMetadata = new Mock<ModelMetadata>( ModelMetadataIdentity.ForType( typeof( string ) ) );
            var options = new ApiExplorerOptions()
            {
                DefaultApiVersion = version,
                ApiVersionParameterSource = new UrlSegmentApiVersionReader()
            };
            var context = new ApiVersionParameterDescriptionContext( description, version, modelMetadata.Object, options );

            // act
            context.AddParameter( "api-version", Path );

            // assert
            description.ParameterDescriptions.Single().ShouldBeEquivalentTo(
                new
                {
                    Name = "api-version",
                    ModelMetadata = modelMetadata,
                    Source = BindingSource.Path,
                    RouteInfo = new ApiParameterRouteInfo()
                    {
                        DefaultValue = "1.0",
                        IsOptional = false,
                        Constraints = parameter.RouteInfo.Constraints
                    },
                    Type = typeof( string )
                },
                o => o.ExcludingMissingMembers() );
        }

        [Fact]
        public void add_parameter_should_remove_other_descriptors_after_path_parameter_is_added()
        {
            // arrange
            var parameter = new ApiParameterDescription()
            {
                Name = "api-version",
                RouteInfo = new ApiParameterRouteInfo()
                {
                    Constraints = new IRouteConstraint[] { new ApiVersionRouteConstraint() }
                },
                Source = BindingSource.Path
            };
            var description = new ApiDescription() { ParameterDescriptions = { parameter } };
            var version = new ApiVersion( 1, 0 );
            var modelMetadata = new Mock<ModelMetadata>( ModelMetadataIdentity.ForType( typeof( string ) ) );
            var options = new ApiExplorerOptions()
            {
                DefaultApiVersion = version,
                ApiVersionParameterSource = Combine( new QueryStringApiVersionReader(), new UrlSegmentApiVersionReader() )
            };
            var context = new ApiVersionParameterDescriptionContext( description, version, modelMetadata.Object, options );

            modelMetadata.SetupGet( m => m.DataTypeName ).Returns( nameof( ApiVersion ) );

            // act
            context.AddParameter( "api-version", Query );
            context.AddParameter( "api-version", Path );

            // assert
            description.ParameterDescriptions.Single().ShouldBeEquivalentTo(
                new
                {
                    Name = "api-version",
                    ModelMetadata = modelMetadata,
                    Source = BindingSource.Path,
                    RouteInfo = new ApiParameterRouteInfo()
                    {
                        DefaultValue = "1.0",
                        IsOptional = false,
                        Constraints = parameter.RouteInfo.Constraints
                    },
                    Type = typeof( string )
                },
                o => o.ExcludingMissingMembers() );
        }

        [Fact]
        public void add_parameter_should_not_add_query_parameter_after_path_parameter_has_been_added()
        {
            // arrange
            var parameter = new ApiParameterDescription()
            {
                Name = "api-version",
                RouteInfo = new ApiParameterRouteInfo()
                {
                    Constraints = new IRouteConstraint[] { new ApiVersionRouteConstraint() }
                },
                Source = BindingSource.Path
            };
            var description = new ApiDescription() { ParameterDescriptions = { parameter } };
            var version = new ApiVersion( 1, 0 );
            var modelMetadata = new Mock<ModelMetadata>( ModelMetadataIdentity.ForType( typeof( string ) ) );
            var options = new ApiExplorerOptions()
            {
                DefaultApiVersion = version,
                ApiVersionParameterSource = Combine( new QueryStringApiVersionReader(), new UrlSegmentApiVersionReader() )
            };
            var context = new ApiVersionParameterDescriptionContext( description, version, modelMetadata.Object, options );

            modelMetadata.SetupGet( m => m.DataTypeName ).Returns( nameof( ApiVersion ) );

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
            const string Json = "application/json";
            var description = new ApiDescription()
            {
                SupportedRequestFormats =
                {
                    new ApiRequestFormat() { MediaType = Json }
                },
                SupportedResponseTypes =
                {
                    new ApiResponseType()
                    {
                        ApiResponseFormats =
                        {
                            new ApiResponseFormat() { MediaType = Json }
                        }
                    }
                }
            };
            var version = new ApiVersion( 1, 0 );
            var modelMetadata = new Mock<ModelMetadata>( ModelMetadataIdentity.ForType( typeof( string ) ) );
            var options = new ApiExplorerOptions()
            {
                DefaultApiVersion = version,
                ApiVersionParameterSource = new MediaTypeApiVersionReader()
            };
            var context = new ApiVersionParameterDescriptionContext( description, version, modelMetadata.Object, options );

            // act
            context.AddParameter( "v", MediaTypeParameter );

            // assert
            description.SupportedRequestFormats.Single().MediaType.Should().Be( "application/json; v=1.0" );
            description.SupportedResponseTypes.Single().ApiResponseFormats.Single().MediaType.Should().Be( "application/json; v=1.0" );
        }

        [Fact]
        public void add_parameter_should_add_optional_parameter_when_allowed()
        {
            // arrange
            var description = new ApiDescription();
            var version = new ApiVersion( 1, 0 );
            var modelMetadata = new Mock<ModelMetadata>( ModelMetadataIdentity.ForType( typeof( string ) ) );
            var options = new ApiExplorerOptions()
            {
                DefaultApiVersion = version,
                ApiVersionParameterSource = new QueryStringApiVersionReader(),
                AssumeDefaultVersionWhenUnspecified = true
            };
            var context = new ApiVersionParameterDescriptionContext( description, version, modelMetadata.Object, options );

            // act
            context.AddParameter( "api-version", Query );

            // assert
            description.ParameterDescriptions.Single().ShouldBeEquivalentTo(
                new
                {
                    Name = "api-version",
                    ModelMetadata = modelMetadata,
                    Source = BindingSource.Query,
                    RouteInfo = new ApiParameterRouteInfo()
                    {
                        DefaultValue = "1.0",
                        IsOptional = true
                    },
                    Type = typeof( string )
                },
                o => o.ExcludingMissingMembers() );
        }

        [Fact]
        public void add_parameter_should_make_parameters_optional_after_first_parameter()
        {
            // arrange
            var description = new ApiDescription();
            var version = new ApiVersion( 1, 0 );
            var modelMetadata = new Mock<ModelMetadata>( ModelMetadataIdentity.ForType( typeof( string ) ) );
            var options = new ApiExplorerOptions()
            {
                DefaultApiVersion = version,
                ApiVersionParameterSource = Combine( new QueryStringApiVersionReader(), new HeaderApiVersionReader() )
            };
            var context = new ApiVersionParameterDescriptionContext( description, version, modelMetadata.Object, options );

            // act
            context.AddParameter( "api-version", Query );
            context.AddParameter( "api-version", Header );

            // assert
            description.ParameterDescriptions[0].RouteInfo.IsOptional.Should().BeFalse();
            description.ParameterDescriptions[1].RouteInfo.IsOptional.Should().BeTrue();
        }
    }
}