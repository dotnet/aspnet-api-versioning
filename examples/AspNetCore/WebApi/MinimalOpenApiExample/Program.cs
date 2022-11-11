using ApiVersioning.Examples;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using OrderV1 = ApiVersioning.Examples.Models.V1.Order;
using OrderV2 = ApiVersioning.Examples.Models.V2.Order;
using OrderV3 = ApiVersioning.Examples.Models.V3.Order;
using PersonV1 = ApiVersioning.Examples.Models.V1.Person;
using PersonV2 = ApiVersioning.Examples.Models.V2.Person;
using PersonV3 = ApiVersioning.Examples.Models.V3.Person;

var builder = WebApplication.CreateBuilder( args );
var services = builder.Services;

// Add services to the container.
services.AddEndpointsApiExplorer();
services.AddApiVersioning(
            options =>
            {
                // reporting api versions will return the headers
                // "api-supported-versions" and "api-deprecated-versions"
                options.ReportApiVersions = true;

                options.Policies.Sunset( 0.9 )
                                .Effective( DateTimeOffset.Now.AddDays( 60 ) )
                                .Link( "policy.html" )
                                    .Title( "Versioning Policy" )
                                    .Type( "text/html" );
            } )
        .AddApiExplorer(
            options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";

                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;
            } )
        // this enables binding ApiVersion as a endpoint callback parameter. if you don't use it, then
        // you should remove this configuration.
        .EnableApiVersionBinding();
services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
services.AddSwaggerGen( options => options.OperationFilter<SwaggerDefaultValues>() );

// Configure the HTTP request pipeline.
var app = builder.Build();
var orders = app.MapGroup( "/api/orders" ).WithApiVersionSet( "Orders" );
var people = app.MapGroup( "/api/v{version:apiVersion}/people" ).WithApiVersionSet( "People" );

// 1.0
orders.MapGet( "/{id:int}", ( int id ) => new OrderV1() { Id = id, Customer = "John Doe" } )
      .Produces<OrderV1>()
      .Produces( 404 )
      .HasDeprecatedApiVersion( 0.9 )
      .HasApiVersion( 1.0 );

orders.MapPost( "/", ( HttpRequest request, OrderV1 order ) =>
       {
           order.Id = 42;
           var scheme = request.Scheme;
           var host = request.Host;
           var location = new Uri( $"{scheme}{Uri.SchemeDelimiter}{host}/api/orders/{order.Id}" );
           return Results.Created( location, order );
       } )
      .Accepts<OrderV1>( "application/json" )
      .Produces<OrderV1>( 201 )
      .Produces( 400 )
      .HasApiVersion( 1.0 );

orders.MapMethods( "/{id:int}", new[] { HttpMethod.Patch.Method }, ( int id, OrderV1 order ) => Results.NoContent() )
      .Accepts<OrderV1>( "application/json" )
      .Produces( 204 )
      .Produces( 400 )
      .Produces( 404 )
      .HasApiVersion( 1.0 );

// 2.0
orders.MapGet( "/", () =>
       new OrderV2[]
       {
           new(){ Id = 1, Customer = "John Doe" },
           new(){ Id = 2, Customer = "Bob Smith" },
           new(){ Id = 3, Customer = "Jane Doe", EffectiveDate = DateTimeOffset.UtcNow.AddDays( 7d ) },
       } )
      .Produces<IEnumerable<OrderV2>>()
      .Produces( 404 )
      .HasApiVersion( 2.0 );

orders.MapGet( "/{id:int}", ( int id ) => new OrderV2() { Id = id, Customer = "John Doe" } )
      .Produces<OrderV2>()
      .Produces( 404 )
      .HasApiVersion( 2.0 );

orders.MapPost( "/", ( HttpRequest request, OrderV2 order ) =>
       {
           order.Id = 42;
           var scheme = request.Scheme;
           var host = request.Host;
           var location = new Uri( $"{scheme}{Uri.SchemeDelimiter}{host}/api/orders/{order.Id}" );
           return Results.Created( location, order );
       } )
      .Accepts<OrderV2>( "application/json" )
      .Produces<OrderV2>( 201 )
      .Produces( 400 )
      .HasApiVersion( 2.0 );

orders.MapMethods( "/{id:int}", new[] { HttpMethod.Patch.Method }, ( int id, OrderV2 order ) => Results.NoContent() )
      .Accepts<OrderV2>( "application/json" )
      .Produces( 204 )
      .Produces( 400 )
      .Produces( 404 )
      .HasApiVersion( 2.0 );

