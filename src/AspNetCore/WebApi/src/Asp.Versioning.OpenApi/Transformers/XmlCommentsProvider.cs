// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Transformers;

using System.Reflection;
using System.Text;

internal static class XmlCommentsProvider
{
    public static string GetDocumentationMemberId( MemberInfo member )
    {
        return member switch
        {
            Type t => new StringBuilder( "T:" ).AppendWith( GetTypeName, t ).ToString(),
            MethodInfo m => new StringBuilder( "M:" ).AppendWith( GetTypeName, m.DeclaringType! ).Append( '.' ).AppendWith( GetMethodSignature, m ).ToString(),
            ConstructorInfo c => new StringBuilder( "M:" ).AppendWith( GetTypeName, c.DeclaringType! ).Append( ".#ctor" ).AppendWith( GetParameters, c.GetParameters() ).ToString(),
            PropertyInfo p => new StringBuilder( "P:" ).AppendWith( GetTypeName, p.DeclaringType! ).Append( '.' ).AppendWith( GetProperty, p ).ToString(),
            FieldInfo f => new StringBuilder( "F:" ).AppendWith( GetTypeName, f.DeclaringType! ).Append( '.' ).Append( f.Name ).ToString(),
            EventInfo e => new StringBuilder( "E:" ).AppendWith( GetTypeName, e.DeclaringType! ).Append( '.' ).Append( e.Name ).ToString(),
            _ => string.Empty,
        };
    }

    private static StringBuilder GetTypeName( StringBuilder builder, Type type )
    {
        if ( type.IsGenericType )
        {
            var name = type.FullName ?? type.Name;
            var i = name.IndexOf( '`', StringComparison.Ordinal );

            if ( i >= 0 )
            {
                name = name[..i] + "``" + type.GetGenericArguments().Length;
            }

            return builder.Append( name.Replace( '+', '.' ) );
        }

        return builder.Append( type.FullName ?? type.Name ).Replace( '+', '.' );
    }

    private static StringBuilder GetMethodSignature( StringBuilder builder, MethodInfo method )
    {
        builder.Append( method.Name );

        if ( method.IsGenericMethod )
        {
            builder.Append( "``" ).Append( method.GetGenericArguments().Length );
        }

        return builder.AppendWith( GetParameters, method.GetParameters() );
    }

    private static StringBuilder GetParameters( StringBuilder builder, ParameterInfo[] parameters )
    {
        if ( parameters.Length == 0 )
        {
            return builder;
        }

        builder.Append( '(' );
        builder.AppendWith( GetParameterTypeName, parameters[0].ParameterType );

        for ( var i = 1; i < parameters.Length; i++ )
        {
            builder.Append( ',' ).AppendWith( GetParameterTypeName, parameters[i].ParameterType );
        }

        return builder.Append( ')' );
    }

    private static StringBuilder GetParameterTypeName( StringBuilder builder, Type type )
    {
        if ( type.IsGenericParameter )
        {
            return builder.Append( "``" ).Append( type.GenericParameterPosition );
        }

        if ( type.IsArray )
        {
            return builder.AppendWith( GetParameterTypeName, type.GetElementType()! ).Append( "[]" );
        }

        if ( type.IsGenericType )
        {
            var name = type.FullName ?? type.Name;
            var args = type.GetGenericArguments();
            var i = name.IndexOf( '`', StringComparison.Ordinal );

            if ( i >= 0 )
            {
                builder.Append( name[0..i] );
            }
            else
            {
                builder.Append( name );
            }

            builder.Append( '{' );
            builder.AppendWith( GetParameterTypeName, args[0] );

            for ( i = 1; i < args.Length; i++ )
            {
                builder.Append( ',' ).AppendWith( GetParameterTypeName, args[i] );
            }

            return builder.Append( '}' );
        }

        return builder.Append( type.FullName ?? type.Name );
    }

    private static StringBuilder GetProperty( StringBuilder builder, PropertyInfo property )
    {
        var parameters = property.GetIndexParameters();

        if ( parameters.Length == 0 )
        {
            return builder.Append( property.Name );
        }

        return builder.Append( "Item" ).AppendWith( GetParameters, parameters );
    }
}