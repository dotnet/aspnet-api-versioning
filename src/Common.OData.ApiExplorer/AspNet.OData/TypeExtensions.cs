namespace Microsoft.AspNet.OData
{
#if !WEBAPI
    using Microsoft.AspNetCore.Mvc;
#endif
    using Microsoft.OData.Edm;
#if WEBAPI
    using Microsoft.Web.Http;
#endif
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Net.Http;
    using static System.StringComparison;
#if WEBAPI
    using IActionResult = System.Web.Http.IHttpActionResult;
#endif

    /// <summary>
    /// Provides extension methods for the <see cref="Type"/> class.
    /// </summary>
    public static partial class TypeExtensions
    {
        static readonly Type VoidType = typeof( void );
        static readonly Type ActionResultType = typeof( IActionResult );
        static readonly Type HttpResponseType = typeof( HttpResponseMessage );
        static readonly Type IEnumerableOfT = typeof( IEnumerable<> );
        static readonly Type ODataValueOfT = typeof( ODataValue<> );

        /// <summary>
        /// Substitutes the specified type, if required.
        /// </summary>
        /// <param name="type">The <see cref="Type">type</see> to be evaluated.</param>
        /// <param name="context">The current <see cref="TypeSubstitutionContext">type substitution context</see>.</param>
        /// <returns>The original <paramref name="type"/> or a substitution <see cref="Type">type</see> based on the
        /// provided <paramref name="context"/>.</returns>
        public static Type SubstituteIfNecessary( this Type type, TypeSubstitutionContext context )
        {
            Arg.NotNull( type, nameof( type ) );
            Arg.NotNull( context, nameof( context ) );
            Contract.Ensures( Contract.Result<Type>() != null );

            var openTypes = new Stack<Type>();
            var holder = new Lazy<Tuple<ApiVersion, StructuredTypeResolver>>(
                () => Tuple.Create( context.ApiVersion, new StructuredTypeResolver( context.Model, context.Assemblies ) ) );

            if ( IsSubstitutableGeneric( type, openTypes, out var innerType ) )
            {
                var (apiVersion, resolver) = holder.Value;
                var structuredType = resolver.GetStructuredType( innerType );

                if ( structuredType == null )
                {
                    return type;
                }

                var newType = context.ModelTypeBuilder.NewStructuredType( structuredType, innerType, apiVersion );

                if ( innerType.Equals( newType ) )
                {
                    return type.ExtractInnerType() ? innerType : type;
                }

                return CloseGeneric( openTypes, newType );
            }

            if ( CanBeSubstituted( type ) )
            {
                var (apiVersion, resolver) = holder.Value;
                var structuredType = resolver.GetStructuredType( type );
                type = context.ModelTypeBuilder.NewStructuredType( structuredType, type, apiVersion );
            }

            return type;
        }

        internal static void Deconstruct<T1, T2>( this Tuple<T1, T2> tuple, out T1 item1, out T2 item2 )
        {
            Contract.Requires( tuple != null );

            item1 = tuple.Item1;
            item2 = tuple.Item2;
        }

        internal static bool IsDeclaringType( this Type type )
        {
            Contract.Requires( type != null );

            if ( type == DeclaringType.Value )
            {
                return true;
            }

            if ( !type.IsGenericType )
            {
                return false;
            }

            var typeArgs = type.GetGenericArguments();

            return typeArgs.Length == 1 && typeArgs[0] == DeclaringType.Value;
        }

        static bool IsSubstitutableGeneric( Type type, Stack<Type> openTypes, out Type innerType )
        {
            Contract.Requires( type != null );
            Contract.Requires( openTypes != null );

            innerType = default;

            if ( !type.IsGenericType )
            {
                return false;
            }

            var typeDef = type.GetGenericTypeDefinition();
            var typeArgs = type.GetGenericArguments();

            if ( typeArgs.Length != 1 )
            {
                return false;
            }

            openTypes.Push( typeDef );

            var typeArg = typeArgs[0];

            if ( typeDef.Equals( IEnumerableOfT ) || typeDef.IsDelta() || typeDef.Equals( ODataValueOfT ) || typeDef.IsActionResult() )
            {
                innerType = typeArg;
            }
            else
            {
                foreach ( var @interface in type.GetInterfaces() )
                {
                    if ( @interface.IsEnumerable( out innerType ) )
                    {
                        break;
                    }
                }
            }

            if ( innerType == null )
            {
                return false;
            }

            // examples: ODataValue<IEnumerable<Entity>>, ActionResult<IEnumerable<Entity>>
            while ( innerType.IsEnumerable( out var nextType ) )
            {
                openTypes.Push( IEnumerableOfT );
                innerType = nextType;
            }

            return true;
        }

        static Type CloseGeneric( Stack<Type> openTypes, Type innerType )
        {
            Contract.Requires( openTypes != null );
            Contract.Requires( openTypes.Count > 0 );
            Contract.Requires( innerType != null );

            var type = openTypes.Pop();

            if ( type.ExtractInnerType() )
            {
                return innerType;
            }

            type = type.MakeGenericType( innerType );

            while ( openTypes.Count > 0 )
            {
                type = openTypes.Pop().MakeGenericType( type );
            }

            return type;
        }

        static bool CanBeSubstituted( Type type )
        {
            Contract.Requires( type != null );

            return Type.GetTypeCode( type ) == TypeCode.Object &&
                  !type.IsValueType &&
                  !type.Equals( VoidType ) &&
                  !type.Equals( ActionResultType ) &&
                  !type.Equals( HttpResponseType ) &&
                  !type.IsODataActionParameters();
        }

        static bool IsEnumerable( this Type type, out Type itemType )
        {
            Contract.Requires( type != null );

            itemType = default;

            if ( !type.IsGenericType )
            {
                return false;
            }

            var typeDef = type.GetGenericTypeDefinition();

            if ( typeDef.Equals( IEnumerableOfT ) )
            {
                itemType = type.GetGenericArguments()[0];
                return true;
            }

            foreach ( var @interface in type.GetInterfaces() )
            {
                if ( @interface.IsEnumerable( out itemType ) )
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsActionResult( this Type type ) =>
            type.IsGenericType &&
            type.GetGenericTypeDefinition().FullName.Equals( "Microsoft.AspNetCore.Mvc.ActionResult`1", Ordinal );

        static bool ExtractInnerType( this Type type ) => type.IsDelta() || type.IsActionResult();
    }
}