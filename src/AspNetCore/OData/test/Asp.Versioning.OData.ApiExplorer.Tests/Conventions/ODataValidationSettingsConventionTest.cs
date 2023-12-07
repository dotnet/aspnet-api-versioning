// Copyright (c) .NET Foundation and contributors. All rights reserved.

//// Ignore Spelling: Dlike
//// Ignore Spelling: Multipart
//// Ignore Spelling: nonaction
//// Ignore Spelling: nonquery

namespace Asp.Versioning.Conventions;

using Asp.Versioning.OData;
using Asp.Versioning.Simulators.Models;
using Asp.Versioning.Simulators.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.ModelBuilder.Config;
using System.Reflection;
using Xunit;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource;
using static Microsoft.AspNetCore.OData.Query.AllowedArithmeticOperators;
using static Microsoft.AspNetCore.OData.Query.AllowedFunctions;
using static Microsoft.AspNetCore.OData.Query.AllowedLogicalOperators;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;
using static Moq.Times;

public class ODataValidationSettingsConventionTest
{
    [Fact]
    public void apply_to_should_ignore_nonquery_and_nonaction_description()
    {
        // arrange
        var description = NewApiDescription( "DELETE" );
        var validationSettings = new ODataValidationSettings();
        var settings = new TestODataQueryOptionSettings( typeof( object ) );
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
        var settings = new TestODataQueryOptionSettings( typeof( string ), dollarPrefix );
        var convention = new ODataValidationSettingsConvention( validationSettings, settings );

        // act
        convention.ApplyTo( description );

        // assert
        description.ParameterDescriptions.Single().Should().BeEquivalentTo(
            new
            {
                Name = name,
                Source = Query,
                Type = typeof( string ),
                DefaultValue = default( object ),
                IsRequired = false,
                ModelMetadata = new { Description = "Test" },
                ParameterDescriptor = new
                {
                    Name = name,
                    ParameterType = typeof( string ),
                },
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
        var settings = new TestODataQueryOptionSettings( typeof( string ), dollarPrefix );
        var convention = new ODataValidationSettingsConvention( validationSettings, settings );

        // act
        convention.ApplyTo( description );

        // assert
        description.ParameterDescriptions.Single().Should().BeEquivalentTo(
            new
            {
                Name = name,
                Source = Query,
                Type = typeof( string ),
                DefaultValue = default( object ),
                IsRequired = false,
                ModelMetadata = new { Description = "Test" },
                ParameterDescriptor = new
                {
                    Name = name,
                    ParameterType = typeof( string ),
                },
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
        var settings = new TestODataQueryOptionSettings( typeof( string ), dollarPrefix );
        var convention = new ODataValidationSettingsConvention( validationSettings, settings );

        // act
        convention.ApplyTo( description );

        // assert
        description.ParameterDescriptions.Single().Should().BeEquivalentTo(
            new
            {
                Name = name,
                Source = Query,
                Type = typeof( string ),
                DefaultValue = default( object ),
                IsRequired = false,
                ModelMetadata = new { Description = "Test" },
                ParameterDescriptor = new
                {
                    Name = name,
                    ParameterType = typeof( string ),
                },
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
        var settings = new TestODataQueryOptionSettings( typeof( string ), dollarPrefix );
        var convention = new ODataValidationSettingsConvention( validationSettings, settings );

        // act
        convention.ApplyTo( description );

        // assert
        description.ParameterDescriptions.Single().Should().BeEquivalentTo(
            new
            {
                Name = name,
                Source = Query,
                Type = typeof( string ),
                DefaultValue = default( object ),
                IsRequired = false,
                ModelMetadata = new { Description = "Test" },
                ParameterDescriptor = new
                {
                    Name = name,
                    ParameterType = typeof( string ),
                },
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
        var settings = new TestODataQueryOptionSettings( typeof( int ), dollarPrefix );
        var convention = new ODataValidationSettingsConvention( validationSettings, settings );

        // act
        convention.ApplyTo( description );

        // assert
        description.ParameterDescriptions.Single().Should().BeEquivalentTo(
            new
            {
                Name = name,
                Source = Query,
                Type = typeof( int ),
                DefaultValue = default( object ),
                IsRequired = false,
                ModelMetadata = new { Description = "Test" },
                ParameterDescriptor = new
                {
                    Name = name,
                    ParameterType = typeof( int ),
                },
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
        var settings = new TestODataQueryOptionSettings( typeof( int ), dollarPrefix );
        var convention = new ODataValidationSettingsConvention( validationSettings, settings );

        // act
        convention.ApplyTo( description );

        // assert
        description.ParameterDescriptions.Single().Should().BeEquivalentTo(
            new
            {
                Name = name,
                Source = Query,
                Type = typeof( int ),
                DefaultValue = default( object ),
                IsRequired = false,
                ModelMetadata = new { Description = "Test" },
                ParameterDescriptor = new
                {
                    Name = name,
                    ParameterType = typeof( int ),
                },
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
        var settings = new TestODataQueryOptionSettings( typeof( bool ), dollarPrefix );
        var convention = new ODataValidationSettingsConvention( validationSettings, settings );

        // act
        convention.ApplyTo( description );

        // assert
        description.ParameterDescriptions.Single().Should().BeEquivalentTo(
            new
            {
                Name = name,
                Source = Query,
                Type = typeof( bool ),
                DefaultValue = (object) false,
                IsRequired = false,
                ModelMetadata = new { Description = "Test" },
                ParameterDescriptor = new
                {
                    Name = name,
                    ParameterType = typeof( bool ),
                },
            },
            options => options.ExcludingMissingMembers() );
        settings.MockDescriptionProvider.Verify( p => p.Describe( Count, It.IsAny<ODataQueryOptionDescriptionContext>() ), Once() );
    }

    [Fact]
    public void apply_to_should_use_default_query_settings()
    {
        // arrange
        var description = NewApiDescription();
        var defaultQuerySettings = new DefaultQuerySettings()
        {
            EnableCount = true,
            EnableExpand = true,
            EnableFilter = true,
            EnableOrderBy = true,
            EnableSelect = true,
        };
        var validationSettings = new ODataValidationSettings() { AllowedQueryOptions = AllowedQueryOptions.None };
        var settings = new TestODataQueryOptionSettings( typeof( object ), defaultQuerySettings );
        var convention = new ODataValidationSettingsConvention( validationSettings, settings );

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
        var settings = new TestODataQueryOptionSettings( typeof( IEnumerable<object> ) );
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
                        Source = Query,
                        Type = typeof( string ),
                        DefaultValue = default( object ),
                        IsRequired = false,
                        ModelMetadata = new { Description = "Test" },
                        ParameterDescriptor = new
                        {
                            Name = "$select",
                            ParameterType = typeof( string ),
                        },
                    },
                    new
                    {
                        Name = "$expand",
                        Source = Query,
                        Type = typeof( string ),
                        DefaultValue = default( object ),
                        IsRequired = false,
                        ModelMetadata = new { Description = "Test" },
                        ParameterDescriptor = new
                        {
                            Name = "$expand",
                            ParameterType = typeof( string ),
                        },
                    },
                    new
                    {
                        Name = "$filter",
                        Source = Query,
                        Type = typeof( string ),
                        DefaultValue = default( object ),
                        IsRequired = false,
                        ModelMetadata = new { Description = "Test" },
                        ParameterDescriptor = new
                        {
                            Name = "$filter",
                            ParameterType = typeof( string ),
                        },
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
        var settings = new TestODataQueryOptionSettings( typeof( Order ) );
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
                    Source = Query,
                    Type = typeof( string ),
                    DefaultValue = default( object ),
                    IsRequired = false,
                    ModelMetadata = new { Description = "Test" },
                    ParameterDescriptor = new
                    {
                        Name = "$select",
                        ParameterType = typeof( string ),
                    },
                },
                new
                {
                    Name = "$filter",
                    Source = Query,
                    Type = typeof( string ),
                    DefaultValue = default( object ),
                    IsRequired = false,
                    ModelMetadata = new { Description = "Test" },
                    ParameterDescriptor = new
                    {
                        Name = "$filter",
                        ParameterType = typeof( string ),
                    },
                },
                new
                {
                    Name = "$orderby",
                    Source = Query,
                    Type = typeof( string ),
                    DefaultValue = default( object ),
                    IsRequired = false,
                    ModelMetadata = new { Description = "Test" },
                    ParameterDescriptor = new
                    {
                        Name = "$orderby",
                        ParameterType = typeof( string ),
                    },
                },
                new
                {
                    Name = "$count",
                    Source = Query,
                    Type = typeof( bool ),
                    DefaultValue = (object) false,
                    IsRequired = false,
                    ModelMetadata = new { Description = "Test" },
                    ParameterDescriptor = new
                    {
                        Name = "$count",
                        ParameterType = typeof( bool ),
                    },
                },
            },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public void apply_should_override_model_bound_settings_with_enable_query_attribute()
    {
        // arrange
        var builder = new ODataConventionModelBuilder().EnableLowerCamelCase();

        builder.EntitySet<Customer>( "Customers" );

        var validationSettings = new ODataValidationSettings()
        {
            AllowedQueryOptions = AllowedQueryOptions.None,
            AllowedArithmeticOperators = AllowedArithmeticOperators.None,
            AllowedLogicalOperators = AllowedLogicalOperators.None,
            AllowedFunctions = AllowedFunctions.None,
        };
        var settings = new TestODataQueryOptionSettings( typeof( Customer ) );
        var convention = new ODataValidationSettingsConvention( validationSettings, settings );
        var model = builder.GetEdmModel();
        var description = NewApiDescription( typeof( CustomersController ), typeof( IEnumerable<Customer> ), model );

        // act
        convention.ApplyTo( description );

        // assert
        var parameter = description.ParameterDescriptions.Single();

        parameter.Should().BeEquivalentTo(
            new
            {
                Name = "$filter",
                Source = Query,
                Type = typeof( string ),
                DefaultValue = default( object ),
                IsRequired = false,
                ModelMetadata = new { Description = "Test" },
                ParameterDescriptor = new
                {
                    Name = "$filter",
                    ParameterType = typeof( string ),
                },
            },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public void apply_to_should_process_odataX2Dlike_api_description()
    {
        // arrange
        var controllerType = typeof( BooksController );
        var action = controllerType.GetRuntimeMethods()
                                   .First( m => m.Name == "Get" && m.GetParameters().Length == 1 );
        var parameter = action.GetParameters()[0];
        var description = new ApiDescription()
        {
            ActionDescriptor = new ControllerActionDescriptor()
            {
                ControllerTypeInfo = controllerType.GetTypeInfo(),
                MethodInfo = action,
                Parameters = new ParameterDescriptor[]
                {
                    new()
                    {
                        Name = parameter.Name,
                        ParameterType = parameter.ParameterType,
                    },
                },
            },
            HttpMethod = "GET",
            SupportedResponseTypes =
            {
                new()
                {
                    Type = typeof( IEnumerable<Book> ),
                    StatusCode = Status200OK,
                },
            },
            Properties = { [typeof( ApiVersion )] = ApiVersion.Default },
        };
        var builder = new ODataQueryOptionsConventionBuilder();
        var settings = new ODataQueryOptionSettings()
        {
            DescriptionProvider = builder.DescriptionProvider,
            DefaultQuerySettings = new(),
            ModelMetadataProvider = Mock.Of<IModelMetadataProvider>(),
        };

        builder.Controller<BooksController>()
               .Action( c => c.Get( default ) )
               .Allow( Select | Count )
               .AllowOrderBy( "title", "published" );

        // act
        builder.ApplyTo( new[] { description }, settings );

        // assert
        description.ParameterDescriptions.Should().BeEquivalentTo(
            new[]
            {
                new
                {
                    Name = "$select",
                    Source = Query,
                    Type = typeof( string ),
                    DefaultValue = default( object ),
                    IsRequired = false,
                    ParameterDescriptor = new
                    {
                        Name = "$select",
                        ParameterType = typeof( string ),
                    },
                },
                new
                {
                    Name = "$orderby",
                    Source = Query,
                    Type = typeof( string ),
                    DefaultValue = default( object ),
                    IsRequired = false,
                    ParameterDescriptor = new
                    {
                        Name = "$orderby",
                        ParameterType = typeof( string ),
                    },
                },
                new
                {
                    Name = "$count",
                    Source = Query,
                    Type = typeof( bool ),
                    DefaultValue = (object) false,
                    IsRequired = false,
                    ParameterDescriptor = new
                    {
                        Name = "$count",
                        ParameterType = typeof( bool ),
                    },
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

    private static ApiDescription NewApiDescription( string method = "GET", bool singleResult = default )
    {
        var model = new EdmModel();

        model.SetAnnotationValue( model, new ApiVersionAnnotation( ApiVersion.Default ) );

        return new()
        {
            ActionDescriptor = new ControllerActionDescriptor()
            {
                ControllerTypeInfo = typeof( ControllerBase ).GetTypeInfo(),
                MethodInfo = typeof( ControllerBase ).GetRuntimeMethod( nameof( ControllerBase.Ok ), Type.EmptyTypes ),
                EndpointMetadata = new object[]
                {
                    new ODataRoutingMetadata( string.Empty, model, [] ),
                },
            },
            HttpMethod = method,
            SupportedResponseTypes =
            {
                new()
                {
                    Type = singleResult ? typeof( object ) : typeof( IEnumerable<object> ),
                    StatusCode = Status200OK,
                },
            },
            Properties = { [typeof( ApiVersion )] = ApiVersion.Default },
        };
    }

    private static ApiDescription NewApiDescription( Type controllerType ) =>
        NewApiDescription( controllerType, typeof( IEnumerable<object> ), new EdmModel() );

    private static ApiDescription NewApiDescription( Type controllerType, Type responseType, IEdmModel model )
    {
        model.SetAnnotationValue( model, new ApiVersionAnnotation( ApiVersion.Default ) );

        return new()
        {
            ActionDescriptor = new ControllerActionDescriptor()
            {
                ControllerTypeInfo = controllerType.GetTypeInfo(),
                MethodInfo = controllerType.GetRuntimeMethods().Single( m => m.Name == "Get" ),
                EndpointMetadata = new object[]
                {
                    new ODataRoutingMetadata( string.Empty, model, [] ),
                },
            },
            HttpMethod = "GET",
            SupportedResponseTypes =
            {
                new()
                {
                    Type = responseType,
                    StatusCode = Status200OK,
                },
            },
            Properties = { [typeof( ApiVersion )] = ApiVersion.Default },
        };
    }

#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable CA1034 // Nested types should not be visible

    public class SinglePartController : ODataController
    {
        [EnableQuery(
            MaxTop = 100,
            MaxOrderByNodeCount = 3,
            AllowedQueryOptions = Select | Expand | Filter,
            AllowedArithmeticOperators = Add | Subtract,
            AllowedFunctions = StartsWith | EndsWith | Contains,
            AllowedLogicalOperators = And | Or,
            AllowedOrderByProperties = "name,price,quantity" )]
        [ProducesResponseType( typeof( IEnumerable<object> ), Status200OK )]
        public IActionResult Get() => Ok();
    }

    [EnableQuery(
        MaxTop = 100,
        MaxOrderByNodeCount = 3,
        AllowedArithmeticOperators = AllowedArithmeticOperators.None,
        AllowedFunctions = AllowedFunctions.None,
        AllowedLogicalOperators = AllowedLogicalOperators.None,
        AllowedQueryOptions = SkipToken | DeltaToken )]
    public class MultipartController : ODataController
    {
        [EnableQuery(
            AllowedQueryOptions = Select | Expand | Filter,
            AllowedArithmeticOperators = Add | Subtract,
            AllowedFunctions = StartsWith | EndsWith | Contains,
            AllowedLogicalOperators = And | Or,
            AllowedOrderByProperties = "name,price,quantity" )]
        [ProducesResponseType( typeof( IEnumerable<object> ), Status200OK )]
        public IActionResult Get() => Ok();
    }

    public class OrdersController : ODataController
    {
        [ProducesResponseType( typeof( IEnumerable<Order> ), Status200OK )]
        public IActionResult Get( ODataQueryOptions<Order> options ) => Ok();
    }

    public class CustomersController : ODataController
    {
        [EnableQuery( AllowedQueryOptions = Filter )]
        [ProducesResponseType( typeof( IEnumerable<Customer> ), Status200OK )]
        public IActionResult Get( ODataQueryOptions<Customer> options ) => Ok();
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

    [Page( MaxTop = 25, PageSize = 25 )]
    public class Customer
    {
        public int CustomerId { get; set; }
    }

    private sealed class TestODataQueryOptionSettings : ODataQueryOptionSettings
    {
        internal TestODataQueryOptionSettings( Type type, bool dollarPrefix = true ) :
            this( type, new DefaultQuerySettings(), dollarPrefix )
        { }

        internal TestODataQueryOptionSettings(
            Type type,
            DefaultQuerySettings defaultQuerySettings,
            bool dollarPrefix = true )
        {
            MockDescriptionProvider = new Mock<IODataQueryOptionDescriptionProvider>();
            MockDescriptionProvider.Setup(
                p => p.Describe(
                    It.IsAny<AllowedQueryOptions>(),
                    It.IsAny<ODataQueryOptionDescriptionContext>() ) ).Returns( "Test" );
            NoDollarPrefix = !dollarPrefix;
            DescriptionProvider = MockDescriptionProvider.Object;
            ModelMetadataProvider = NewModelMetadataProvider( type );
            DefaultQuerySettings = defaultQuerySettings;
        }

        internal Mock<IODataQueryOptionDescriptionProvider> MockDescriptionProvider { get; }

        private static IModelMetadataProvider NewModelMetadataProvider( Type type )
        {
            var provider = new Mock<IModelMetadataProvider>();
            var identity = ModelMetadataIdentity.ForType( type );
            var metadata = new Mock<ModelMetadata>( identity ) { CallBase = true };

            provider.Setup( p => p.GetMetadataForType( type ) ).Returns( metadata.Object );

            return provider.Object;
        }
    }
}