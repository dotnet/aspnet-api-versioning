namespace Microsoft.AspNetCore.Mvc
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class ReportApiVersionsAttributeTest
    {
        [Fact]
        public async Task on_action_executing_should_add_version_headers()
        {
            // arrange
            var supported = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ) };
            var deprecated = new[] { new ApiVersion( 0, 5 ) };
            var model = new ApiVersionModel( supported, deprecated );
            var onStartResponse = new List<(Func<object, Task>, object)>();
            var context = CreateContext( model, onStartResponse );
            var attribute = new ReportApiVersionsAttribute();

            // act
            attribute.OnActionExecuting( context );

            for ( var i = 0; i < onStartResponse.Count; i++ )
            {
                var (callback, state) = onStartResponse[i];
                await callback( state );
            }

            // assert
            var headers = context.HttpContext.Response.Headers;

            headers["api-supported-versions"].Single().Should().Be( "1.0, 2.0" );
            headers["api-deprecated-versions"].Single().Should().Be( "0.5" );
        }

        [Fact]
        public async Task on_action_executing_should_not_add_headers_for_versionX2Dneutral_controller()
        {
            // arrange
            var onStartResponse = new List<(Func<object, Task>, object)>();
            var context = CreateContext( ApiVersionModel.Neutral, onStartResponse );
            var attribute = new ReportApiVersionsAttribute();

            // act
            attribute.OnActionExecuting( context );

            for ( var i = 0; i < onStartResponse.Count; i++ )
            {
                var (callback, state) = onStartResponse[i];
                await callback( state );
            }

            // assert
            var headers = context.HttpContext.Response.Headers;

            headers.ContainsKey( "api-supported-versions" ).Should().BeFalse();
            headers.ContainsKey( "api-deprecated-versions" ).Should().BeFalse();
        }

        static ActionExecutingContext CreateContext( ApiVersionModel model, ICollection<(Func<object, Task>, object)> onStartResponse )
        {
            var headers = new HeaderDictionary();
            var response = new Mock<HttpResponse>();
            var httpContext = new Mock<HttpContext>();
            var action = new ActionDescriptor();
            var actionContext = new ActionContext( httpContext.Object, new RouteData(), action );
            var filters = Array.Empty<IFilterMetadata>();
            var actionArguments = new Dictionary<string, object>();
            var controller = default( object );

            response.SetupGet( r => r.Headers ).Returns( headers );
            response.Setup( r => r.OnStarting( It.IsAny<Func<object, Task>>(), It.IsAny<object>() ) )
                    .Callback( ( Func<object, Task> callback, object state ) => onStartResponse.Add( (callback, state) ) );
            httpContext.SetupGet( c => c.Response ).Returns( response.Object );
            action.SetProperty( model );

            return new ActionExecutingContext( actionContext, filters, actionArguments, controller );
        }
    }
}