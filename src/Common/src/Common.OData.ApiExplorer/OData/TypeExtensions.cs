// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

#if NETFRAMEWORK
using Microsoft.OData.Edm;
using System.Net.Http;
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.OData.Edm;
#endif
using System.Reflection;
using System.Reflection.Emit;
#if NETFRAMEWORK
using IActionResult = System.Web.Http.IHttpActionResult;
#else
using static System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes;
#endif

/// <summary>
/// Provides extension methods for the <see cref="Type"/> class.
/// </summary>
public static partial class TypeExtensions
{
    private static readonly Type IEnumerableOfT = typeof( IEnumerable<> );
    private static readonly Type ODataValueOfT = typeof( ODataValue<> );
    private static readonly Type SingleResultOfT = typeof( SingleResult<> );
    private static readonly Type ActionResultType = typeof( IActionResult );
#if NETFRAMEWORK
    private static readonly Type HttpResponseType = typeof( HttpResponseMessage );
#else
    private static readonly Type IAsyncEnumerableOfT = typeof( IAsyncEnumerable<> );
    private static readonly Type ActionResultOfT = typeof( ActionResult<> );
#endif

    /// <param name="type">The <see cref="Type">type</see> to be evaluated.</param>
    extension(
#if !NETFRAMEWORK
        [DynamicallyAccessedMembers( Interfaces | PublicProperties )]
#endif
        Type type )
    {
        /// <summary>
        /// Substitutes the specified type, if required.
        /// </summary>
        /// <param name="context">The current <see cref="TypeSubstitutionContext">type substitution context</see>.</param>
        /// <returns>The original <see cref="Type">type</see> or a substitution <see cref="Type">type</see> based on the
        /// provided <paramref name="context"/>.</returns>
#if !NETFRAMEWORK
        [UnconditionalSuppressMessage( "ILLink", "IL2026" )]
        [UnconditionalSuppressMessage( "ILLink", "IL2073" )]
        [return: DynamicallyAccessedMembers( Interfaces | PublicProperties )]
#endif
        public Type SubstituteIfNecessary( TypeSubstitutionContext context )
        {
            ArgumentNullException.ThrowIfNull( type );
            ArgumentNullException.ThrowIfNull( context );

            var openTypes = new Stack<Type>();
            var apiVersion = context.ApiVersion;
            var resolver = new StructuredTypeResolver( context.Model );
            IEdmStructuredType? structuredType;

            if ( IsSubstitutableGeneric( type, openTypes, out var innerType ) )
            {
                if ( ( structuredType = resolver.GetStructuredType( innerType! ) ) == null )
                {
                    return type;
                }

                var newType = context.ModelTypeBuilder.NewStructuredType( context.Model, structuredType, innerType!, apiVersion );

                if ( innerType!.Equals( newType ) )
                {
                    return type.ShouldExtractInnerType ? innerType : type;
                }

                return CloseGeneric( openTypes, newType );
            }

            if ( type.CanBeSubstituted && ( structuredType = resolver.GetStructuredType( type ) ) != null )
            {
                type = context.ModelTypeBuilder.NewStructuredType( context.Model, structuredType, type, apiVersion );
            }

            return type;
        }
    }

    extension( Type type )
    {
        private bool Is( Type typeDefinition ) => type.IsGenericType && type.GetGenericTypeDefinition().Equals( typeDefinition );

        private bool ShouldExtractInnerType =>
            type.IsDelta ||
#if !NETFRAMEWORK
            type.IsActionResult ||
#endif
            type.IsSingleResult;

        private bool CanBeSubstituted =>
            Type.GetTypeCode( type ) == TypeCode.Object &&
            !type.IsValueType &&
            !type.Equals( ActionResultType ) &&
#if NETFRAMEWORK
            !type.Equals( HttpResponseType ) &&
#endif
            !type.IsODataActionParameters;

        private bool IsSingleResult => type.Is( SingleResultOfT );

#if !NETFRAMEWORK
        private bool IsActionResult => type.Is( ActionResultOfT );
#endif

#if !NETFRAMEWORK
        [UnconditionalSuppressMessage( "ILLink", "IL2070" )]
#endif
        internal bool IsEnumerable( [NotNullWhen( true )] out Type? itemType )
        {
            var types = new Queue<Type>();

            types.Enqueue( type );

            while ( types.Count > 0 )
            {
                type = types.Dequeue();

                if ( type.IsGenericType )
                {
                    var typeDef = type.GetGenericTypeDefinition();

                    if ( typeDef.Equals( IEnumerableOfT )
#if !NETFRAMEWORK
                        || typeDef.Equals( IAsyncEnumerableOfT )
#endif
                       )
                    {
                        itemType = type.GetGenericArguments()[0];
                        return true;
                    }
                }

                var interfaces = type.GetInterfaces();

                for ( var i = 0; i < interfaces.Length; i++ )
                {
                    types.Enqueue( interfaces[i] );
                }
            }

            itemType = default;
            return false;
        }

