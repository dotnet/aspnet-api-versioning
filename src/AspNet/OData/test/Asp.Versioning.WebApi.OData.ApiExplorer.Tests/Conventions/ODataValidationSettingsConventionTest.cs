// Copyright (c) .NET Foundation and contributors. All rights reserved.

//// Ignore Spelling: Dlike
//// Ignore Spelling: Multipart
//// Ignore Spelling: nonaction
//// Ignore Spelling: nonquery

namespace Asp.Versioning.Conventions;

using Asp.Versioning.Description;
using Asp.Versioning.Simulators.Models;
using Asp.Versioning.Simulators.V1;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using static Microsoft.AspNet.OData.Query.AllowedArithmeticOperators;
using static Microsoft.AspNet.OData.Query.AllowedFunctions;
using static Microsoft.AspNet.OData.Query.AllowedLogicalOperators;
using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
using static Moq.Times;
using static System.Web.Http.Description.ApiParameterSource;

public class ODataValidationSettingsConventionTest
{
    [Fact]
    public void apply_to_should_ignore_nonquery_and_nonaction_description()
    {
        // arrange
        var description = NewApiDescription( "DELETE" );
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
                    },
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
                    },
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
                    },
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
                    },
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
                    },
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
                    },
                },
            },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public void apply_to_should_process_odataX2Dlike_api_description()
    {
        // arrange
        var controllerType = typeof( BooksController );
        var controllerName = controllerType.Name.Substring( 0, controllerType.Name.Length - 10 );
        var action = controllerType.GetRuntimeMethods()
                                   .First( m => m.Name == "Get" && m.GetParameters().Length == 1 );
        var configuration = new HttpConfiguration();
        var controllerDescriptor = new HttpControllerDescriptor( configuration, controllerName, controllerType );
        var actionDescriptor = new ReflectedHttpActionDescriptor( controllerDescriptor, action ) { Configuration = configuration };
        var parameter = actionDescriptor.GetParameters()[0];
        var description = new VersionedApiDescription()
        {
            ActionDescriptor = actionDescriptor,
            HttpMethod = HttpMethod.Get,
            ParameterDescriptions =
            {
                new()
                {
                    Name = parameter.ParameterName,
                    ParameterDescriptor = parameter,
                    Source = Unknown,
                },
            },
            ResponseDescription = new() { ResponseType = typeof( IEnumerable<Book> ) },
        };
        var builder = new ODataQueryOptionsConventionBuilder();
        var settings = new ODataQueryOptionSettings()
        {
            DescriptionProvider = builder.DescriptionProvider,
            DefaultQuerySettings = new(),
        };

        configuration.EnableDependencyInjection();
        builder.Controller<BooksController>()
               .Action( c => c.Get( default ) )
               .Allow( Select | Count )
               .AllowOrderBy( "title", "published" );

        // act
        builder.ApplyTo( new[] { description }, settings );

        // assert
        description.ParameterDescriptions.RemoveAt( 0 );
        description.ParameterDescriptions.Should().BeEquivalentTo(
            new[]
            {
                new
                {
                    Name = "$select",
                    Source = FromUri,
                    ParameterDescriptor = new
                    {
                        ParameterName = "$select",
                        ParameterType = typeof( string ),
                        Prefix = "$",
                        IsOptional = true,
                        DefaultValue = default( object ),
                    },
                },
                new
                {
                    Name = "$orderby",
                    Source = FromUri,
                    ParameterDescriptor = new
                    {
                        ParameterName = "$orderby",
                        ParameterType = typeof( string ),
                        Prefix = "$",
                        IsOptional = true,
                        DefaultValue = default( object ),
                    },
                },
                new
                {
                    Name = "$count",
                    Source = FromUri,
                    ParameterDescriptor = new
                    {
                        ParameterName = "$count",
                        ParameterType = typeof( bool ),
                        Prefix = "$",
                        IsOptional = true,
                        DefaultValue = (object) false,
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
        var controller = new Mock<HttpControllerDescriptor>() { CallBase = true };
        var action = new Mock<HttpActionDescriptor>() { CallBase = true };

        controller.Setup( m => m.GetCustomAttributes<EnableQueryAttribute>( It.IsAny<bool>() ) ).Returns( [] );
        action.Setup( m => m.GetCustomAttributes<EnableQueryAttribute>( It.IsAny<bool>() ) ).Returns( [] );

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

    private static ApiDescription NewApiDescription( Type controllerType ) =>
        NewApiDescription( controllerType, typeof( IEnumerable<object> ), new EdmModel() );

    private static ApiDescription NewApiDescription( Type controllerType, Type responseType, IEdmModel model )
    {
        var configuration = new HttpConfiguration();
        var controllerName = controllerType.Name.Substring( 0, controllerType.Name.Length - 10 );
        var controllerDescriptor = new HttpControllerDescriptor( configuration, controllerName, controllerType );
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

#pragma warning disable IDE0060 // Remove unused parameter

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
        [ResponseType( typeof( IEnumerable<object> ) )]
        public IHttpActionResult Get() => Ok();
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

    private sealed class TestODataQueryOptionSettings : ODataQueryOptionSettings
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