// 3.0
orders.MapGet( "/", () =>
       new OrderV3[]
       {
           new(){ Id = 1, Customer = "John Doe" },
           new(){ Id = 2, Customer = "Bob Smith" },
           new(){ Id = 3, Customer = "Jane Doe", EffectiveDate = DateTimeOffset.UtcNow.AddDays( 7d ) },
       } )
      .Produces<IEnumerable<OrderV3>>()
      .HasApiVersion( 3.0 );

orders.MapGet( "/{id:int}", ( int id ) => new OrderV3() { Id = id, Customer = "John Doe" } )
      .Produces<OrderV3>()
      .Produces( 404 )
      .HasApiVersion( 3.0 );

orders.MapPost( "/", ( HttpRequest request, OrderV3 order ) =>
       {
           order.Id = 42;
           var scheme = request.Scheme;
           var host = request.Host;
           var location = new Uri( $"{scheme}{Uri.SchemeDelimiter}{host}/api/orders/{order.Id}" );
           return Results.Created( location, order );
       } )
      .Accepts<OrderV3>( "application/json" )
      .Produces<OrderV3>( 201 )
      .Produces( 400 )
      .HasApiVersion( 3.0 );

orders.MapDelete( "/{id:int}", ( int id ) => Results.NoContent() )
      .Produces( 204 )
      .HasApiVersion( 3.0 );

// 1.0
people.MapGet( "/{id:int}", ( int id ) =>
       new PersonV1()
       {
           Id = id,
           FirstName = "John",
           LastName = "Doe",
       } )
      .Produces<PersonV1>()
      .Produces( 404 )
      .HasDeprecatedApiVersion( 0.9 )
      .HasApiVersion( 1.0 );

// 2.0
people.MapGet( "/", () =>
       new PersonV2[]
       {
           new()
           {
               Id = 1,
               FirstName = "John",
               LastName = "Doe",
               Email = "john.doe@somewhere.com",
           },
           new()
           {
               Id = 2,
               FirstName = "Bob",
               LastName = "Smith",
               Email = "bob.smith@somewhere.com",
           },
           new()
           {
               Id = 3,
               FirstName = "Jane",
               LastName = "Doe",
               Email = "jane.doe@somewhere.com",
           },
       } )
      .Produces<IEnumerable<PersonV2>>()
      .HasApiVersion( 2.0 );

people.MapGet( "/{id:int}", ( int id ) =>
       new PersonV2()
       {
           Id = id,
           FirstName = "John",
           LastName = "Doe",
           Email = "john.doe@somewhere.com",
       } )
      .Produces<PersonV2>()
      .Produces( 404 )
      .HasApiVersion( 2.0 );

// 3.0
people.MapGet( "/", () =>
       new PersonV3[]
       {
           new()
           {
               Id = 1,
               FirstName = "John",
               LastName = "Doe",
               Email = "john.doe@somewhere.com",
               Phone = "555-987-1234",
           },
           new()
           {
               Id = 2,
               FirstName = "Bob",
               LastName = "Smith",
               Email = "bob.smith@somewhere.com",
               Phone = "555-654-4321",
           },
           new()
           {
               Id = 3,
               FirstName = "Jane",
               LastName = "Doe",
               Email = "jane.doe@somewhere.com",
               Phone = "555-789-3456",
           },
       } )
      .Produces<IEnumerable<PersonV3>>()
      .HasApiVersion( 3.0 );

people.MapGet( "/{id:int}", ( int id ) =>
       new PersonV3()
       {
           Id = id,
           FirstName = "John",
           LastName = "Doe",
           Email = "john.doe@somewhere.com",
           Phone = "555-987-1234",
       } )
      .Produces<PersonV3>()
      .Produces( 404 )
      .HasApiVersion( 3.0 );

people.MapPost( "/", ( HttpRequest request, ApiVersion version, PersonV3 person ) =>
       {
           person.Id = 42;
           var scheme = request.Scheme;
           var host = request.Host;
           var location = new Uri( $"{scheme}{Uri.SchemeDelimiter}{host}/v{version}/api/people/{person.Id}" );
           return Results.Created( location, person );
       } )
      .Accepts<PersonV3>( "application/json" )
      .Produces<PersonV3>( 201 )
      .Produces( 400 )
      .HasApiVersion( 3.0 );

app.UseSwagger();
app.UseSwaggerUI(
    options =>
    {
        var descriptions = app.DescribeApiVersions();

        // build a swagger endpoint for each discovered API version
        foreach ( var description in descriptions )
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            options.SwaggerEndpoint( url, name );
        }
    } );

app.Run();