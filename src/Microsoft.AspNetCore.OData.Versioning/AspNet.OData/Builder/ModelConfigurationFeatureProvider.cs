namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents an object that discovers model configurations from a collection of <see cref="ApplicationPart"/> instances.
    /// </summary>
    [CLSCompliant( false )]
    public class ModelConfigurationFeatureProvider : IApplicationFeatureProvider<ModelConfigurationFeature>
    {
        static readonly TypeInfo ModelConfiguration = typeof( IModelConfiguration ).GetTypeInfo();

        /// <inheritdoc />
        public void PopulateFeature( IEnumerable<ApplicationPart> parts, ModelConfigurationFeature feature )
        {
            if ( parts == null )
            {
                throw new ArgumentNullException( nameof( parts ) );
            }

            if ( feature == null )
            {
                throw new ArgumentNullException( nameof( feature ) );
            }

            foreach ( var part in parts.OfType<IApplicationPartTypeProvider>() )
            {
                foreach ( var type in part.Types )
                {
                    if ( IsModelConfiguration( type ) )
                    {
                        feature.ModelConfigurations.Add( type );
                    }
                }
            }
        }

        /// <summary>
        /// Determines if a given <paramref name="typeInfo"/> is a model configuration.
        /// </summary>
        /// <param name="typeInfo">The <see cref="TypeInfo"/> candidate.</param>
        /// <returns><c>True</c> if the type is a <see cref="IModelConfiguration">model configuration</see>; otherwise <c>false</c>.</returns>
        protected virtual bool IsModelConfiguration( TypeInfo typeInfo )
        {
            if ( typeInfo == null )
            {
                throw new ArgumentNullException( nameof( typeInfo ) );
            }

            if ( !typeInfo.IsClass || typeInfo.IsAbstract || !typeInfo.IsPublic || typeInfo.ContainsGenericParameters )
            {
                return false;
            }

            return ModelConfiguration.IsAssignableFrom( typeInfo );
        }
    }
}