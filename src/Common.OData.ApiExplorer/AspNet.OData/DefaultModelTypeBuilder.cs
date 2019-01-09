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
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
#if WEBAPI
    using System.Web.Http.Dispatcher;
#endif
    using static System.Globalization.CultureInfo;
    using static System.Guid;
    using static System.Reflection.Emit.AssemblyBuilderAccess;

    /// <summary>
    /// Represents the default model type builder.
    /// </summary>
    public sealed class DefaultModelTypeBuilder : IModelTypeBuilder
    {
        static readonly Type IEnumerableOfT = typeof( IEnumerable<> );
        readonly ConcurrentDictionary<ApiVersion, ModuleBuilder> modules = new ConcurrentDictionary<ApiVersion, ModuleBuilder>();
        readonly ConcurrentDictionary<ApiVersion, ConcurrentDictionary<EdmTypeKey, TypeInfo>> generatedEdmTypesPerVersion = new ConcurrentDictionary<ApiVersion, ConcurrentDictionary<EdmTypeKey, TypeInfo>>();
        readonly HashSet<EdmTypeKey> visitedEdmTypes = new HashSet<EdmTypeKey>();
        readonly List<PropertyDependency> dependencies = new List<PropertyDependency>();

        /// <inheritdoc />
        public Type NewStructuredType( IEdmStructuredType structuredType, Type clrType, ApiVersion apiVersion, IEdmModel edmModel )
        {
            Arg.NotNull( structuredType, nameof( structuredType ) );
            Arg.NotNull( clrType, nameof( clrType ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Arg.NotNull( edmModel, nameof( edmModel ) );
            Contract.Ensures( Contract.Result<Type>() != null );

            var typeKey = new EdmTypeKey( structuredType, apiVersion );
            GenerateTypesForEdmModel( edmModel, apiVersion ).TryGetValue( typeKey, out var type );
            return type;
        }

        private Type GenerateTypeIfNeeded( IEdmStructuredType structuredType, ApiVersion apiVersion, IEdmModel edmModel, ConcurrentDictionary<EdmTypeKey, TypeInfo> edmTypes )
        {
            var clrType = structuredType.GetClrType(edmModel);
            var typeKey = new EdmTypeKey( structuredType, apiVersion );

            if ( edmTypes.TryGetValue( typeKey, out var generatedType ) )
            {
                return generatedType;
            }

            visitedEdmTypes.Add( typeKey );

            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;

            var properties = new List<ClassProperty>();
            var structuralProperties = structuredType.Properties().ToDictionary( p => p.Name, StringComparer.OrdinalIgnoreCase );
            var clrTypeMatchesEdmType = true;
            var hasUnfinishedTypes = false;
            var dependentProperties = new List<PropertyDependency>();

            foreach ( var property in clrType.GetProperties( bindingFlags ) )
            {
                if ( !structuralProperties.TryGetValue( property.Name, out var structuralProperty ) )
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

                        var itemType = elementType.Definition.GetClrType( edmModel );
                        var elementKey = new EdmTypeKey( elementType, apiVersion );

                        if ( visitedEdmTypes.Contains( elementKey ) )
                        {
                            clrTypeMatchesEdmType = false;
                            hasUnfinishedTypes = true;
                            var dependency = new PropertyDependency( elementKey, true, property.Name );
                            dependentProperties.Add( dependency );
                            continue;
                        }

                        var newItemType = GenerateTypeIfNeeded( elementType.ToStructuredType(), apiVersion, edmModel, edmTypes );

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
                        propertyType = GenerateTypeIfNeeded( structuredTypeRef.ToStructuredType(), apiVersion, edmModel, edmTypes );
                        if ( propertyType is TypeBuilder )
                        {
                            hasUnfinishedTypes = true;
                        }
                    }
                    else
                    {
                        clrTypeMatchesEdmType = false;
                        hasUnfinishedTypes = true;
                        var dependency = new PropertyDependency( propertyTypeKey, false, property.Name );
                        dependentProperties.Add( dependency );
                        continue;
                    }
                }

                clrTypeMatchesEdmType &= property.PropertyType.Equals( propertyType );
                properties.Add( new ClassProperty( property, propertyType ) );
            }

            if ( clrTypeMatchesEdmType )
            {
                return edmTypes.GetOrAdd( typeKey, clrType.GetTypeInfo() );
            }

            var signature = new ClassSignature( clrType, properties, apiVersion );

            if ( hasUnfinishedTypes )
            {
                if ( !edmTypes.TryGetValue( typeKey, out var type ) )
                {
                    TypeBuilder typeBuilder = CreateTypeBuilderFromSignature( signature );

                    foreach ( var propertyDependency in dependentProperties )
                    {
                        propertyDependency.DependentType = typeBuilder;
                        dependencies.Add( propertyDependency );
                    }

                    return edmTypes.GetOrAdd( typeKey, typeBuilder );
                }

                return type;
            }

            return edmTypes.GetOrAdd( typeKey, CreateTypeInfoFromSignature( signature ) );
        }

        /// <inheritdoc />
        public Type NewActionParameters( IServiceProvider services, IEdmAction action, ApiVersion apiVersion, string controllerName )
        {
            Arg.NotNull( services, nameof( services ) );
            Arg.NotNull( action, nameof( action ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Arg.NotNull( controllerName, nameof( controllerName ) );
            Contract.Ensures( Contract.Result<Type>() != null );

            var name = controllerName + "." + action.FullName() + "Parameters";
            var properties = action.Parameters.Where( p => p.Name != "bindingParameter" ).Select( p => new ClassProperty( services, p, this ) );
            var signature = new ClassSignature( name, properties, apiVersion );

            return CreateTypeInfoFromSignature( signature );
        }

        TypeInfo CreateTypeInfoFromSignature( ClassSignature @class )
        {
            Contract.Requires( @class != null );
            Contract.Ensures( Contract.Result<TypeInfo>() != null );

            return CreateTypeBuilderFromSignature( @class ).CreateTypeInfo();
        }

        TypeBuilder CreateTypeBuilderFromSignature( ClassSignature @class )
        {
            Contract.Requires( @class != null );
            Contract.Ensures( Contract.Result<TypeBuilder>() != null );

            var moduleBuilder = modules.GetOrAdd( @class.ApiVersion, CreateModuleForApiVersion );
            var typeBuilder = moduleBuilder.DefineType( @class.Name, TypeAttributes.Class );

            foreach ( var attribute in @class.Attributes )
            {
                typeBuilder.SetCustomAttribute( attribute );
            }

            foreach ( var property in @class.Properties )
            {
                var type = property.Type;
                var name = property.Name;
                var propertyBuilder = AddProperty( typeBuilder, type, name );

                foreach ( var attribute in property.Attributes )
                {
                    propertyBuilder.SetCustomAttribute( attribute );
                }
            }

            return typeBuilder;
        }

        private ConcurrentDictionary<EdmTypeKey, TypeInfo> GenerateTypesForEdmModel( IEdmModel model, ApiVersion apiVersion )
        {
            if ( generatedEdmTypesPerVersion.TryGetValue( apiVersion, out var edmTypes ) )
            {
                return edmTypes;
            }

            edmTypes = new ConcurrentDictionary<EdmTypeKey, TypeInfo>();
            foreach ( var element in model.SchemaElements )
            {
                if ( element is IEdmStructuredType structuredType )
                {
                    GenerateTypeIfNeeded( structuredType, apiVersion, model, edmTypes );
                }
            }

            var generatedEdmTypes = ResolveDependencies( edmTypes );
            return generatedEdmTypesPerVersion.GetOrAdd( apiVersion, generatedEdmTypes );
        }

        private ConcurrentDictionary<EdmTypeKey, TypeInfo> ResolveDependencies( ConcurrentDictionary<EdmTypeKey, TypeInfo> edmTypes )
        {
            for ( var i = dependencies.Count - 1; i >= 0; i-- )
            {
                var propertyDependency = dependencies[i];
                edmTypes.TryGetValue( propertyDependency.DependentOnTypeKey, out var dependentOnType );

                if ( propertyDependency.IsCollection )
                {
                    dependentOnType = IEnumerableOfT.MakeGenericType( dependentOnType ).GetTypeInfo();
                }

                AddProperty( propertyDependency.DependentType, dependentOnType, propertyDependency.PropertyName );
                dependencies.RemoveAt( i );
            }

            var generatedEdmTypes = new ConcurrentDictionary<EdmTypeKey, TypeInfo>();
            foreach ( var typePair in edmTypes )
            {
                var type = typePair.Value;
                if ( type is TypeBuilder typeBuilder )
                {
                    type = typeBuilder.CreateTypeInfo();
                }

                generatedEdmTypes.GetOrAdd( typePair.Key, type );
            }

            return generatedEdmTypes;
        }

        static PropertyBuilder AddProperty( TypeBuilder addTo, Type shouldBeAdded, string name )
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
    }
}