        internal Type ExtractInnerType()
        {
            if ( !type.IsGenericType )
            {
                return type;
            }

            var typeDef = type.GetGenericTypeDefinition();
            var typeArgs = type.GetGenericArguments();

            if ( typeArgs.Length != 1 )
            {
                return type;
            }

            var generic = typeDef.IsDelta ||
                          typeDef.IsODataValue ||
#if !NETFRAMEWORK
                          typeDef.IsActionResult ||
#endif
                          typeDef.IsSingleResult;

            if ( generic )
            {
                return typeArgs[0];
            }

            return type;
        }
    }

    extension( Type? type )
    {
        private bool IsODataValue
        {
            get
            {
                while ( type != null )
                {
                    if ( !type.IsGenericType )
                    {
                        return false;
                    }

                    var typeDef = type.GetGenericTypeDefinition();

                    if ( typeDef.Equals( ODataValueOfT ) )
                    {
                        return true;
                    }

                    type = type.BaseType;
                }

                return false;
            }
        }
    }

    extension( MemberInfo member )
    {
        internal IEnumerable<CustomAttributeBuilder> DeclaredAttributes
        {
            get
            {
                foreach ( var attribute in member.CustomAttributes )
                {
                    var ctor = attribute.Constructor;
                    var ctorArgs = attribute.ConstructorArguments.Select( a => a.Value ).ToArray();
                    var namedProperties = new List<PropertyInfo>( attribute.NamedArguments.Count );
                    var propertyValues = new List<object>( attribute.NamedArguments.Count );
                    var namedFields = new List<FieldInfo>( attribute.NamedArguments.Count );
                    var fieldValues = new List<object>( attribute.NamedArguments.Count );

                    for ( var i = 0; i < attribute.NamedArguments.Count; i++ )
                    {
                        var argument = attribute.NamedArguments[i];

                        if ( argument.IsField )
                        {
                            namedFields.Add( (FieldInfo) argument.MemberInfo );
                            fieldValues.Add( argument.TypedValue.Value! );
                        }
                        else
                        {
                            namedProperties.Add( (PropertyInfo) argument.MemberInfo );
                            propertyValues.Add( argument.TypedValue.Value! );
                        }
                    }

                    for ( var i = 0; i < ctorArgs.Length; i++ )
                    {
                        if ( ctorArgs[i] is IReadOnlyCollection<CustomAttributeTypedArgument> paramsList )
                        {
                            ctorArgs[i] = paramsList.Select( a => a.Value ).ToArray();
                        }
                    }

                    yield return new CustomAttributeBuilder(
                        ctor,
                        ctorArgs,
                        [.. namedProperties],
                        [.. propertyValues],
                        [.. namedFields],
                        [.. fieldValues] );
                }
            }
        }
    }

    private static bool IsSubstitutableGeneric(
#if !NETFRAMEWORK
        [DynamicallyAccessedMembers( Interfaces )]
#endif
        Type type,
        Stack<Type> openTypes,
        out Type? innerType )
    {
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
        var generic = typeDef.Equals( IEnumerableOfT ) ||
                      typeDef.IsDelta ||
                      typeDef.IsODataValue ||
#if !NETFRAMEWORK
                      typeDef.Equals( IAsyncEnumerableOfT ) ||
                      typeDef.IsActionResult ||
#endif
                      typeDef.IsSingleResult;

        if ( generic )
        {
            innerType = typeArg;
        }
        else
        {
            var interfaces = type.GetInterfaces();

            for ( var i = 0; i < interfaces.Length; i++ )
            {
                if ( interfaces[i].IsEnumerable( out innerType ) )
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
            innerType = nextType!;
        }

        return true;
    }

#if !NETFRAMEWORK
    [RequiresDynamicCode( "Might not be available at runtime" )]
    [RequiresUnreferencedCode( "Cannot be validated by trim analysis" )]
#endif
    private static Type CloseGeneric( Stack<Type> openTypes, Type innerType )
    {
        var type = openTypes.Pop();

        if ( type.ShouldExtractInnerType )
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
}