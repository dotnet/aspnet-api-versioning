// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;
using static Asp.Versioning.ApiVersionMapping;

public class IntroducedInApiVersionConventionTest
{
    [Fact]
    public void apply_to_should_expand_introduced_version_to_controller_declared_versions()
    {
        // arrange
        var action = ApplyConventions( typeof( IntroducedController ), nameof( IntroducedController.GetIntroduced ) );
        var metadata = GetApiVersionMetadata( action );

        // act
        var model = metadata.Map( Explicit );

        // assert
        model.DeclaredApiVersions.Should().Equal(
            new ApiVersion( new DateOnly( 2026, 12, 1 ) ),
            new ApiVersion( new DateOnly( 2027, 6, 1 ) ) );
    }

    [Theory]
    [InlineData( "2026-11-12", false )]
    [InlineData( "2026-12-01", true )]
    [InlineData( "2027-06-01", true )]
    public void mapping_should_include_only_versions_on_or_after_introduced_version( string value, bool expected )
    {
        // arrange
        var action = ApplyConventions( typeof( IntroducedController ), nameof( IntroducedController.GetIntroduced ) );
        var metadata = GetApiVersionMetadata( action );
        var apiVersion = ApiVersionParser.Default.Parse( value );

        // act
        var mapped = metadata.IsMappedTo( apiVersion );

        // assert
        mapped.Should().Be( expected );
    }

    [Fact]
    public void apply_to_should_add_status_code_metadata_for_introduced_version()
    {
        // arrange
        var action = ApplyConventions( typeof( IntroducedController ), nameof( IntroducedController.GetIntroduced ) );

        // act
        var metadata = action.Selectors.Single().EndpointMetadata.OfType<IntroducedInApiVersionMetadata>().Single();

        // assert
        metadata.IntroducedIn.Should().Be( new ApiVersion( new DateOnly( 2026, 12, 1 ) ) );
        metadata.StatusCode.Should().Be( IntroducedInApiVersionAttribute.DefaultStatusCode );
    }

    [Fact]
    public void map_to_api_version_should_retain_exact_match_semantics()
    {
        // arrange
        var action = ApplyConventions( typeof( IntroducedController ), nameof( IntroducedController.GetMapped ) );
        var metadata = GetApiVersionMetadata( action );

        // act
        var model = metadata.Map( Explicit );

        // assert
        model.DeclaredApiVersions.Should().Equal( new ApiVersion( new DateOnly( 2026, 12, 1 ) ) );
        metadata.IsMappedTo( ApiVersionParser.Default.Parse( "2027-06-01" ) ).Should().BeFalse();
    }

    [Fact]
    public void api_explorer_mapping_should_exclude_versions_before_introduced_version()
    {
        // arrange
        var action = ApplyConventions( typeof( IntroducedController ), nameof( IntroducedController.GetIntroduced ) );
        var metadata = GetApiVersionMetadata( action );

        // act
        var before = metadata.MappingTo( ApiVersionParser.Default.Parse( "2026-11-12" ) );
        var introduced = metadata.MappingTo( ApiVersionParser.Default.Parse( "2026-12-01" ) );
        var later = metadata.MappingTo( ApiVersionParser.Default.Parse( "2027-06-01" ) );

        // assert
        before.Should().Be( None );
        introduced.Should().Be( Explicit );
        later.Should().Be( Explicit );
    }

    private static ActionModel ApplyConventions( Type controllerType, string actionName )
    {
        var controllerAttributes = controllerType.GetTypeInfo().GetCustomAttributes().Cast<object>().ToArray();
        var controller = new ControllerModel( controllerType.GetTypeInfo(), controllerAttributes )
        {
            ControllerName = controllerType.Name.Replace( "Controller", string.Empty, StringComparison.Ordinal ),
        };

        foreach ( var method in controllerType.GetRuntimeMethods().Where( method => method.DeclaringType == controllerType && method.IsPublic ) )
        {
            var action = new ActionModel( method, method.GetCustomAttributes().Cast<object>().ToArray() )
            {
                Controller = controller,
            };

            controller.Actions.Add( action );
        }

        var builder = new ControllerApiVersionConventionBuilder( controllerType );
        builder.ApplyTo( controller );
        return controller.Actions.Single( action => action.ActionMethod.Name == actionName );
    }

    private static ApiVersionMetadata GetApiVersionMetadata( ActionModel action ) =>
        action.Selectors.Single().EndpointMetadata.OfType<ApiVersionMetadata>().Single();

#pragma warning disable CA1034 // Nested types should not be visible

    [ApiController]
    [ApiVersion( 2026, 11, 12 )]
    [ApiVersion( 2026, 12, 1 )]
    [ApiVersion( 2027, 6, 1 )]
    public sealed class IntroducedController : ControllerBase
    {
        [IntroducedInApiVersion( "2026-12-01" )]
        public OkResult GetIntroduced() => Ok();

        [MapToApiVersion( "2026-12-01" )]
        public OkResult GetMapped() => Ok();
    }

#pragma warning restore CA1034 // Nested types should not be visible
}