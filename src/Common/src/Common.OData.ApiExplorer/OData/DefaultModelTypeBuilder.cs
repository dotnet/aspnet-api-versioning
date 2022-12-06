// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

#if NETFRAMEWORK
using Microsoft.AspNet.OData;
#endif
using Microsoft.OData.Edm;
#if !NETFRAMEWORK
using Microsoft.OData.ModelBuilder;
#endif
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Globalization.CultureInfo;
using static System.Guid;
using static System.Reflection.BindingFlags;
using static System.Reflection.Emit.AssemblyBuilderAccess;

/// <summary>
/// Represents the default model type builder.
/// </summary>
public sealed class DefaultModelTypeBuilder : IModelTypeBuilder
{
    private static Type? ienumerableOfT;
    private readonly bool adHoc;
    private DefaultModelTypeBuilder? adHocBuilder;
    private ConcurrentDictionary<ApiVersion, ModuleBuilder>? modules;
    private ConcurrentDictionary<ApiVersion, IDictionary<EdmTypeKey, Type>>? generatedEdmTypesPerVersion;
    private ConcurrentDictionary<ApiVersion, ConcurrentDictionary<EdmTypeKey, Type>>? generatedActionParamsPerVersion;

    private DefaultModelTypeBuilder( bool adHoc ) => this.adHoc = adHoc;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultModelTypeBuilder"/> class.
    /// </summary>
    public DefaultModelTypeBuilder() { }

    /// <inheritdoc />
    public Type NewStructuredType( IEdmModel model, IEdmStructuredType structuredType, Type clrType, ApiVersion apiVersion )
    {
        if ( model == null )
        {
            throw new ArgumentNullException( nameof( model ) );
        }

        if ( !adHoc && model.IsAdHoc() )
        {
            adHocBuilder ??= new( adHoc: true );
            return adHocBuilder.NewStructuredType( model, structuredType, clrType, apiVersion );
        }

        if ( structuredType == null )
        {
            throw new ArgumentNullException( nameof( structuredType ) );
        }

        if ( clrType == null )
        {
            throw new ArgumentNullException( nameof( clrType ) );
        }

        if ( apiVersion == null )
        {
            throw new ArgumentNullException( nameof( apiVersion ) );
        }

        generatedEdmTypesPerVersion ??= new();

        var edmTypes = generatedEdmTypesPerVersion.GetOrAdd( apiVersion, key => GenerateTypesForEdmModel( model, key ) );

        return edmTypes[new( structuredType, apiVersion )];
    }

    /// <inheritdoc />
    public Type NewActionParameters( IEdmModel model, IEdmAction action, string controllerName, ApiVersion apiVersion )
    {
        if ( model == null )
        {
            throw new ArgumentNullException( nameof( model ) );
        }

        if ( !adHoc && model.IsAdHoc() )
        {
            adHocBuilder ??= new( adHoc: true );
            return adHocBuilder.NewActionParameters( model, action, controllerName, apiVersion );
        }

        if ( action == null )
        {
            throw new ArgumentNullException( nameof( action ) );
        }

        if ( string.IsNullOrEmpty( controllerName ) )
        {
            throw new ArgumentNullException( nameof( controllerName ) );
        }

        if ( apiVersion == null )
        {
            throw new ArgumentNullException( nameof( apiVersion ) );
        }

        generatedActionParamsPerVersion ??= new();

        var paramTypes = generatedActionParamsPerVersion.GetOrAdd( apiVersion, _ => new() );
        var fullTypeName = $"{controllerName}.{action.Namespace}.{controllerName}{action.Name}Parameters";
        var key = new EdmTypeKey( fullTypeName, apiVersion );
        var type = paramTypes.GetOrAdd( key, _ =>
        {
            var context = new TypeSubstitutionContext( model, this, apiVersion );
            var properties = action.Parameters.Where( p => p.Name != "bindingParameter" ).Select( p => new ClassProperty( p, context ) );
            var signature = new ClassSignature( fullTypeName, properties, apiVersion );
            var moduleBuilder = ( modules ??= new() ).GetOrAdd( apiVersion, CreateModuleForApiVersion );

            return CreateTypeFromSignature( moduleBuilder, signature );
        } );

        return type;
    }

