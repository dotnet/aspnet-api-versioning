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
using System.Runtime.CompilerServices;
#if NETFRAMEWORK
using IActionResult = System.Web.Http.IHttpActionResult;
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

    /// <summary>
    /// Substitutes the specified type, if required.
    /// </summary>
    /// <param name="type">The <see cref="Type">type</see> to be evaluated.</param>
    /// <param name="context">The current <see cref="TypeSubstitutionContext">type substitution context</see>.</param>
    /// <returns>The original <paramref name="type"/> or a substitution <see cref="Type">type</see> based on the
    /// provided <paramref name="context"/>.</returns>
    public static Type SubstituteIfNecessary( this Type type, TypeSubstitutionContext context )
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
                return type.ShouldExtractInnerType() ? innerType : type;
            }

            return CloseGeneric( openTypes, newType );
        }

        if ( CanBeSubstituted( type ) && ( structuredType = resolver.GetStructuredType( type ) ) != null )
        {
            type = context.ModelTypeBuilder.NewStructuredType( context.Model, structuredType, type, apiVersion );
        }

        return type;
    }

    internal static IEnumerable<CustomAttributeBuilder> DeclaredAttributes( this MemberInfo member )
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

    internal static Type ExtractInnerType( this Type type )
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

        var generic = typeDef.IsDelta() ||
                      typeDef.IsODataValue() ||
#if !NETFRAMEWORK
                      typeDef.IsActionResult() ||
#endif
                      typeDef.IsSingleResult();

        if ( generic )
        {
            return typeArgs[0];
        }

        return type;
    }

    private static bool IsSubstitutableGeneric( Type type, Stack<Type> openTypes, out Type? innerType )
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
                      typeDef.IsDelta() ||
                      typeDef.IsODataValue() ||
#if !NETFRAMEWORK
                      typeDef.Equals( IAsyncEnumerableOfT ) ||
                      typeDef.IsActionResult() ||
#endif
                      typeDef.IsSingleResult();

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

    private static Type CloseGeneric( Stack<Type> openTypes, Type innerType )
    {
        var type = openTypes.Pop();

        if ( type.ShouldExtractInnerType() )
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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool CanBeSubstituted( Type type ) =>
        Type.GetTypeCode( type ) == TypeCode.Object &&
        !type.IsValueType &&
        !type.Equals( ActionResultType ) &&
#if NETFRAMEWORK
        !type.Equals( HttpResponseType ) &&
#endif
        !type.IsODataActionParameters();

    internal static bool IsEnumerable( this Type type, [NotNullWhen( true )] out Type? itemType )
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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool IsSingleResult( this Type type ) => type.Is( SingleResultOfT );

    private static bool IsODataValue( this Type? type )
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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool Is( this Type type, Type typeDefinition ) =>
        type.IsGenericType && type.GetGenericTypeDefinition().Equals( typeDefinition );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool ShouldExtractInnerType( this Type type ) =>
        type.IsDelta() ||
#if !NETFRAMEWORK
        type.IsActionResult() ||
#endif
        type.IsSingleResult();

#if !NETFRAMEWORK
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool IsActionResult( this Type type ) => type.Is( ActionResultOfT );
#endif
}