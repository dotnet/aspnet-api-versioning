namespace System.Web.Http
{
    using Microsoft;
    using Microsoft.Web.Http;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Reflection;
    using static System.Linq.Expressions.Expression;
    using static System.Net.Http.Headers.MediaTypeHeaderValue;
    using static System.Reflection.BindingFlags;

    static class MediaTypeFormatterAdapterFactory
    {
        static readonly ConcurrentDictionary<Type, Func<MediaTypeFormatter, MediaTypeFormatter>> cloneFunctions =
            new ConcurrentDictionary<Type, Func<MediaTypeFormatter, MediaTypeFormatter>>();

        internal static Func<MediaTypeFormatter, MediaTypeFormatter> GetOrCreateCloneFunction( MediaTypeFormatter formatter )
        {
            if ( formatter is ICloneable )
            {
                return UseICloneable;
            }

            var type = formatter.GetType();

            return cloneFunctions.GetOrAdd( type, NewCloneFunction );
        }

        static MediaTypeFormatter UseICloneable( MediaTypeFormatter instance ) => (MediaTypeFormatter) ( (ICloneable) instance ).Clone();

        static Func<MediaTypeFormatter, MediaTypeFormatter> NewCloneFunction( Type type )
        {
            Contract.Requires( type != null );
            Contract.Ensures( Contract.Result<Func<MediaTypeFormatter, MediaTypeFormatter>>() != null );

            var clone = NewCopyConstructorActivator( type ) ??
                        NewParameterlessConstructorActivator( type ) ??
                        throw new InvalidOperationException( LocalSR.MediaTypeFormatterNotCloneable.FormatDefault( type.Name, typeof( ICloneable ).Name ) );

            return instance => CloneMediaTypes( clone( instance ), instance );
        }

        static Func<MediaTypeFormatter, MediaTypeFormatter> NewCopyConstructorActivator( Type type )
        {
            Contract.Requires( type != null );

            var constructors = from ctor in type.GetConstructors( Public | NonPublic | Instance )
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

        static Func<MediaTypeFormatter, MediaTypeFormatter> ReinitializeSupportedMediaTypes( Func<MediaTypeFormatter, MediaTypeFormatter> clone )
        {
            Contract.Requires( clone != null );
            Contract.Ensures( Contract.Result<Func<MediaTypeFormatter, MediaTypeFormatter>>() != null );

            return formatter =>
            {
                var instance = clone( formatter );
                SupportedMediaTypesInitializer.Initialize( instance );
                return instance;
            };
        }

        static Func<MediaTypeFormatter, MediaTypeFormatter> NewParameterlessConstructorActivator( Type type )
        {
            Contract.Requires( type != null );

            var constructors = from ctor in type.GetConstructors( Public | NonPublic | Instance )
                               let args = ctor.GetParameters()
                               where args.Length == 0
                               select ctor;
            var constructor = constructors.SingleOrDefault();

            if ( constructor == null )
            {
                return null;
            }

            var formatter = Parameter( typeof( MediaTypeFormatter ), "formatter" );
            var @new = New( constructor );
            var lambda = Lambda<Func<MediaTypeFormatter, MediaTypeFormatter>>( @new, formatter );

            return lambda.Compile(); // formatter => new MediaTypeFormatter();
        }

        static MediaTypeFormatter CloneMediaTypes( MediaTypeFormatter target, MediaTypeFormatter source )
        {
            Contract.Requires( target != null );
            Contract.Requires( source != null );
            Contract.Ensures( Contract.Result<MediaTypeFormatter>() != null );

            target.SupportedMediaTypes.Clear();

            foreach ( var mediaType in source.SupportedMediaTypes )
            {
                target.SupportedMediaTypes.Add( Parse( mediaType.ToString() ) );
            }

            return target;
        }

        /// <summary>
        /// Supports cloning with a copy constructor.
        /// </summary>
        /// <remarks>
        /// The <see cref="MediaTypeFormatter"/> copy constructor does not clone the SupportedMediaTypes property or backing field.
        /// <seealso cref="!:https://github.com/ASP-NET-MVC/aspnetwebstack/blob/4e40cdef9c8a8226685f95ef03b746bc8322aa92/src/System.Net.Http.Formatting/Formatting/MediaTypeFormatter.cs#L62"/>
        /// </remarks>
        static class SupportedMediaTypesInitializer
        {
            static FieldInfo field;
            static PropertyInfo property;
            static readonly ConstructorInfo newCollection;

            static SupportedMediaTypesInitializer()
            {
                var flags = Public | NonPublic | Instance;
                var mediaTypeFormatter = typeof( MediaTypeFormatter );

                field = mediaTypeFormatter.GetField( "_supportedMediaTypes", flags );
                property = mediaTypeFormatter.GetProperty( nameof( MediaTypeFormatter.SupportedMediaTypes ), flags );
                newCollection = mediaTypeFormatter.GetNestedType( "MediaTypeHeaderValueCollection", flags ).GetConstructors( flags ).Single();
            }

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
}