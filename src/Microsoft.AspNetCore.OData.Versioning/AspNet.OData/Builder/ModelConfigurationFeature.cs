namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Represents the model configurations in an application.
    /// </summary>
    /// <remarks>The <see cref="ModelConfigurationFeature"/> can be populated using the <see cref="ApplicationPartManager"/>
    /// that is available during startup at <see cref="IMvcBuilder.PartManager"/> and <see cref="IMvcCoreBuilder.PartManager"/>
    /// or at a later stage by requiring the <see cref="ApplicationPartManager"/> as a dependency in a component.
    /// </remarks>
    [CLSCompliant( false )]
    public class ModelConfigurationFeature
    {
        /// <summary>
        /// Gets the collection of model configurations in an application.
        /// </summary>
        /// <value>The <see cref="ICollection{T}">collection</see> of model configurations in an application.</value>
        public ICollection<TypeInfo> ModelConfigurations { get; } = new HashSet<TypeInfo>();
    }
}