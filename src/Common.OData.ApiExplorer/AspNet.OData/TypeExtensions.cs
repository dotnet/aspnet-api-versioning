namespace Microsoft.AspNet.OData
{
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNet.OData.Routing;
#if !WEBAPI
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
#endif
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
#if WEBAPI
    using System.Web.Http.Dispatcher;
#endif
    using static System.Reflection.BindingFlags;
#if WEBAPI
    using IActionResult = System.Web.Http.IHttpActionResult;
#endif

    static partial class TypeExtensions
    {
        static readonly Type VoidType = typeof( void );
        static readonly Type ActionResultType = typeof( IActionResult );
        static readonly Type HttpResponseType = typeof( HttpResponseMessage );
        static readonly Type IEnumerableOfT = typeof( IEnumerable<> );

#if WEBAPI
        internal static Type SubstituteIfNecessary( this Type type, IServiceProvider serviceProvider, IAssembliesResolver assembliesResolver, ModelTypeBuilder modelTypeBuilder ) =>
            type.SubstituteIfNecessary( serviceProvider, assembliesResolver.GetAssemblies(), modelTypeBuilder );
#endif

        internal static Type SubstituteIfNecessary( this Type type, IServiceProvider serviceProvider, IEnumerable<Assembly> assemblies, ModelTypeBuilder modelTypeBuilder )
        {
            Contract.Requires( serviceProvider != null );
            Contract.Requires( assemblies != null );
            Contract.Requires( modelTypeBuilder != null );
            Contract.Ensures( Contract.Result<Type>() != null );

            var result = type.CanBeSubstituted();

            if ( !result.IsSupported )
            {
                return type;
            }

            var innerType = result.InnerType;
            var model = serviceProvider.GetRequiredService<IEdmModel>();
            var structuredType = innerType.GetStructuredType( model, assemblies );

            if ( structuredType == null )
            {
                return type;
            }

            if ( structuredType.IsEquivalentTo( innerType ) )
            {
                return type.IsDelta() ? innerType : type;
            }

            var apiVersion = model.GetAnnotationValue<ApiVersionAnnotation>( model ).ApiVersion;

            innerType = modelTypeBuilder.NewStructuredType( structuredType, innerType, apiVersion );

            if ( !type.IsDelta() )
            {
                foreach ( var openType in result.OpenTypes )
                {
                    innerType = openType.MakeGenericType( innerType );
                }
            }

            return innerType;
        }

        static SubstitutionResult CanBeSubstituted( this Type type )
        {
            if ( type == null )
            {
                return default;
            }

            var openTypes = new Stack<Type>();

            // IEnumerable<T>, ODataValue<T>, or Delta<T>
            if ( type.IsGenericTypeWithSingleTypeArgument() )
            {
                openTypes.Push( type.GetGenericTypeDefinition() );
                type = type.GetGenericArguments()[0];

                // ODataValue<IEnumerable<T>>
                if ( type.IsEnumerable() )
                {
                    openTypes.Push( type.GetGenericTypeDefinition() );
                    type = type.GetGenericArguments()[0];
                }
                else if ( type.IsGenericType )
                {
                    return default;
                }
            }

            var supported = Type.GetTypeCode( type ) == TypeCode.Object &&
                            !type.IsValueType &&
                            !type.Equals( VoidType ) &&
                            !type.Equals( ActionResultType ) &&
                            !type.Equals( HttpResponseType );

            return new SubstitutionResult( type, supported, openTypes );
        }

        static bool IsGenericTypeWithSingleTypeArgument( this Type type ) => type.IsGenericType && type.GetGenericArguments().Length == 1;

        static bool IsEnumerable( this Type type )
        {
            if ( !type.IsGenericTypeWithSingleTypeArgument() )
            {
                return false;
            }

            var typeDef = type.GetGenericTypeDefinition();

            return typeDef.Equals( IEnumerableOfT ) || typeDef.GetInterfaces().Any( i => i.IsGenericType && i.GetGenericTypeDefinition().Equals( IEnumerableOfT ) );
        }

        static IEdmStructuredType GetStructuredType( this Type type, IEdmModel model, IEnumerable<Assembly> assemblies )
        {
            Contract.Requires( type != null );
            Contract.Requires( model != null );
            Contract.Requires( assemblies != null );

            var structuredTypes = model.SchemaElements.OfType<IEdmStructuredType>();
            var structuredType = structuredTypes.FirstOrDefault( t => t.GetClrType( assemblies ).Equals( type ) );

            return structuredType;
        }

        static bool IsEquivalentTo( this IEdmStructuredType structuredType, Type type )
        {
            Contract.Requires( structuredType != null );
            Contract.Requires( type != null );

            const BindingFlags bindingFlags = Public | Instance;

            var comparer = StringComparer.OrdinalIgnoreCase;
            var clrProperties = new HashSet<string>( type.GetProperties( bindingFlags ).Select( p => p.Name ), comparer );
            var structuralProperties = new HashSet<string>( structuredType.StructuralProperties().Select( p => p.Name ), comparer );

            return structuralProperties.IsSupersetOf( clrProperties );
        }

        struct SubstitutionResult
        {
            internal SubstitutionResult( Type innerType, bool supported, IEnumerable<Type> openTypes )
            {
                IsSupported = supported;
                InnerType = innerType;
                OpenTypes = openTypes;
            }

            internal readonly bool IsSupported;
            internal readonly Type InnerType;
            internal readonly IEnumerable<Type> OpenTypes;
        }
    }
}