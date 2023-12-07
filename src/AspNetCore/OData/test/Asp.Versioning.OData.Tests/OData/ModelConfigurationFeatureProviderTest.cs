// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData
{
    using Asp.Versioning.OData.ModelConfigurations;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.OData.ModelBuilder;

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
            feature.ModelConfigurations.Should().Equal( [typeof( PublicModelConfiguration )] );
        }
    }

#pragma warning disable IDE0079
#pragma warning disable CA1812
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1403 // File may only contain a single namespace

    namespace ModelConfigurations
    {
        internal struct ValueTypeModelConfiguration : IModelConfiguration
        {
            public readonly void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix ) { }
        }

        internal sealed class PrivateModelConfiguration : IModelConfiguration
        {
            public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix ) { }
        }

        public abstract class AbstractModelConfiguration : IModelConfiguration
        {
            public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix ) { }
        }

        public sealed class GenericModelConfiguration<T> : IModelConfiguration
        {
            public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix ) { }
        }

        public sealed class PublicModelConfiguration : IModelConfiguration
        {
            public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix ) { }
        }
    }
}