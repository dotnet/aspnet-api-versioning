// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Asp.Versioning;
using Asp.Versioning.OData;
#if NETFRAMEWORK
using Microsoft.AspNet.OData.Builder;
#else
using Microsoft.OData.ModelBuilder;
#endif

/// <summary>
/// Represents an OData model bound settings <see cref="IModelConfiguration">model configuration</see>
/// that is also an <see cref="IODataQueryOptionsConvention">OData query options convention</see>.
/// </summary>
public sealed partial class ImplicitModelBoundSettingsConvention : IModelConfiguration, IODataQueryOptionsConvention
{
    private readonly HashSet<Type> types = new();

    /// <inheritdoc />
    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string? routePrefix )
    {
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        if ( types.Count == 0 )
        {
            return;
        }

        if ( GetExistingTypes( builder ) is HashSet<Type> existingTypes )
        {
            types.ExceptWith( existingTypes );
        }

        if ( types.Count == 0 )
        {
            return;
        }

        // model configurations are applied unordered, which could matter.
        // defer implicit registrations in the model until all other model
        // configurations have been applied, if possible
        if ( builder is ODataConventionModelBuilder modelBuilder )
        {
            modelBuilder.OnModelCreating += OnModelCreating;
        }
        else
        {
            OnModelCreating( builder );
        }
    }

    private static HashSet<Type>? GetExistingTypes( ODataModelBuilder builder )
    {
        var types = default( HashSet<Type> );

        foreach ( var entitySet in builder.EntitySets )
        {
            types ??= new();
            types.Add( entitySet.ClrType );
        }

        foreach ( var singleton in builder.Singletons )
        {
            types ??= new();
            types.Add( singleton.ClrType );
        }

        return types;
    }

    private void OnModelCreating( ODataModelBuilder builder )
    {
        foreach ( var entityType in types.Select( builder.AddEntityType ) )
        {
            builder.AddEntitySet( entityType.Name, entityType );
        }
    }
}