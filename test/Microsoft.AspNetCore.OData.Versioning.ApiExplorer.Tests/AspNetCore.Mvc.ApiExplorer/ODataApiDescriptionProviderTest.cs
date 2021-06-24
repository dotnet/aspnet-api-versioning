namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using FluentAssertions;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq;
    using Xunit;
    using Xunit.Abstractions;

    public class ODataApiDescriptionProviderTest
    {
        readonly ITestOutputHelper Console;

        public ODataApiDescriptionProviderTest( ITestOutputHelper console ) => Console = console;

        [Fact]
        public void odata_api_explorer_should_group_and_order_descriptions_on_providers_executed()
        {
            // arrange
            var builder = new WebHostBuilder()
                .ConfigureServices(
                    services =>
                    {
                        services.AddMvcCore( options => options.EnableEndpointRouting = false );
                        services.AddApiVersioning( options => options.ReportApiVersions = true );
                        services.AddOData().EnableApiVersioning();
                        services.AddODataApiExplorer( options => options.GroupNameFormat = "'v'VVV" );
                    } )
                .Configure(
                    app =>
                    {
                        var modelBuilder = app.ApplicationServices.GetRequiredService<VersionedODataModelBuilder>();
                        app.UseMvc( rb => rb.MapVersionedODataRoute( "odata", "api", modelBuilder.GetEdmModels() ) );
                    } );
            var server = new TestServer( builder );
            var serviceProvider = server.Host.Services;

            // act
            var groups = serviceProvider.GetRequiredService<IApiDescriptionGroupCollectionProvider>()
                                        .ApiDescriptionGroups
                                        .Items
                                        .OrderBy( i => i.GroupName )
                                        .ToArray();

            // assert
            groups.Length.Should().Be( 4 );
            AssertVersion0_9( groups[0] );
            AssertVersion1( groups[1] );
            AssertVersion2( groups[2] );
            AssertVersion3( groups[3] );
        }

        void AssertVersion0_9( ApiDescriptionGroup group )
        {
            const string GroupName = "v0.9";

            PrintGroup( group );
            group.GroupName.Should().Be( GroupName );
            group.Items.Should().BeEquivalentTo(
                new[]
                {
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/GetSalesTaxRate(PostalCode={postalCode})" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders/{key}" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/People/{key}" },
                },
                options => options.ExcludingMissingMembers() );
        }

        void AssertVersion1( ApiDescriptionGroup group )
        {
            const string GroupName = "v1";

            PrintGroup( group );
            group.GroupName.Should().Be( GroupName );
            group.Items.Should().BeEquivalentTo(
                new[]
                {
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/GetSalesTaxRate(PostalCode={postalCode})" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders/{key}" },
                    new { HttpMethod = "POST", GroupName, RelativePath = "api/Orders" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders/MostExpensive" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/People/{key}" },
                },
                options => options.ExcludingMissingMembers() );
        }

        void AssertVersion2( ApiDescriptionGroup group )
        {
            const string GroupName = "v2";

            PrintGroup( group );
            group.GroupName.Should().Be( GroupName );
            group.Items.Should().BeEquivalentTo(
              new[]
              {
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/GetSalesTaxRate(PostalCode={postalCode})" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders/{key}" },
                    new { HttpMethod = "POST", GroupName, RelativePath = "api/Orders" },
                    new { HttpMethod = "PATCH", GroupName, RelativePath = "api/Orders/{key}" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders/MostExpensive" },
                    new { HttpMethod = "POST", GroupName, RelativePath = "api/Orders/{key}/Rate" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/People" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/People/{key}" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/People/NewHires(Since={since})" },
              },
              options => options.ExcludingMissingMembers() );
        }

        void AssertVersion3( ApiDescriptionGroup group )
        {
            const string GroupName = "v3";

            PrintGroup( group );
            group.GroupName.Should().Be( GroupName );
            group.Items.Should().BeEquivalentTo(
                new[]
                {
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/GetSalesTaxRate(PostalCode={postalCode})" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders/{key}" },
                    new { HttpMethod = "POST", GroupName, RelativePath = "api/Orders" },
                    new { HttpMethod = "PATCH", GroupName, RelativePath = "api/Orders/{key}" },
                    new { HttpMethod = "DELETE", GroupName, RelativePath = "api/Orders/{key}?suspendOnly={suspendOnly}" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/Orders/MostExpensive" },
                    new { HttpMethod = "POST", GroupName, RelativePath = "api/Orders/{key}/Rate" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/People" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/People/{key}" },
                    new { HttpMethod = "POST", GroupName, RelativePath = "api/People" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/People/NewHires(Since={since})" },
                    new { HttpMethod = "POST", GroupName, RelativePath = "api/People/{key}/Promote" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/Products" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/Products/{key}" },
                    new { HttpMethod = "POST", GroupName, RelativePath = "api/Products" },
                    new { HttpMethod = "PATCH", GroupName, RelativePath = "api/Products/{key}" },
                    new { HttpMethod = "PUT", GroupName, RelativePath = "api/Products/{key}" },
                    new { HttpMethod = "DELETE", GroupName, RelativePath = "api/Products/{key}" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/Products/{key}/Supplier" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/Products/{key}/supplier/$ref" },
                    new { HttpMethod = "POST", GroupName, RelativePath = "api/Products/{key}/supplier/$ref" },
                    new { HttpMethod = "DELETE", GroupName, RelativePath = "api/Products/{key}/supplier/$ref" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/Suppliers" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/Suppliers/{key}" },
                    new { HttpMethod = "POST", GroupName, RelativePath = "api/Suppliers" },
                    new { HttpMethod = "PATCH", GroupName, RelativePath = "api/Suppliers/{key}" },
                    new { HttpMethod = "PUT", GroupName, RelativePath = "api/Suppliers/{key}" },
                    new { HttpMethod = "DELETE", GroupName, RelativePath = "api/Suppliers/{key}" },
                    new { HttpMethod = "GET", GroupName, RelativePath = "api/Suppliers/{key}/Products" },
                    new { HttpMethod = "POST", GroupName, RelativePath = "api/Suppliers/{key}/Products/$ref" },
                    new { HttpMethod = "DELETE", GroupName, RelativePath = "api/Suppliers/{key}/Products/$ref?$id={$id}" },
                },
                options => options.ExcludingMissingMembers() );
        }

        void PrintGroup( ApiDescriptionGroup group )
        {
            var items = group.Items;

            for ( var i = items.Count - 1; i >= 0; i-- )
            {
                var item = items[i];
                Console.WriteLine( $"{item.HttpMethod} {item.RelativePath}" );
            }
        }
    }
}