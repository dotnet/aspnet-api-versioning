namespace Microsoft.AspNet.OData
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    /// <summary>
    /// Represents a dependency on a type of a property that has not been built yet.
    /// </summary>
    internal class PropertyDependency
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDependency"/> class.
        /// </summary>
        /// <param name="dependentOnTypeKey">The key of the type the property has a dependency on.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="isCollection">Whether the property is a collection or not.</param>
        /// <param name="customAttributes">A collection of custom attribute builders.</param>
        internal PropertyDependency( EdmTypeKey dependentOnTypeKey,  bool isCollection, string propertyName, IEnumerable<CustomAttributeBuilder> customAttributes )
        {
            Arg.NotNull<EdmTypeKey>( dependentOnTypeKey, nameof( dependentOnTypeKey ) );
            Arg.NotNull<string>( propertyName, nameof( propertyName ) );

            DependentOnTypeKey = dependentOnTypeKey;
            PropertyName = propertyName;
            CustomAttributes = customAttributes;
            IsCollection = isCollection;
        }

        /// <summary>
        /// Gets or sets the type that has a dependent property.
        /// </summary>
        internal TypeBuilder DependentType { get; set;  }

        /// <summary>
        /// Gets the key of the type the property has a dependency on.
        /// </summary>
        internal EdmTypeKey DependentOnTypeKey { get; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        internal string PropertyName { get; }

        /// <summary>
        /// Gets a value indicating whether the property is a collection.
        /// </summary>
        internal bool IsCollection { get; }

        /// <summary>
        /// Gets custom attribute builders of the property.
        /// </summary>
        internal IEnumerable<CustomAttributeBuilder> CustomAttributes { get; }
    }
}