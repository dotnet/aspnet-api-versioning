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

    [Fact]
    public void apply_to_should_not_throw_when_action_has_only_introduced_version()
    {
        // arrange
        var action = default( ActionModel );

        // act
        var exception = Record.Exception( () => action = ApplyConventions( typeof( IntroducedController ), nameof( IntroducedController.GetIntroduced ) ) );
        var metadata = GetApiVersionMetadata( action! );
        var model = metadata.Map( Explicit );

        // assert
        exception.Should().BeNull();
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

    [Fact]
    public void apply_to_should_use_latest_introduced_version_when_multiple_are_declared()
    {
        // arrange
        var action = ApplyConventions( typeof( MultipleIntroducedController ), nameof( MultipleIntroducedController.Get ) );
        var metadata = GetApiVersionMetadata( action );

        // act
        var model = metadata.Map( Explicit );
        var introduced = action.Selectors.Single().EndpointMetadata.OfType<IntroducedInApiVersionMetadata>().ToArray();

        // assert
        model.DeclaredApiVersions.Should().Equal( new ApiVersion( 3, 0 ) );
        introduced.Select( item => item.IntroducedIn ).Should().Equal( new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) );
    }

    [Fact]
    public void apply_to_should_ignore_introduced_version_metadata_for_version_neutral_controller()
    {
        // arrange
        var action = ApplyConventions( typeof( NeutralIntroducedController ), nameof( NeutralIntroducedController.Get ) );

        // act
        var metadata = GetApiVersionMetadata( action );
        var introduced = action.Selectors.Single().EndpointMetadata.OfType<IntroducedInApiVersionMetadata>();

        // assert
        metadata.IsApiVersionNeutral.Should().BeTrue();
        metadata.IntroducedInApiVersions.Should().BeEmpty();
        introduced.Should().BeEmpty();
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


    [ApiController]
    [ApiVersion( 1.0 )]
    [ApiVersion( 2.0 )]
    [ApiVersion( 3.0 )]
    public sealed class MultipleIntroducedController : ControllerBase
    {
        [TestIntroducedInApiVersion( 2.0, StatusCode = 409 )]
        [TestIntroducedInApiVersion( 3.0, StatusCode = 410 )]
        public OkResult Get() => Ok();
    }

    [ApiController]
    [ApiVersionNeutral]
    public sealed class NeutralIntroducedController : ControllerBase
    {
        [IntroducedInApiVersion( "2.0" )]
        public OkResult Get() => Ok();
    }

    [AttributeUsage( AttributeTargets.Method, AllowMultiple = true, Inherited = false )]
    public sealed class TestIntroducedInApiVersionAttribute : Attribute, IIntroducedInApiVersionProvider
    {
        public TestIntroducedInApiVersionAttribute( double version )
        {
            Version = version;
            Versions = [new ApiVersion( version )];
        }

        public double Version { get; }

        public IReadOnlyList<ApiVersion> Versions { get; }

        public int StatusCode { get; set; } = IntroducedInApiVersionAttribute.DefaultStatusCode;

        ApiVersionProviderOptions IApiVersionProvider.Options => ApiVersionProviderOptions.Introduced;
    }

#pragma warning restore CA1034 // Nested types should not be visible
}