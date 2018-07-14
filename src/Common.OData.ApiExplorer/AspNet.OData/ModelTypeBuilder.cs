namespace Microsoft.AspNet.OData
{
#if WEBAPI
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http;
#else
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
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

    sealed class ModelTypeBuilder
    {
        readonly IAssembliesResolver assembliesResolver;
        readonly ConcurrentDictionary<ApiVersion, ModuleBuilder> modules = new ConcurrentDictionary<ApiVersion, ModuleBuilder>();
        readonly ConcurrentDictionary<ClassSignature, Type> generatedTypes = new ConcurrentDictionary<ClassSignature, Type>();

        internal ModelTypeBuilder( IAssembliesResolver assembliesResolver )
        {
            Contract.Requires( assembliesResolver != null );
            this.assembliesResolver = assembliesResolver;
        }

        internal Type NewStructuredType( IEdmStructuredType structuredType, Type clrType, ApiVersion apiVersion )
        {
            Contract.Requires( structuredType != null );
            Contract.Requires( clrType != null );
            Contract.Requires( apiVersion != null );
            Contract.Ensures( Contract.Result<Type>() != null );

            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;

            var properties = new List<ClassProperty>();
            var structuralProperties = new HashSet<string>( structuredType.StructuralProperties().Select( p => p.Name ), StringComparer.OrdinalIgnoreCase );

            foreach ( var property in clrType.GetProperties( bindingFlags ) )
            {
                if ( structuralProperties.Contains( property.Name ) )
                {
                    properties.Add( new ClassProperty( property ) );
                }
            }

            var name = clrType.FullName;
            var signature = new ClassSignature( name, properties, apiVersion );

            return generatedTypes.GetOrAdd( signature, CreateFromSignature );
        }

        internal Type NewActionParameters( IEdmAction action, ApiVersion apiVersion )
        {
            Contract.Requires( action != null );
            Contract.Requires( apiVersion != null );
            Contract.Ensures( Contract.Result<Type>() != null );

            var name = action.FullName() + "Parameters";
            var properties = action.Parameters.Where( p => p.Name != "bindingParameter" ).Select( p => new ClassProperty( assembliesResolver, p ) );
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