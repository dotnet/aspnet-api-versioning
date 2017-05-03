namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Options;
    using Moq;
    using System.Collections.Generic;
    using Xunit;

    public class VersionedApiDescriptionProviderTest
    {
        [Fact]
        public void versioned_api_explorer_should_group_and_order_descriptions_on_providers_executed()
        {
            // arrange
            var actionProvider = new TestActionDescriptorCollectionProvider();
            var context = new ApiDescriptionProviderContext( actionProvider.ActionDescriptors.Items );
            var modelMetadataProvider = NewModelMetadataProvider();
            var apiExplorerOptions = new OptionsWrapper<ApiExplorerOptions>( new ApiExplorerOptions() { GroupNameFormat = "'v'VVV" } );
            var apiExplorer = new VersionedApiDescriptionProvider( modelMetadataProvider, apiExplorerOptions );

            foreach ( var action in context.Actions )
            {
                context.Results.Add( new ApiDescription() { ActionDescriptor = action } );
            }

            // act
            apiExplorer.OnProvidersExecuted( context );

            // assert
            context.Results.ShouldBeEquivalentTo(
                new[]
                {
                    // orders
                    new { GroupName = "v0.9", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 0, 9 ) } },
                    new { GroupName = "v1", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 1, 0 ) } },
                    new { GroupName = "v1", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 1, 0 ) } },
                    new { GroupName = "v2", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 2, 0 ) } },
                    new { GroupName = "v2", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 2, 0 ) } },
                    new { GroupName = "v2", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 2, 0 ) } },
                    new { GroupName = "v3", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 3, 0 ) } },
                    new { GroupName = "v3", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 3, 0 ) } },
                    new { GroupName = "v3", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 3, 0 ) } },
                    new { GroupName = "v3", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 3, 0 ) } },

                    // people
                    new { GroupName = "v0.9", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 0, 9 ) } },
                    new { GroupName = "v1", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 1, 0 ) } },
                    new { GroupName = "v1", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 1, 0 ) } },
                    new { GroupName = "v2", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 2, 0 ) } },
                    new { GroupName = "v2", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 2, 0 ) } },
                    new { GroupName = "v2", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 2, 0 ) } },
                    new { GroupName = "v3", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 3, 0 ) } },
                    new { GroupName = "v3", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 3, 0 ) } },
                    new { GroupName = "v3", Properties = new Dictionary<object,object>() { [typeof( ApiVersion )] = new ApiVersion( 3, 0 ) } },
                },
                options => options.ExcludingMissingMembers() );
        }

        static IModelMetadataProvider NewModelMetadataProvider()
        {
            var provider = new Mock<IModelMetadataProvider>();
            var identity = ModelMetadataIdentity.ForType( typeof( string ) );
            var metadata = new Mock<ModelMetadata>( identity ) { CallBase = true };

            provider.Setup( p => p.GetMetadataForType( typeof( string ) ) ).Returns( metadata.Object );

            return provider.Object;
        }
    }
}