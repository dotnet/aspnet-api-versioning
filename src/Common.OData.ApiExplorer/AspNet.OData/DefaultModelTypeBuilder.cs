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
        readonly ICollection<Assembly> assemblies;
        readonly ConcurrentDictionary<ApiVersion, ModuleBuilder> modules = new ConcurrentDictionary<ApiVersion, ModuleBuilder>();
        readonly ConcurrentDictionary<ClassSignature, Type> generatedTypes = new ConcurrentDictionary<ClassSignature, Type>();
        readonly Dictionary<EdmTypeKey, Type> visitedEdmTypes = new Dictionary<EdmTypeKey, Type>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultModelTypeBuilder"/> class.
        /// </summary>
        /// <param name="assemblies">The <see cref="IEnumerable{T}">sequence</see> of application <see cref="Assembly">assemblies</see>.</param>
        public DefaultModelTypeBuilder( IEnumerable<Assembly> assemblies )
        {
            Arg.NotNull( assemblies, nameof( assemblies ) );
            this.assemblies = new HashSet<Assembly>( assemblies );
        }

        /// <inheritdoc />
        public Type NewStructuredType( IEdmStructuredType structuredType, Type clrType, ApiVersion apiVersion )
        {
            Arg.NotNull( structuredType, nameof( structuredType ) );
            Arg.NotNull( clrType, nameof( clrType ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( Contract.Result<Type>() != null );

            var typeKey = new EdmTypeKey( structuredType, apiVersion );

            if ( visitedEdmTypes.TryGetValue( typeKey, out var generatedType ) )
            {
                return generatedType;
            }

            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;

            var properties = new List<ClassProperty>();
            var structuralProperties = structuredType.Properties().ToDictionary( p => p.Name, StringComparer.OrdinalIgnoreCase );
            var clrTypeMatchesEdmType = true;

            foreach ( var property in clrType.GetProperties( bindingFlags ) )
            {
                if ( !structuralProperties.TryGetValue( property.Name, out var structuralProperty ) )
                {
                    clrTypeMatchesEdmType = false;
                    continue;
                }

                var structuredTypeRef = structuralProperty.Type;
                var propertyType = property.PropertyType;

                if ( structuredTypeRef.IsCollection() )
                {
                    propertyType = NewStructuredTypeOrSelf( typeKey, structuredTypeRef.AsCollection(), propertyType, apiVersion );
                }
                else if ( structuredTypeRef.IsStructured() )
                {
                    propertyType = NewStructuredTypeOrSelf( typeKey, structuredTypeRef.ToStructuredType(), propertyType, apiVersion );
                }

                clrTypeMatchesEdmType &= propertyType.IsDeclaringType() || property.PropertyType.Equals( propertyType );
                properties.Add( new ClassProperty( property, propertyType ) );
            }

            if ( clrTypeMatchesEdmType )
            {
                return clrType;
            }

            var signature = new ClassSignature( clrType.FullName, properties, apiVersion );

            generatedType = generatedTypes.GetOrAdd( signature, CreateFromSignature );
            visitedEdmTypes.Add( typeKey, generatedType );

            return generatedType;
        }

        /// <inheritdoc />
        public Type NewActionParameters( IEdmAction action, ApiVersion apiVersion )
        {
            Arg.NotNull( action, nameof( action ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( Contract.Result<Type>() != null );

            var name = action.FullName() + "Parameters";
            var properties = action.Parameters.Where( p => p.Name != "bindingParameter" ).Select( p => new ClassProperty( assemblies, p ) );
            var signature = new ClassSignature( name, properties, apiVersion );

            return generatedTypes.GetOrAdd( signature, CreateFromSignature );
        }

        TypeInfo CreateFromSignature( ClassSignature @class )
        {
            Contract.Requires( @class != null );
            Contract.Ensures( Contract.Result<TypeInfo>() != null );

            const MethodAttributes PropertyMethodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            var moduleBuilder = modules.GetOrAdd( @class.ApiVersion, CreateModuleForApiVersion );
            var typeBuilder = moduleBuilder.DefineType( @class.Name, TypeAttributes.Class );

            foreach ( var property in @class.Properties )
            {
                var type = property.GetType( typeBuilder );
                var name = property.Name;
                var field = typeBuilder.DefineField( "_" + name, type, FieldAttributes.Private );
                var propertyBuilder = typeBuilder.DefineProperty( name, PropertyAttributes.HasDefault, type, null );
                var getter = typeBuilder.DefineMethod( "get_" + name, PropertyMethodAttributes, type, Type.EmptyTypes );
                var setter = typeBuilder.DefineMethod( "set_" + name, PropertyMethodAttributes, null, new[] { type } );
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

                foreach ( var attribute in property.Attributes )
                {
                    propertyBuilder.SetCustomAttribute( attribute );
                }
            }

            return typeBuilder.CreateTypeInfo();
        }

        Type NewStructuredTypeOrSelf( EdmTypeKey declaringTypeKey, IEdmCollectionTypeReference collectionType, Type clrType, ApiVersion apiVersion )
        {
            Contract.Requires( collectionType != null );
            Contract.Requires( clrType != null );
            Contract.Requires( apiVersion != null );
            Contract.Ensures( Contract.Result<Type>() != null );

            var elementType = collectionType.ElementType();

            if ( !elementType.IsStructured() )
            {
                return clrType;
            }

            var structuredType = elementType.ToStructuredType();

            if ( declaringTypeKey == new EdmTypeKey( structuredType, apiVersion ) )
            {
                return IEnumerableOfT.MakeGenericType( DeclaringType.Value );
            }

            assemblies.Add( clrType.Assembly );

            var itemType = elementType.Definition.GetClrType( assemblies );
            var newItemType = NewStructuredType( structuredType, itemType, apiVersion );

            if ( !itemType.Equals( newItemType ) )
            {
                clrType = IEnumerableOfT.MakeGenericType( newItemType );
            }

            return clrType;
        }

        Type NewStructuredTypeOrSelf( EdmTypeKey declaringTypeKey, IEdmStructuredType structuredType, Type clrType, ApiVersion apiVersion )
        {
            Contract.Requires( structuredType != null );
            Contract.Requires( clrType != null );
            Contract.Requires( apiVersion != null );
            Contract.Ensures( Contract.Result<Type>() != null );

            if ( declaringTypeKey == new EdmTypeKey( structuredType, apiVersion ) )
            {
                return DeclaringType.Value;
            }

            return NewStructuredType( structuredType, clrType, apiVersion );
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