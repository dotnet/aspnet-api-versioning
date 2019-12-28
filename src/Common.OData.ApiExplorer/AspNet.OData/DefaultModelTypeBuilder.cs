namespace Microsoft.AspNet.OData
{
#if WEBAPI
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http;
#else
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.OData.Edm;
#endif
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
#if WEBAPI
    using System.Web.Http.Dispatcher;
#endif
    using static System.Globalization.CultureInfo;
    using static System.Guid;
    using static System.Reflection.BindingFlags;
    using static System.Reflection.Emit.AssemblyBuilderAccess;

    /// <summary>
    /// Represents the default model type builder.
    /// </summary>
    public sealed class DefaultModelTypeBuilder : IModelTypeBuilder
    {
        static readonly Type IEnumerableOfT = typeof( IEnumerable<> );
        readonly ConcurrentDictionary<ApiVersion, ModuleBuilder> modules = new ConcurrentDictionary<ApiVersion, ModuleBuilder>();
        readonly ConcurrentDictionary<ApiVersion, IDictionary<EdmTypeKey, Type>> generatedEdmTypesPerVersion = new ConcurrentDictionary<ApiVersion, IDictionary<EdmTypeKey, Type>>();

        /// <inheritdoc />
        public Type NewStructuredType( IEdmStructuredType structuredType, Type clrType, ApiVersion apiVersion, IEdmModel edmModel )
        {
            var typeKey = new EdmTypeKey( structuredType, apiVersion );
            var edmTypes = generatedEdmTypesPerVersion.GetOrAdd( apiVersion, key => GenerateTypesForEdmModel( edmModel, apiVersion: key ) );

            return edmTypes[typeKey];
        }

        /// <inheritdoc />
        public Type NewActionParameters( IServiceProvider services, IEdmAction action, ApiVersion apiVersion, string controllerName )
        {
            if ( action == null )
            {
                throw new ArgumentNullException( nameof( action ) );
            }

            var name = controllerName + "." + action.FullName() + "Parameters";
            var properties = action.Parameters.Where( p => p.Name != "bindingParameter" ).Select( p => new ClassProperty( services, p, this ) );
            var signature = new ClassSignature( name, properties, apiVersion );
            var moduleBuilder = modules.GetOrAdd( apiVersion, CreateModuleForApiVersion );

            return CreateTypeInfoFromSignature( moduleBuilder, signature );
        }

        IDictionary<EdmTypeKey, Type> GenerateTypesForEdmModel( IEdmModel model, ApiVersion apiVersion )
        {
            ModuleBuilder NewModuleBuilder() => modules.GetOrAdd( apiVersion, CreateModuleForApiVersion );

            var context = new BuilderContext( model, apiVersion, NewModuleBuilder );

            foreach ( var structuredType in model.SchemaElements.OfType<IEdmStructuredType>() )
            {
                GenerateTypeIfNeeded( structuredType, context );
            }

            return ResolveDependencies( context );
        }

        static void MapEdmPropertiesToClrProperties(
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

        static Type GenerateTypeIfNeeded( IEdmStructuredType structuredType, BuilderContext context )
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

        static Tuple<bool, bool> BuildSignatureProperties(
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

            foreach ( var property in clrType.GetProperties( Public | Instance ) )
            {
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
                            propertyType = IEnumerableOfT.MakeGenericType( newItemType );
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

        static Type ResolveType(
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
                    edmTypes.Add( typeKey, type = clrType.GetTypeInfo() );
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
                edmTypes.Add( typeKey, type = CreateTypeInfoFromSignature( context.ModuleBuilder, signature ) );
            }

            return type;
        }

        static Type CreateTypeInfoFromSignature( ModuleBuilder moduleBuilder, ClassSignature @class ) => CreateTypeBuilderFromSignature( moduleBuilder, @class ).CreateType()!;

        static TypeBuilder CreateTypeBuilderFromSignature( ModuleBuilder moduleBuilder, ClassSignature @class )
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

        static IDictionary<EdmTypeKey, Type> ResolveDependencies( BuilderContext context )
        {
            var edmTypes = context.EdmTypes;
            var dependencies = context.Dependencies;

            for ( var i = 0; i < dependencies.Count; i++ )
            {
                var dependency = dependencies[i];
                var dependentOnType = edmTypes[dependency.DependentOnTypeKey];

                if ( dependency.IsCollection )
                {
                    dependentOnType = IEnumerableOfT.MakeGenericType( dependentOnType ).GetTypeInfo();
                }

                AddProperty( dependency.DependentType!, dependentOnType, dependency.PropertyName, dependency.CustomAttributes );
            }

            var keys = edmTypes.Keys.ToArray();

            for ( var i = 0; i < keys.Length; i++ )
            {
                var key = keys[i];

                if ( edmTypes[key] is TypeBuilder typeBuilder )
                {
                    edmTypes[key] = typeBuilder.CreateTypeInfo()!;
                }
            }

            return edmTypes;
        }

        static PropertyBuilder AddProperty( TypeBuilder addTo, Type shouldBeAdded, string name, IReadOnlyList<CustomAttributeBuilder> customAttributes )
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

        static ModuleBuilder CreateModuleForApiVersion( ApiVersion apiVersion )
        {
            var name = new AssemblyName( $"T{NewGuid().ToString( "n", InvariantCulture )}.DynamicModels" );
#if WEBAPI
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly( name, Run );
#else
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly( name, Run );
#endif
            return assemblyBuilder.DefineDynamicModule( "<module>" );
        }

        sealed class BuilderContext
        {
            readonly Lazy<ModuleBuilder> moduleBuilder;

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

            internal ISet<EdmTypeKey> VisitedEdmTypes { get; } = new HashSet<EdmTypeKey>();

            internal IList<PropertyDependency> Dependencies { get; } = new List<PropertyDependency>();
        }
    }
}