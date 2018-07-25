namespace Microsoft.AspNet.OData.Builder
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using ModelConfigurations;
    using System.Reflection;
    using Xunit;

    public class ModelConfigurationFeatureProviderTest
    {
        [Fact]
        public void populate_feature_should_discover_valid_model_configurations()
        {
            // arrange
            var part = new TestApplicationPart(
                typeof( ValueTypeModelConfiguration ),
                typeof( PrivateModelConfiguration ),
                typeof( AbstractModelConfiguration ),
                typeof( GenericModelConfiguration<> ),
                typeof( PublicModelConfiguration ) );
            var partManager = new ApplicationPartManager();
            var provider = new ModelConfigurationFeatureProvider();
            var feature = new ModelConfigurationFeature();

            partManager.ApplicationParts.Add( part );

            // act
            provider.PopulateFeature( partManager.ApplicationParts, feature );

            // assert
            feature.ModelConfigurations.Should().Equal( new[] { typeof( PublicModelConfiguration ).GetTypeInfo() } );
        }
    }

    namespace ModelConfigurations
    {
        struct ValueTypeModelConfiguration : IModelConfiguration
        {
            public void Apply( ODataModelBuilder builder, ApiVersion apiVersion ) { }
        }

        class PrivateModelConfiguration : IModelConfiguration
        {
            public void Apply( ODataModelBuilder builder, ApiVersion apiVersion ) { }
        }

        public abstract class AbstractModelConfiguration : IModelConfiguration
        {
            public void Apply( ODataModelBuilder builder, ApiVersion apiVersion ) { }
        }

        public class GenericModelConfiguration<T> : IModelConfiguration
        {
            public void Apply( ODataModelBuilder builder, ApiVersion apiVersion ) { }
        }

        public class PublicModelConfiguration : IModelConfiguration
        {
            public void Apply( ODataModelBuilder builder, ApiVersion apiVersion ) { }
        }
    }
}