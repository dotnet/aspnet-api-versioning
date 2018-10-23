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

                var propertyType = property.PropertyType;
                var structuredTypeRef = structuralProperty.Type;

                if ( structuredTypeRef.IsCollection() )
                {
                    var collectionType = structuredTypeRef.AsCollection();
                    var elementType = collectionType.ElementType();

                    if ( elementType.IsStructured() )
                    {
                        assemblies.Add( clrType.Assembly );

                        var itemType = elementType.Definition.GetClrType( assemblies );
                        var newItemType = NewStructuredType( elementType.ToStructuredType(), itemType, apiVersion );

                        if ( !itemType.Equals( newItemType ) )
                        {
                            propertyType = IEnumerableOfT.MakeGenericType( newItemType );
                        }
                    }
                }
                else if ( structuredTypeRef.IsStructured() )
                {
                    propertyType = NewStructuredType( structuredTypeRef.ToStructuredType(), property.PropertyType, apiVersion );
                }

                clrTypeMatchesEdmType &= property.PropertyType.Equals( propertyType );
                properties.Add( new ClassProperty( property, propertyType ) );
            }

            if ( clrTypeMatchesEdmType )
            {
                return clrType;
            }

            var signature = new ClassSignature( clrType.FullName, properties, apiVersion );

            return generatedTypes.GetOrAdd( signature, CreateFromSignature );
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
                var field = typeBuilder.DefineField( "_" + property.Name, property.Type, FieldAttributes.Private );
                var propertyBuilder = typeBuilder.DefineProperty( property.Name, PropertyAttributes.HasDefault, property.Type, null );
                var getter = typeBuilder.DefineMethod( "get_" + property.Name, PropertyMethodAttributes, property.Type, Type.EmptyTypes );
                var setter = typeBuilder.DefineMethod( "set_" + property.Name, PropertyMethodAttributes, null, new[] { property.Type } );
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