namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Simulators;
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Xunit;
    using static Microsoft.AspNetCore.Mvc.ApiVersion;

    public class ODataApiVersionActionSelectorTest
    {
        [Theory]
        [InlineData( "1.0", typeof( TestsController ) )]
        [InlineData( "2.0", typeof( Tests2Controller ) )]
        [InlineData( "3.0", typeof( Tests3Controller ) )]
        public async Task select_best_candidate_should_return_correct_versionedX2C_attributeX2Dbased_controller( string version, Type controllerType )
        {
            // arrange
            using var server = new WebServer();

            // act
            await server.Client.GetAsync( $"api/tests?api-version={version}" );

            // assert
            var action = ( (TestODataApiVersionActionSelector) server.Services.GetRequiredService<IActionSelector>() ).SelectedCandidate;
            action.GetProperty<ApiVersionModel>().SupportedApiVersions.Should().Contain( Parse( version ) );
            action.As<ControllerActionDescriptor>().ControllerTypeInfo.Should().Be( controllerType.GetTypeInfo() );
        }

        [Theory]
        [InlineData( "http://localhost/api/NeutralTests" )]
        [InlineData( "http://localhost/api/NeutralTests?api-version=2.0" )]
        public async Task select_best_candidate_should_return_correct_versionX2DneutralX2C_attributeX2Dbased_controller( string requestUri )
        {
            // arrange
            var controllerType = typeof( VersionNeutralController ).GetTypeInfo();
            using var server = new WebServer();
            
            // act
            await server.Client.GetAsync( requestUri );

            // assert
            var action = ( (TestODataApiVersionActionSelector) server.Services.GetRequiredService<IActionSelector>() ).SelectedCandidate;
            action.As<ControllerActionDescriptor>().ControllerTypeInfo.Should().Be( controllerType );
        }
    }
}