// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http;

using Asp.Versioning;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using static System.Linq.Expressions.Expression;
using static System.Net.Http.Headers.MediaTypeHeaderValue;
using static System.Reflection.BindingFlags;

internal static class MediaTypeFormatterAdapterFactory
{
    private const BindingFlags Flags = Public | NonPublic | Instance;
    private static ConcurrentDictionary<Type, Func<MediaTypeFormatter, MediaTypeFormatter>>? cloneFunctions;

    internal static Func<MediaTypeFormatter, MediaTypeFormatter> GetOrCreateCloneFunction( MediaTypeFormatter formatter )
    {
        if ( formatter is ICloneable )
        {
            return UseICloneable;
        }

        var type = formatter.GetType();
        cloneFunctions ??= new();

        return cloneFunctions.GetOrAdd( type, NewCloneFunction );
    }

    private static MediaTypeFormatter UseICloneable( MediaTypeFormatter instance ) => (MediaTypeFormatter) ( (ICloneable) instance ).Clone();

    private static Func<MediaTypeFormatter, MediaTypeFormatter> NewCloneFunction( Type type )
    {
        var clone = NewCopyConstructorActivator( type ) ??
                    NewParameterlessConstructorActivator( type ) ??
                    throw InvalidOperation( type );

        return instance => CloneMediaTypes( clone( instance ), instance );

        static InvalidOperationException InvalidOperation( Type type ) =>
            new( string.Format( CultureInfo.CurrentCulture, LocalSR.MediaTypeFormatterNotCloneable, type.Name, typeof( ICloneable ).Name ) );
    }

    private static Func<MediaTypeFormatter, MediaTypeFormatter>? NewCopyConstructorActivator( Type type )
    {
        var constructors = from ctor in type.GetConstructors( Flags )
                           let args = ctor.GetParameters()
                           where args.Length == 1 && type.Equals( args[0].ParameterType )
                           select ctor;
        var constructor = constructors.SingleOrDefault();

        if ( constructor == null )
        {
            return null;
        }

        var formatter = Parameter( typeof( MediaTypeFormatter ), "formatter" );
        var @new = New( constructor, Convert( formatter, type ) );
        var lambda = Lambda<Func<MediaTypeFormatter, MediaTypeFormatter>>( @new, formatter );

        return ReinitializeSupportedMediaTypes( lambda.Compile() );  // formatter => new MediaTypeFormatter( formatter );
    }

    private static Func<MediaTypeFormatter, MediaTypeFormatter> ReinitializeSupportedMediaTypes( Func<MediaTypeFormatter, MediaTypeFormatter> clone ) =>
        formatter =>
        {
            var instance = clone( formatter );
            SupportedMediaTypesInitializer.Initialize( instance );
            return instance;
        };

    private static Func<MediaTypeFormatter, MediaTypeFormatter>? NewParameterlessConstructorActivator( Type type )
    {
        var constructors = from ctor in type.GetConstructors( Flags )
                           let args = ctor.GetParameters()
                           where args.Length == 0
                           select ctor;
        var constructor = constructors.FirstOrDefault();

        if ( constructor == null )
        {
            return null;
        }

        var formatter = Parameter( typeof( MediaTypeFormatter ), "formatter" );
        var @new = New( constructor );
        var lambda = Lambda<Func<MediaTypeFormatter, MediaTypeFormatter>>( @new, formatter );

        return lambda.Compile();
    }

    private static MediaTypeFormatter CloneMediaTypes( MediaTypeFormatter target, MediaTypeFormatter source )
    {
        var targetMediaTypes = target.SupportedMediaTypes;
        var sourceMediaTypes = source.SupportedMediaTypes;

        targetMediaTypes.Clear();

        for ( var i = 0; i < sourceMediaTypes.Count; i++ )
        {
            targetMediaTypes.Add( Parse( sourceMediaTypes[i].ToString() ) );
        }

        return target;
    }

    /// <summary>
    /// Supports cloning with a copy constructor.
    /// </summary>
    /// <remarks>
    /// REF: https://github.com/ASP-NET-MVC/aspnetwebstack/blob/4e40cdef9c8a8226685f95ef03b746bc8322aa92/src/System.Net.Http.Formatting/Formatting/MediaTypeFormatter.cs#L62"
    /// The <see cref="MediaTypeFormatter"/> copy constructor does not clone the SupportedMediaTypes property or backing field.
    /// </remarks>
    private static class SupportedMediaTypesInitializer
    {
        private static readonly ConstructorInfo newCollection = typeof( MediaTypeFormatter ).GetNestedType( "MediaTypeHeaderValueCollection", Flags ).GetConstructors( Flags ).Single();
        private static readonly FieldInfo field = typeof( MediaTypeFormatter ).GetField( "_supportedMediaTypes", Flags );
        private static readonly PropertyInfo property = typeof( MediaTypeFormatter ).GetProperty( nameof( MediaTypeFormatter.SupportedMediaTypes ), Flags );

        internal static void Initialize( MediaTypeFormatter instance )
        {
            var list = new List<MediaTypeHeaderValue>();
            var collection = newCollection.Invoke( new object[] { list } );

            // the _supportedMediaTypes field is "readonly", which is why we must use Reflection instead of compiling an expression;
            // interestingly, the Reflection API lets us break rules that expression compilation does not
            field.SetValue( instance, list );

            // since the value for the SupportedMediaTypes property comes from the backing field, we must do this here, even
            // though it's possible to set this property with a compiled expression
            property.SetMethod.Invoke( instance, new object[] { collection } );
        }
    }
}