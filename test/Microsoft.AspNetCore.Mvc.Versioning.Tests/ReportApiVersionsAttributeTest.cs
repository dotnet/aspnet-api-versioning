namespace Microsoft.AspNetCore.Mvc
{
    using Abstractions;
    using AspNetCore.Routing;
    using Filters;
    using FluentAssertions;
    using Http;
    using Moq;
    using System.Linq;
    using Versioning;
    using Xunit;

    public class ReportApiVersionsAttributeTest
    {
        private static ActionExecutedContext CreateContext( ApiVersionModel model )
        {
            var headers = new HeaderDictionary();
            var response = new Mock<HttpResponse>();
            var httpContext = new Mock<HttpContext>();
            var action = new ActionDescriptor();
            var actionContext = new ActionContext( httpContext.Object, new RouteData(), action  );

            response.SetupGet( r => r.Headers ).Returns( headers );
            httpContext.SetupGet( c => c.Response ).Returns( response.Object );
            action.SetProperty( model );

            return new ActionExecutedContext( actionContext, new IFilterMetadata[0], null );
        }

        [Fact]
        public void on_action_executed_should_add_version_headers()
        {
            // arrange
            var supported = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ) };
            var deprecated = new[] { new ApiVersion( 0, 5 ) };
            var model = new ApiVersionModel( supported, deprecated );
            var context = CreateContext( model );
            var attribute = new ReportApiVersionsAttribute();

            // act
            attribute.OnActionExecuted( context );

            // assert
            context.HttpContext.Response.Headers["api-supported-versions"].Single().Should().Be( "1.0, 2.0" );
            context.HttpContext.Response.Headers["api-deprecated-versions"].Single().Should().Be( "0.5" );
        }

        [Fact]
        public void on_action_executing_should_not_add_headers_for_versionX2Dneutral_controller()
        {
            // arrange
            var context = CreateContext( ApiVersionModel.Neutral );
            var attribute = new ReportApiVersionsAttribute();

            // act
            attribute.OnActionExecuted( context );


            // assert
            context.HttpContext.Response.Headers.ContainsKey( "api-supported-versions" ).Should().BeFalse();
            context.HttpContext.Response.Headers.ContainsKey( "api-deprecated-versions" ).Should().BeFalse();
        }
    }
}
