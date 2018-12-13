namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
    using Moq;
    using System.Threading.Tasks;
    using Xunit;

    public class ApiVersionModelBinderTest
    {
        [Fact]
        public async Task bind_model_should_set_api_version_as_result()
        {
            // arrange
            var apiVersion = new ApiVersion( 42, 0 );
            var bindingContext = NewModelBindingContext( apiVersion );
            var binder = new ApiVersionModelBinder();

            // act
            await binder.BindModelAsync( bindingContext );

            // assert
            bindingContext.Result.Model.Should().Be( apiVersion );
        }

        [Fact]
        public async Task bind_model_should_set_null_api_version_as_result()
        {
            // arrange
            var bindingContext = NewModelBindingContext( default );
            var binder = new ApiVersionModelBinder();

            // act
            await binder.BindModelAsync( bindingContext );

            // assert
            bindingContext.Result.IsModelSet.Should().BeTrue();
            bindingContext.Result.Model.Should().BeNull();
        }

        static HttpContext NewHttpContext( ApiVersion apiVersion )
        {
            var feature = new Mock<IApiVersioningFeature>();
            var featureCollection = new Mock<IFeatureCollection>();
            var httpContext = new Mock<HttpContext>();

            feature.SetupProperty( f => f.RequestedApiVersion, apiVersion );
            featureCollection.Setup( fc => fc.Get<IApiVersioningFeature>() ).Returns( feature.Object );
            httpContext.SetupGet( hc => hc.Features ).Returns( featureCollection.Object );

            return httpContext.Object;
        }

        static ModelBindingContext NewModelBindingContext( ApiVersion apiVersion )
        {
            var httpContext = NewHttpContext( apiVersion );
            var bindingContext = new Mock<ModelBindingContext>();

            bindingContext.SetupGet( bc => bc.HttpContext ).Returns( httpContext );
            bindingContext.SetupProperty( bc => bc.Result );
            bindingContext.SetupProperty( bc => bc.ValidationState, new ValidationStateDictionary() );

            return bindingContext.Object;
        }
    }
}