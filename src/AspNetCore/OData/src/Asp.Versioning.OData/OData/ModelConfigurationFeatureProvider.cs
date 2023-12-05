// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.AspNetCore.Mvc.ApplicationParts;

/// <summary>
/// Represents an object that discovers model configurations from a collection of <see cref="ApplicationPart"/> instances.
/// </summary>
[CLSCompliant( false )]
public class ModelConfigurationFeatureProvider : IApplicationFeatureProvider<ModelConfigurationFeature>
{
    private static Type? modelConfiguration;

    /// <inheritdoc />
    public void PopulateFeature( IEnumerable<ApplicationPart> parts, ModelConfigurationFeature feature )
    {
        ArgumentNullException.ThrowIfNull( parts );
        ArgumentNullException.ThrowIfNull( feature );

        var types = from part in parts.OfType<IApplicationPartTypeProvider>()
                    from type in part.Types
                    where IsModelConfiguration( type )
                    select type;

        foreach ( var type in types )
        {
            feature.ModelConfigurations.Add( type );
        }
    }

    /// <summary>
    /// Determines if a given <paramref name="type"/> is a model configuration.
    /// </summary>
    /// <param name="type">The <see cref="Type">type</see> candidate.</param>
    /// <returns><c>True</c> if the type is a <see cref="IModelConfiguration">model configuration</see>; otherwise <c>false</c>.</returns>
    protected virtual bool IsModelConfiguration( Type type )
    {
        ArgumentNullException.ThrowIfNull( type );

        if ( !type.IsClass || type.IsAbstract || !type.IsPublic || type.ContainsGenericParameters )
        {
            return false;
        }

        modelConfiguration ??= typeof( IModelConfiguration );
        return modelConfiguration.IsAssignableFrom( type );
    }
}