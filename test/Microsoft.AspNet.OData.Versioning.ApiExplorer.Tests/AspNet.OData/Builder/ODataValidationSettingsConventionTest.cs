namespace Microsoft.AspNet.OData.Builder
{
    using FluentAssertions;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http.Description;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Description;
    using Xunit;
    using static Microsoft.AspNet.OData.Query.AllowedArithmeticOperators;
    using static Microsoft.AspNet.OData.Query.AllowedFunctions;
    using static Microsoft.AspNet.OData.Query.AllowedLogicalOperators;
    using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
    using static Moq.Times;
    using static System.Web.Http.Description.ApiParameterSource;

    public class ODataValidationSettingsConventionTest
    {
        [Theory]
        [InlineData( "PUT" )]
        [InlineData( "PATCH" )]
        [InlineData( "DELETE" )]
        public void apply_to_should_ignore_nonquery_and_nonaction_descriptions( string httpMethod )
        {
            // arrange
            var description = NewApiDescription( httpMethod );
            var validationSettings = new ODataValidationSettings();
            var settings = new TestODataQueryOptionSettings();
            var convention = new ODataValidationSettingsConvention( validationSettings, settings );

            // act
            convention.ApplyTo( description );

            // assert
            description.ParameterDescriptions.Should().BeEmpty();
        }

        [Theory]
        [InlineData( "$filter" )]
        [InlineData( "filter" )]
        public void apply_to_should_add_filter_parameter_description( string name )
        {
            // arrange
            var dollarPrefix = name[0] == '$';
            var description = NewApiDescription();
            var validationSettings = new ODataValidationSettings() { AllowedQueryOptions = Filter };
            var settings = new TestODataQueryOptionSettings( dollarPrefix );
            var convention = new ODataValidationSettingsConvention( validationSettings, settings );

            // act
            convention.ApplyTo( description );

            // assert
            description.ParameterDescriptions.Single().Should().BeEquivalentTo(
                new
                {
                    Name = name,
                    Documentation = "Test",
                    Source = FromUri,
                    ParameterDescriptor = new
                    {
                        ParameterName = name,
                        ParameterType = typeof( string ),
                        Prefix = "$",
                        IsOptional = true,
                        DefaultValue = default( object ),
                    }
                },
                options => options.ExcludingMissingMembers() );
            settings.MockDescriptionProvider.Verify( p => p.Describe( Filter, It.IsAny<ODataQueryOptionDescriptionContext>() ), Once() );
        }

        [Theory]
        [InlineData( "$expand" )]
        [InlineData( "expand" )]
        public void apply_to_should_add_expand_parameter_description( string name )
        {
            // arrange
            var dollarPrefix = name[0] == '$';
            var description = NewApiDescription();
            var validationSettings = new ODataValidationSettings() { AllowedQueryOptions = Expand };
            var settings = new TestODataQueryOptionSettings( dollarPrefix );
            var convention = new ODataValidationSettingsConvention( validationSettings, settings );

            // act
            convention.ApplyTo( description );

            // assert
            description.ParameterDescriptions.Single().Should().BeEquivalentTo(
                new
                {
                    Name = name,
                    Documentation = "Test",
                    Source = FromUri,
                    ParameterDescriptor = new
                    {
                        ParameterName = name,
                        ParameterType = typeof( string ),
                        Prefix = "$",
                        IsOptional = true,
                        DefaultValue = default( object ),
                    }
                },
                options => options.ExcludingMissingMembers() );
            settings.MockDescriptionProvider.Verify( p => p.Describe( Expand, It.IsAny<ODataQueryOptionDescriptionContext>() ), Once() );
        }

        [Theory]
        [InlineData( "$select" )]
        [InlineData( "select" )]
        public void apply_to_should_add_select_parameter_description( string name )
        {
            // arrange
            var dollarPrefix = name[0] == '$';
            var description = NewApiDescription();
            var validationSettings = new ODataValidationSettings() { AllowedQueryOptions = Select };
            var settings = new TestODataQueryOptionSettings( dollarPrefix );
            var convention = new ODataValidationSettingsConvention( validationSettings, settings );

            // act
            convention.ApplyTo( description );

            // assert
            description.ParameterDescriptions.Single().Should().BeEquivalentTo(
                new
                {
                    Name = name,
                    Documentation = "Test",
                    Source = FromUri,
                    ParameterDescriptor = new
                    {
                        ParameterName = name,
                        ParameterType = typeof( string ),
                        Prefix = "$",
                        IsOptional = true,
                        DefaultValue = default( object ),
                    }
                },
                options => options.ExcludingMissingMembers() );
            settings.MockDescriptionProvider.Verify( p => p.Describe( Select, It.IsAny<ODataQueryOptionDescriptionContext>() ), Once() );
        }

        [Theory]
        [InlineData( "$orderby" )]
        [InlineData( "orderby" )]
        public void apply_to_should_add_orderby_parameter_description( string name )
        {
            // arrange
            var dollarPrefix = name[0] == '$';
            var description = NewApiDescription();
            var validationSettings = new ODataValidationSettings() { AllowedQueryOptions = OrderBy };
            var settings = new TestODataQueryOptionSettings( dollarPrefix );
            var convention = new ODataValidationSettingsConvention( validationSettings, settings );

            // act
            convention.ApplyTo( description );

            // assert
            description.ParameterDescriptions.Single().Should().BeEquivalentTo(
                new
                {
                    Name = name,
                    Documentation = "Test",
                    Source = FromUri,
                    ParameterDescriptor = new
                    {
                        ParameterName = name,
                        ParameterType = typeof( string ),
                        Prefix = "$",
                        IsOptional = true,
                        DefaultValue = default( object ),
                    }
                },
                options => options.ExcludingMissingMembers() );
            settings.MockDescriptionProvider.Verify( p => p.Describe( OrderBy, It.IsAny<ODataQueryOptionDescriptionContext>() ), Once() );
        }

        [Theory]
        [InlineData( "$top" )]
        [InlineData( "top" )]
        public void apply_to_should_add_top_parameter_description( string name )
        {
            // arrange
            var dollarPrefix = name[0] == '$';
            var description = NewApiDescription();
            var validationSettings = new ODataValidationSettings() { AllowedQueryOptions = Top };
            var settings = new TestODataQueryOptionSettings( dollarPrefix );
            var convention = new ODataValidationSettingsConvention( validationSettings, settings );

            // act
            convention.ApplyTo( description );

            // assert
            description.ParameterDescriptions.Single().Should().BeEquivalentTo(
                new
                {
                    Name = name,
                    Documentation = "Test",
                    Source = FromUri,
                    ParameterDescriptor = new
                    {
                        ParameterName = name,
                        ParameterType = typeof( int ),
                        Prefix = "$",
                        IsOptional = true,
                        DefaultValue = default( object ),
                    }
                },
                options => options.ExcludingMissingMembers() );
            settings.MockDescriptionProvider.Verify( p => p.Describe( Top, It.IsAny<ODataQueryOptionDescriptionContext>() ), Once() );
        }

        [Theory]
        [InlineData( "$skip" )]
        [InlineData( "skip" )]
        public void apply_to_should_add_skip_parameter_description( string name )
        {
            // arrange
            var dollarPrefix = name[0] == '$';
            var description = NewApiDescription();
            var validationSettings = new ODataValidationSettings() { AllowedQueryOptions = Skip };
            var settings = new TestODataQueryOptionSettings( dollarPrefix );
            var convention = new ODataValidationSettingsConvention( validationSettings, settings );

            // act
            convention.ApplyTo( description );

            // assert
            description.ParameterDescriptions.Single().Should().BeEquivalentTo(
                new
                {
                    Name = name,
                    Documentation = "Test",
                    Source = FromUri,
                    ParameterDescriptor = new
                    {
                        ParameterName = name,
                        ParameterType = typeof( int ),
                        Prefix = "$",
                        IsOptional = true,
                        DefaultValue = default( object ),
                    }
                },
                options => options.ExcludingMissingMembers() );
            settings.MockDescriptionProvider.Verify( p => p.Describe( Skip, It.IsAny<ODataQueryOptionDescriptionContext>() ), Once() );
        }

        [Theory]
        [InlineData( "$count" )]
        [InlineData( "count" )]
        public void apply_to_should_add_count_parameter_description( string name )
        {
            // arrange
            var dollarPrefix = name[0] == '$';
            var description = NewApiDescription();
            var validationSettings = new ODataValidationSettings() { AllowedQueryOptions = Count };
            var settings = new TestODataQueryOptionSettings( dollarPrefix );
            var convention = new ODataValidationSettingsConvention( validationSettings, settings );

            // act
            convention.ApplyTo( description );

            // assert
            description.ParameterDescriptions.Single().Should().BeEquivalentTo(
                new
                {
                    Name = name,
                    Documentation = "Test",
                    Source = FromUri,
                    ParameterDescriptor = new
                    {
                        ParameterName = name,
                        ParameterType = typeof( bool ),
                        Prefix = "$",
                        IsOptional = true,
                        DefaultValue = false,
                    }
                },
                options => options.ExcludingMissingMembers() );
            settings.MockDescriptionProvider.Verify( p => p.Describe( Count, It.IsAny<ODataQueryOptionDescriptionContext>() ), Once() );
        }

        [Fact]
        public void apply_to_should_use_default_query_settings()
        {
            // arrange
            var description = NewApiDescription();
            var configuration = description.ActionDescriptor.Configuration;
            var validationSettings = new ODataValidationSettings() { AllowedQueryOptions = AllowedQueryOptions.None };
            var settings = new TestODataQueryOptionSettings();
            var convention = new ODataValidationSettingsConvention( validationSettings, settings );

            configuration.Count().Expand().Filter().OrderBy().Select();

            // act
            convention.ApplyTo( description );

            // assert
            description.ParameterDescriptions.Count.Should().Be( 5 );
        }

        [Theory]
        [MemberData( nameof( EnableQueryAttributeData ) )]
        public void apply_to_should_use_enable_query_attribute( ApiDescription description )
        {
            // arrange
            var validationSettings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.None,
                AllowedArithmeticOperators = AllowedArithmeticOperators.None,
                AllowedLogicalOperators = AllowedLogicalOperators.None,
                AllowedFunctions = AllowedFunctions.None,
            };
            var settings = new TestODataQueryOptionSettings();
            var convention = new ODataValidationSettingsConvention( validationSettings, settings );

            // act
            convention.ApplyTo( description );

            // assert
            description.ParameterDescriptions.Should().BeEquivalentTo(
                new[]
                {
                    new
                    {
                        Name = "$select",
                        Documentation = "Test",
                        Source = FromUri,
                        ParameterDescriptor = new
                        {
                            ParameterName = "$select",
                            ParameterType = typeof( string ),
                            Prefix = "$",
                            IsOptional = true,
                            DefaultValue = default( object ),
                        }
                    },
                    new
                    {
                        Name = "$expand",
                        Documentation = "Test",
                        Source = FromUri,
                        ParameterDescriptor = new
                        {
                            ParameterName = "$expand",
                            ParameterType = typeof( string ),
                            Prefix = "$",
                            IsOptional = true,
                            DefaultValue = default( object ),
                        }
                    },
                    new
                    {
                        Name = "$filter",
                        Documentation = "Test",
                        Source = FromUri,
                        ParameterDescriptor = new
                        {
                            ParameterName = "$filter",
                            ParameterType = typeof( string ),
                            Prefix = "$",
                            IsOptional = true,
                            DefaultValue = default( object ),
                        }
                    },
                },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public void apply_to_should_use_model_bound_query_attributes()
        {
            // arrange
            var builder = new ODataConventionModelBuilder().EnableLowerCamelCase();

            builder.EntitySet<Order>( "Orders" );

            var validationSettings = new ODataValidationSettings()
            {
                AllowedQueryOptions = AllowedQueryOptions.None,
                AllowedArithmeticOperators = AllowedArithmeticOperators.None,
                AllowedLogicalOperators = AllowedLogicalOperators.None,
                AllowedFunctions = AllowedFunctions.None,
            };
            var settings = new TestODataQueryOptionSettings();
            var convention = new ODataValidationSettingsConvention( validationSettings, settings );
            var model = builder.GetEdmModel();
            var description = NewApiDescription( typeof( OrdersController ), typeof( IEnumerable<Order> ), model );

            // act
            convention.ApplyTo( description );

            // assert
            description.ParameterDescriptions.Should().BeEquivalentTo(
                new[]
                {
                    new
                    {
                        Name = "$select",
                        Documentation = "Test",
                        Source = FromUri,
                        ParameterDescriptor = new
                        {
                            ParameterName = "$select",
                            ParameterType = typeof( string ),
                            Prefix = "$",
                            IsOptional = true,
                            DefaultValue = default( object ),
                        }
                    },
                    new
                    {
                        Name = "$filter",
                        Documentation = "Test",
                        Source = FromUri,
                        ParameterDescriptor = new
                        {
                            ParameterName = "$filter",
                            ParameterType = typeof( string ),
                            Prefix = "$",
                            IsOptional = true,
                            DefaultValue = default( object ),
                        }
                    },
                    new
                    {
                        Name = "$orderby",
                        Documentation = "Test",
                        Source = FromUri,
                        ParameterDescriptor = new
                        {
                            ParameterName = "$orderby",
                            ParameterType = typeof( string ),
                            Prefix = "$",
                            IsOptional = true,
                            DefaultValue = default( object ),
                        }
                    },
                    new
                    {
                        Name = "$count",
                        Documentation = "Test",
                        Source = FromUri,
                        ParameterDescriptor = new
                        {
                            ParameterName = "$count",
                            ParameterType = typeof( bool ),
                            Prefix = "$",
                            IsOptional = true,
                            DefaultValue = (object) false,
                        }
                    },
                },
                options => options.ExcludingMissingMembers() );
        }

        public static IEnumerable<object[]> EnableQueryAttributeData
        {
            get
            {
                yield return new object[] { NewApiDescription( typeof( SinglePartController ) ) };
                yield return new object[] { NewApiDescription( typeof( MultipartController ) ) };
            }
        }

        static ApiDescription NewApiDescription( string method = "GET", bool singleResult = default )
        {
            var controller = new Mock<HttpControllerDescriptor>() { CallBase = true };
            var action = new Mock<HttpActionDescriptor>() { CallBase = true };

            controller.Setup( m => m.GetCustomAttributes<EnableQueryAttribute>( It.IsAny<bool>() ) ).Returns( new Collection<EnableQueryAttribute>() );
            action.Setup( m => m.GetCustomAttributes<EnableQueryAttribute>( It.IsAny<bool>() ) ).Returns( new Collection<EnableQueryAttribute>() );

            var actionDescriptor = action.Object;
            var responseType = singleResult ? typeof( object ) : typeof( IEnumerable<object> );

            actionDescriptor.Configuration = new HttpConfiguration();
            actionDescriptor.ControllerDescriptor = controller.Object;

            return new VersionedApiDescription()
            {
                ActionDescriptor = actionDescriptor,
                HttpMethod = new HttpMethod( method ),
                ResponseDescription = new ResponseDescription() { ResponseType = responseType },
                Properties = { [typeof( IEdmModel )] = new EdmModel() },
            };
        }

        static ApiDescription NewApiDescription( Type controllerType ) =>
            NewApiDescription( controllerType, typeof( IEnumerable<object> ), new EdmModel() );

        static ApiDescription NewApiDescription( Type controllerType, Type responseType, IEdmModel model )
        {
            var configuration = new HttpConfiguration();
            var controllerDescriptor = new HttpControllerDescriptor( configuration, "Orders", controllerType );
            var method = controllerType.GetMethod( "Get" );
            var actionDescriptor = new ReflectedHttpActionDescriptor( controllerDescriptor, method ) { Configuration = configuration };

            return new VersionedApiDescription()
            {
                ActionDescriptor = actionDescriptor,
                HttpMethod = HttpMethod.Get,
                ResponseDescription = new ResponseDescription() { ResponseType = responseType },
                Properties = { [typeof( IEdmModel )] = model },
            };
        }

        public class SinglePartController : ODataController
        {
            [EnableQuery( MaxTop = 100,
                          MaxOrderByNodeCount = 3,
                          AllowedQueryOptions = Select | Expand | Filter,
                          AllowedArithmeticOperators = Add | Subtract,
                          AllowedFunctions = StartsWith | EndsWith | Contains,
                          AllowedLogicalOperators = And | Or,
                          AllowedOrderByProperties = "name,price,quantity" )]
            [ResponseType( typeof( IEnumerable<object> ) )]
            public IHttpActionResult Get() => Ok();
        }

        [EnableQuery( MaxTop = 100,
                      MaxOrderByNodeCount = 3,
                      AllowedArithmeticOperators = AllowedArithmeticOperators.None,
                      AllowedFunctions = AllowedFunctions.None,
                      AllowedLogicalOperators = AllowedLogicalOperators.None,
                      AllowedQueryOptions = SkipToken | DeltaToken )]
        public class MultipartController : ODataController
        {
            [EnableQuery( AllowedQueryOptions = Select | Expand | Filter,
                          AllowedArithmeticOperators = Add | Subtract,
                          AllowedFunctions = StartsWith | EndsWith | Contains,
                          AllowedLogicalOperators = And | Or,
                          AllowedOrderByProperties = "name,price,quantity" )]
            [ResponseType( typeof( IEnumerable<object> ) )]
            public IHttpActionResult Get() => Ok();
        }

        public class OrdersController : ODataController
        {
            [ResponseType( typeof( IEnumerable<Order> ) )]
            public IHttpActionResult Get( ODataQueryOptions<Order> options ) => Ok();
        }

        [Select]
        [Filter]
        [Count]
        [OrderBy( "name" )]
        public class Order
        {
            public int OrderId { get; set; }

            public string Name { get; set; }

            public decimal Price { get; set; }

            public int Quantity { get; set; }
        }

        sealed class TestODataQueryOptionSettings : ODataQueryOptionSettings
        {
            internal TestODataQueryOptionSettings( bool dollarPrefix = true )
            {
                MockDescriptionProvider = new Mock<IODataQueryOptionDescriptionProvider>();
                MockDescriptionProvider.Setup( p => p.Describe( It.IsAny<AllowedQueryOptions>(), It.IsAny<ODataQueryOptionDescriptionContext>() ) ).Returns( "Test" );
                NoDollarPrefix = !dollarPrefix;
                DescriptionProvider = MockDescriptionProvider.Object;
            }

            internal Mock<IODataQueryOptionDescriptionProvider> MockDescriptionProvider { get; }
        }
    }
}