    private IDictionary<EdmTypeKey, Type> GenerateTypesForEdmModel( IEdmModel model, ApiVersion apiVersion )
    {
        ModuleBuilder NewModuleBuilder() => ( modules ??= new() ).GetOrAdd( apiVersion, CreateModuleForApiVersion );

        var context = new BuilderContext( model, apiVersion, NewModuleBuilder );

        foreach ( var structuredType in model.SchemaElements.OfType<IEdmStructuredType>() )
        {
            GenerateTypeIfNeeded( structuredType, context );
        }

        return ResolveDependencies( context );
    }

    private static void MapEdmPropertiesToClrProperties(
        IEdmModel edmModel,
        IEdmStructuredType edmType,
        Dictionary<string, IEdmProperty> structuralProperties,
        Dictionary<PropertyInfo, IEdmProperty> mappedClrProperties )
    {
        foreach ( var edmProperty in edmType.Properties() )
        {
            structuralProperties.Add( edmProperty.Name, edmProperty );

            var clrProperty = edmModel.GetAnnotationValue<ClrPropertyInfoAnnotation>( edmProperty )?.ClrPropertyInfo;

            if ( clrProperty != null )
            {
                mappedClrProperties.Add( clrProperty, edmProperty );
            }
        }
    }

    private static Type GenerateTypeIfNeeded( IEdmStructuredType structuredType, BuilderContext context )
    {
        var typeKey = new EdmTypeKey( structuredType, context.ApiVersion );

        if ( context.EdmTypes.TryGetValue( typeKey, out var generatedType ) )
        {
            return generatedType;
        }

        var clrType = structuredType.GetClrType( context.EdmModel )!;
        var visitedEdmTypes = context.VisitedEdmTypes;

        visitedEdmTypes.Add( typeKey );

        var properties = new List<ClassProperty>();
        var structuralProperties = new Dictionary<string, IEdmProperty>( StringComparer.OrdinalIgnoreCase );
        var mappedClrProperties = new Dictionary<PropertyInfo, IEdmProperty>();
        var dependentProperties = new List<PropertyDependency>();

        MapEdmPropertiesToClrProperties( context.EdmModel, structuredType, structuralProperties, mappedClrProperties );

        var (clrTypeMatchesEdmType, hasUnfinishedTypes) =
            BuildSignatureProperties(
                clrType,
                structuralProperties,
                mappedClrProperties,
                properties,
                dependentProperties,
                context );

        return ResolveType(
            typeKey,
            clrType,
            clrTypeMatchesEdmType,
            hasUnfinishedTypes,
            properties,
            dependentProperties,
            context );
    }

    private static Tuple<bool, bool> BuildSignatureProperties(
        Type clrType,
        IReadOnlyDictionary<string, IEdmProperty> structuralProperties,
        IReadOnlyDictionary<PropertyInfo, IEdmProperty> mappedClrProperties,
        List<ClassProperty> properties,
        List<PropertyDependency> dependentProperties,
        BuilderContext context )
    {
        var edmModel = context.EdmModel;
        var apiVersion = context.ApiVersion;
        var visitedEdmTypes = context.VisitedEdmTypes;
        var clrTypeMatchesEdmType = true;
        var hasUnfinishedTypes = false;
        var clrProperties = clrType.GetProperties( Public | Instance );

        for ( var i = 0; i < clrProperties.Length; i++ )
        {
            var property = clrProperties[i];

            if ( !structuralProperties.TryGetValue( property.Name, out var structuralProperty ) &&
                 !mappedClrProperties.TryGetValue( property, out structuralProperty ) )
            {
                clrTypeMatchesEdmType = false;
                continue;
            }

            var structuredTypeRef = structuralProperty.Type;
            var propertyType = property.PropertyType;
            var propertyTypeKey = new EdmTypeKey( structuredTypeRef, apiVersion );

            if ( structuredTypeRef.IsCollection() )
            {
                var collectionType = structuredTypeRef.AsCollection();
                var elementType = collectionType.ElementType();

                if ( elementType.IsStructured() )
                {
                    visitedEdmTypes.Add( propertyTypeKey );

                    var itemType = elementType.Definition.GetClrType( edmModel )!;
                    var elementKey = new EdmTypeKey( elementType, apiVersion );

                    if ( visitedEdmTypes.Contains( elementKey ) )
                    {
                        clrTypeMatchesEdmType = false;
                        hasUnfinishedTypes = true;
                        dependentProperties.Add( new PropertyDependency( elementKey, true, property.Name, property.DeclaredAttributes() ) );
                        continue;
                    }

                    var newItemType = GenerateTypeIfNeeded( elementType.ToStructuredType(), context );

                    if ( newItemType is TypeBuilder )
                    {
                        hasUnfinishedTypes = true;
                    }

                    if ( !itemType.Equals( newItemType ) )
                    {
                        propertyType = MakeEnumerable( newItemType );
                        clrTypeMatchesEdmType = false;
                    }
                }
            }
            else if ( structuredTypeRef.IsStructured() )
            {
                if ( !visitedEdmTypes.Contains( propertyTypeKey ) )
                {
                    propertyType = GenerateTypeIfNeeded( structuredTypeRef.ToStructuredType(), context );

                    if ( propertyType is TypeBuilder )
                    {
                        hasUnfinishedTypes = true;
                    }
                }
                else
                {
                    clrTypeMatchesEdmType = false;
                    hasUnfinishedTypes = true;
                    dependentProperties.Add( new PropertyDependency( propertyTypeKey, false, property.Name, property.DeclaredAttributes() ) );
                    continue;
                }
            }

            clrTypeMatchesEdmType &= property.PropertyType.Equals( propertyType );
            properties.Add( new ClassProperty( property, propertyType ) );
        }

        return Tuple.Create( clrTypeMatchesEdmType, hasUnfinishedTypes );
    }

