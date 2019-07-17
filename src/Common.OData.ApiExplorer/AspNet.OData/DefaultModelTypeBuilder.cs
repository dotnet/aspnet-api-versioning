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
        readonly ConcurrentDictionary<ApiVersion, IDictionary<EdmTypeKey, TypeInfo>> generatedEdmTypesPerVersion = new ConcurrentDictionary<ApiVersion, IDictionary<EdmTypeKey, TypeInfo>>();

        /// <inheritdoc />
        public Type NewStructuredType( IEdmStructuredType structuredType, Type clrType, ApiVersion apiVersion, IEdmModel edmModel )
        {
            Arg.NotNull( structuredType, nameof( structuredType ) );
            Arg.NotNull( clrType, nameof( clrType ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Arg.NotNull( edmModel, nameof( edmModel ) );
            Contract.Ensures( Contract.Result<Type>() != null );

            var typeKey = new EdmTypeKey( structuredType, apiVersion );
            var edmTypes = generatedEdmTypesPerVersion.GetOrAdd( apiVersion, key => GenerateTypesForEdmModel( edmModel, apiVersion: key ) );

            return edmTypes[typeKey];
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
            var moduleBuilder = modules.GetOrAdd( apiVersion, CreateModuleForApiVersion );

            return CreateTypeInfoFromSignature( moduleBuilder, signature );
        }

        IDictionary<EdmTypeKey, TypeInfo> GenerateTypesForEdmModel( IEdmModel model, ApiVersion apiVersion )
        {
            ModuleBuilder NewModuleBuilder() => modules.GetOrAdd( apiVersion, CreateModuleForApiVersion );

            var context = new BuilderContext( model, apiVersion, NewModuleBuilder );

            foreach ( var structuredType in model.SchemaElements.OfType<IEdmStructuredType>() )
            {
                GenerateTypeIfNeeded( structuredType, context );
            }

            return ResolveDependencies( context );
        }

        static Type GenerateTypeIfNeeded( IEdmStructuredType structuredType, BuilderContext context )
        {
            var apiVersion = context.ApiVersion;
            var edmTypes = context.EdmTypes;
            var typeKey = new EdmTypeKey( structuredType, apiVersion );

            if ( edmTypes.TryGetValue( typeKey, out var generatedType ) )
            {
                return generatedType;
            }

            var edmModel = context.EdmModel;
            var clrType = structuredType.GetClrType( edmModel );
            var visitedEdmTypes = context.VisitedEdmTypes;

            visitedEdmTypes.Add( typeKey );

            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;

            var properties = new List<ClassProperty>();
            var structuralProperties = new Dictionary<string, IEdmProperty>( StringComparer.OrdinalIgnoreCase );
            var mappedClrProperties = new Dictionary<PropertyInfo, IEdmProperty>();
            var clrTypeMatchesEdmType = true;
            var hasUnfinishedTypes = false;
            var dependentProperties = new List<PropertyDependency>();

            foreach ( var property in structuredType.Properties() )
            {
                structuralProperties.Add( property.Name, property );
                var clrProperty = edmModel.GetAnnotationValue<ClrPropertyInfoAnnotation>( property )?.ClrPropertyInfo;
                if ( clrProperty != null )
                {
                    mappedClrProperties.Add( clrProperty, property );
                }
            }

            foreach ( var property in clrType.GetProperties( bindingFlags ) )
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

                        var itemType = elementType.Definition.GetClrType( edmModel );
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

            var type = default( TypeInfo );

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

                foreach ( var propertyDependency in dependentProperties )
                {
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

        static TypeInfo CreateTypeInfoFromSignature( ModuleBuilder moduleBuilder, ClassSignature @class ) => CreateTypeBuilderFromSignature( moduleBuilder, @class ).CreateTypeInfo();

        static TypeBuilder CreateTypeBuilderFromSignature( ModuleBuilder moduleBuilder, ClassSignature @class )
        {
            Contract.Requires( moduleBuilder != null );
            Contract.Requires( @class != null );
            Contract.Ensures( Contract.Result<TypeBuilder>() != null );

            var typeBuilder = moduleBuilder.DefineType( @class.Name, TypeAttributes.Class );

            foreach ( var attribute in @class.Attributes )
            {
                typeBuilder.SetCustomAttribute( attribute );
            }

            foreach ( var property in @class.Properties )
            {
                var type = property.Type;
                var name = property.Name;
                AddProperty( typeBuilder, type, name, property.Attributes );
            }

            return typeBuilder;
        }

        static IDictionary<EdmTypeKey, TypeInfo> ResolveDependencies( BuilderContext context )
        {
            var edmTypes = context.EdmTypes;
            var dependencies = context.Dependencies;

            for ( var i = 0; i < dependencies.Count; i++ )
            {
                var propertyDependency = dependencies[i];
                var dependentOnType = edmTypes[propertyDependency.DependentOnTypeKey];

                if ( propertyDependency.IsCollection )
                {
                    dependentOnType = IEnumerableOfT.MakeGenericType( dependentOnType ).GetTypeInfo();
                }

                AddProperty( propertyDependency.DependentType, dependentOnType, propertyDependency.PropertyName, propertyDependency.CustomAttributes );
            }

            var keys = edmTypes.Keys.ToArray();

            for ( var i = 0; i < keys.Length; i++ )
            {
                var key = keys[i];

                if ( edmTypes[key] is TypeBuilder typeBuilder )
                {
                    edmTypes[key] = typeBuilder.CreateTypeInfo();
                }
            }

            return edmTypes;
        }

        static PropertyBuilder AddProperty( TypeBuilder addTo, Type shouldBeAdded, string name, IEnumerable<CustomAttributeBuilder> customAttributes )
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

            foreach ( var attribute in customAttributes )
            {
                propertyBuilder.SetCustomAttribute( attribute );
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

            internal IDictionary<EdmTypeKey, TypeInfo> EdmTypes { get; } = new Dictionary<EdmTypeKey, TypeInfo>();

            internal ICollection<EdmTypeKey> VisitedEdmTypes { get; } = new HashSet<EdmTypeKey>();

            internal IList<PropertyDependency> Dependencies { get; } = new List<PropertyDependency>();
        }
    }
}