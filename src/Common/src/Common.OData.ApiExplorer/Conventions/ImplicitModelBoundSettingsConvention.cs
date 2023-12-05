// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Asp.Versioning;
using Asp.Versioning.OData;
#if NETFRAMEWORK
using Microsoft.AspNet.OData.Builder;
#else
using Microsoft.OData.ModelBuilder;
using System.Buffers;
#endif

/// <summary>
/// Represents an OData model bound settings <see cref="IModelConfiguration">model configuration</see>
/// that is also an <see cref="IODataQueryOptionsConvention">OData query options convention</see>.
/// </summary>
public sealed partial class ImplicitModelBoundSettingsConvention : IModelConfiguration, IODataQueryOptionsConvention
{
    private readonly HashSet<Type> types = [];

    /// <inheritdoc />
    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string? routePrefix )
    {
        ArgumentNullException.ThrowIfNull( builder );

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
        HashSet<Type> types;

        if ( builder.StructuralTypes is ICollection<StructuralTypeConfiguration> collection )
        {
            var count = collection.Count;

            if ( count == 0 )
            {
                return default;
            }

#if NETFRAMEWORK
            var array = new StructuralTypeConfiguration[count];
            types = [];
#else
            var pool = ArrayPool<StructuralTypeConfiguration>.Shared;
            var array = pool.Rent( count );

            types = new( capacity: count );
#endif

            collection.CopyTo( array, 0 );

            for ( var i = 0; i < count; i++ )
            {
                types.Add( array[i].ClrType );
            }

#if !NETFRAMEWORK
            pool.Return( array, clearArray: true );
#endif

            return types;
        }

        using var structuralTypes = builder.StructuralTypes.GetEnumerator();

        if ( !structuralTypes.MoveNext() )
        {
            return default;
        }

        types = [structuralTypes.Current.ClrType];

        while ( structuralTypes.MoveNext() )
        {
            types.Add( structuralTypes.Current.ClrType );
        }

        return types;
    }

    private void OnModelCreating( ODataModelBuilder builder )
    {
        foreach ( var type in types )
        {
            builder.AddComplexType( type );
        }
    }
}