    private static Type ResolveType(
        EdmTypeKey typeKey,
        Type clrType,
        bool clrTypeMatchesEdmType,
        bool hasUnfinishedTypes,
        List<ClassProperty> properties,
        List<PropertyDependency> dependentProperties,
        BuilderContext context )
    {
        var apiVersion = context.ApiVersion;
        var edmTypes = context.EdmTypes;

        Type? type;

        if ( clrTypeMatchesEdmType )
        {
            if ( !edmTypes.TryGetValue( typeKey, out type ) )
            {
                edmTypes.Add( typeKey, type = clrType );
            }

            return type;
        }

        var signature = new ClassSignature( clrType, properties, apiVersion );

        if ( hasUnfinishedTypes )
        {
            if ( edmTypes.TryGetValue( typeKey, out type ) )
            {
                return type;
            }

            var typeBuilder = CreateTypeBuilderFromSignature( context.ModuleBuilder, signature );
            var dependencies = context.Dependencies;

            for ( var i = 0; i < dependentProperties.Count; i++ )
            {
                var propertyDependency = dependentProperties[i];

                propertyDependency.DependentType = typeBuilder;
                dependencies.Add( propertyDependency );
            }

            edmTypes.Add( typeKey, typeBuilder );
            return typeBuilder;
        }

        if ( !edmTypes.TryGetValue( typeKey, out type ) )
        {
            edmTypes.Add( typeKey, type = CreateTypeFromSignature( context.ModuleBuilder, signature ) );
        }

        return type;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Type MakeEnumerable( Type itemType ) => ( ienumerableOfT ??= typeof( IEnumerable<> ) ).MakeGenericType( itemType );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Type CreateTypeFromSignature( ModuleBuilder moduleBuilder, ClassSignature @class ) =>
        CreateTypeBuilderFromSignature( moduleBuilder, @class ).CreateType()!;

    private static TypeBuilder CreateTypeBuilderFromSignature( ModuleBuilder moduleBuilder, ClassSignature @class )
    {
        var typeBuilder = moduleBuilder.DefineType( @class.Name, TypeAttributes.Class );
        var attributes = @class.Attributes;
        var properties = @class.Properties;

        for ( var i = 0; i < attributes.Count; i++ )
        {
            typeBuilder.SetCustomAttribute( attributes[i] );
        }

        for ( var i = 0; i < properties.Length; i++ )
        {
            ref var property = ref properties[i];
            var type = property.Type;
            var name = property.Name;

            AddProperty( typeBuilder, type, name, property.Attributes );
        }

        return typeBuilder;
    }

    private static IDictionary<EdmTypeKey, Type> ResolveDependencies( BuilderContext context )
    {
        var edmTypes = context.EdmTypes;

        if ( context.HasDependencies )
        {
            var dependencies = context.Dependencies;

            for ( var i = 0; i < dependencies.Count; i++ )
            {
                var dependency = dependencies[i];
                var dependentOnType = edmTypes[dependency.DependentOnTypeKey];

                if ( dependency.IsCollection )
                {
                    dependentOnType = MakeEnumerable( dependentOnType );
                }

                AddProperty( dependency.DependentType!, dependentOnType, dependency.PropertyName, dependency.CustomAttributes );
            }
        }

        var keys = edmTypes.Keys.ToArray();

        for ( var i = 0; i < keys.Length; i++ )
        {
            var key = keys[i];

            if ( edmTypes[key] is TypeBuilder typeBuilder )
            {
                edmTypes[key] = typeBuilder.CreateType()!;
            }
        }

        return edmTypes;
    }

    private static PropertyBuilder AddProperty(
        TypeBuilder addTo,
        Type shouldBeAdded,
        string name,
        IReadOnlyList<CustomAttributeBuilder> customAttributes )
    {
        const MethodAttributes propertyMethodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
        var field = addTo.DefineField( "_" + name, shouldBeAdded, FieldAttributes.Private );
        var propertyBuilder = addTo.DefineProperty( name, PropertyAttributes.HasDefault, shouldBeAdded, null );
        var getter = addTo.DefineMethod( "get_" + name, propertyMethodAttributes, shouldBeAdded, Type.EmptyTypes );
        var setter = addTo.DefineMethod( "set_" + name, propertyMethodAttributes, shouldBeAdded, Type.EmptyTypes );
        var il = getter.GetILGenerator();

        il.Emit( OpCodes.Ldarg_0 );
        il.Emit( OpCodes.Ldfld, field );
        il.Emit( OpCodes.Ret );

        il = setter.GetILGenerator();
        il.Emit( OpCodes.Ldarg_0 );
        il.Emit( OpCodes.Ldarg_1 );
        il.Emit( OpCodes.Stfld, field );
        il.Emit( OpCodes.Ret );
        propertyBuilder.SetGetMethod( getter );
        propertyBuilder.SetSetMethod( setter );

        for ( var i = 0; i < customAttributes.Count; i++ )
        {
            propertyBuilder.SetCustomAttribute( customAttributes[i] );
        }

        return propertyBuilder;
    }

    private static AssemblyName NewAssemblyName( ApiVersion apiVersion, bool adHoc )
    {
        // this is not strictly necessary, but it makes debugging a bit easier as each
        // assembly-qualified type name provides visibility as to which api version a
        // type and assembly correspond to
        var name = new StringBuilder();

        if ( apiVersion.GroupVersion.HasValue )
        {
            name.Append( apiVersion.GroupVersion.Value.ToString( "yyyyMMdd", InvariantCulture ) );
        }

        if ( apiVersion.MajorVersion.HasValue )
        {
            if ( name.Length > 0 )
            {
                name.Append( '_' );
            }

            name.Append( apiVersion.MajorVersion ).Append( '_' );

            if ( apiVersion.MinorVersion.HasValue )
            {
                name.Append( apiVersion.MinorVersion.Value );
            }
            else
            {
                name.Append( '0' );
            }
        }

        if ( name.Length > 0 )
        {
            name.Append( '_' );
        }

        name.Insert( 0, 'V' )
            .Append( NewGuid().ToString( "n", InvariantCulture ) );

        if ( adHoc )
        {
            name.Append( ".AdHoc" );
        }

        name.Append( ".DynamicModels" );

        return new( name.ToString() );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ModuleBuilder CreateModuleForApiVersion( ApiVersion apiVersion )
    {
        var assemblyName = NewAssemblyName( apiVersion, adHoc );
#if NETFRAMEWORK
        var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly( assemblyName, Run );
#else
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly( assemblyName, Run );
#endif
        return assemblyBuilder.DefineDynamicModule( "<module>" );
    }

    private sealed class BuilderContext
    {
        private readonly Lazy<ModuleBuilder> moduleBuilder;
        private HashSet<EdmTypeKey>? visitedEdmTypes;
        private List<PropertyDependency>? dependencies;

        internal BuilderContext( IEdmModel edmModel, ApiVersion apiVersion, Func<ModuleBuilder> moduleBuilderFactory )
        {
            EdmModel = edmModel;
            ApiVersion = apiVersion;
            moduleBuilder = new Lazy<ModuleBuilder>( moduleBuilderFactory );
        }

        internal ModuleBuilder ModuleBuilder => moduleBuilder.Value;

        internal ApiVersion ApiVersion { get; }

        internal IEdmModel EdmModel { get; }

        internal IDictionary<EdmTypeKey, Type> EdmTypes { get; } = new Dictionary<EdmTypeKey, Type>();

        internal ISet<EdmTypeKey> VisitedEdmTypes => visitedEdmTypes ??= new();

        internal IList<PropertyDependency> Dependencies => dependencies ??= new();

        internal bool HasDependencies => dependencies != null && dependencies.Count > 0;
    }
}