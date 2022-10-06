using ApiVersioning.Examples;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OData.ModelBuilder;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;
using static System.Text.Json.JsonNamingPolicy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// note: this example application intentionally only illustrates the
// bare minimum configuration for OpenAPI with partial OData support.
// see the OpenAPI or OData OpenAPI examples for additional options.

builder.Services.Configure<JsonOptions>(
    options =>
    {
        // odata projection operations (ex: $select) use a dictionary, but for good
        // measure set the default property naming policy for any other use cases
        options.JsonSerializerOptions.PropertyNamingPolicy = CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = CamelCase;
    });

builder.Services
    .AddControllers()
    .AddOData((options, serviceProvider) =>
    {
        options.SetMaxTop(null).Select().Expand().Filter().OrderBy().Count();
    });
builder.Services.AddApiVersioning((options) =>
{
    options.ReportApiVersions = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
})
            .AddOData(options =>
            {
                options.AddRouteComponents("v{version:apiVersion}");
               // options.ModelBuilder.ModelBuilderFactory = () => new ODataConventionModelBuilder();
            })
            .AddODataApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VV";
                options.SubstituteApiVersionInUrl = true;
            });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(
    options =>
    {
        // add a custom operation filter which sets default values
        options.OperationFilter<SwaggerDefaultValues>();

        var fileName = typeof(Program).Assembly.GetName().Name + ".xml";
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

        // integrate xml comments
        options.IncludeXmlComments(filePath);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // navigate to ~/$odata to determine whether any endpoints did not match an odata route template
    app.UseODataRouteDebug();
}

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(
    options =>
    {
        var descriptions = app.DescribeApiVersions();

        // build a swagger endpoint for each discovered API version
        foreach (var description in descriptions)
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            options.SwaggerEndpoint(url, name);
        }
    });

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();