namespace System.Web.Http
{
    using Microsoft;
    using Microsoft.Web.Http;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http.Formatting;
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

        static MediaTypeFormatter UseICloneable( MediaTypeFormatter instance )
        {
            Contract.Requires( instance != null );
            Contract.Ensures( Contract.Result<MediaTypeFormatter>() != null );

            return (MediaTypeFormatter) ( (ICloneable) instance ).Clone();
        }

        static Func<MediaTypeFormatter, MediaTypeFormatter> NewCloneFunction( Type type )
        {
            Contract.Requires( type != null );
            Contract.Ensures( Contract.Result<Func<MediaTypeFormatter, MediaTypeFormatter>>() != null );

            var clone = NewCopyConstructorActivator( type ) ??
                        NewParameterlessConstructorActivator( type ) ??
                        throw new InvalidOperationException( LocalSR.MediaTypeFormatterNotCloneable.FormatDefault( type.Name, typeof( ICloneable ).Name ) );

            return instance => CloneMediaTypes( clone( instance ) );
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

            return lambda.Compile();  // formatter => new MediaTypeFormatter( formatter );
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

        static MediaTypeFormatter CloneMediaTypes( MediaTypeFormatter instance )
        {
            Contract.Requires( instance != null );
            Contract.Ensures( Contract.Result<MediaTypeFormatter>() != null );

            var mediaTypes = instance.SupportedMediaTypes.ToArray();

            instance.SupportedMediaTypes.Clear();

            foreach ( var mediaType in mediaTypes )
            {
                instance.SupportedMediaTypes.Add( Parse( mediaType.ToString() ) );
            }

            return instance;
        }
    }
}