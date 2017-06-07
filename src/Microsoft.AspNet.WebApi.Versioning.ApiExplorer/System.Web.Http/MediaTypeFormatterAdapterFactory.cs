namespace System.Web.Http
{
    using Microsoft;
    using Microsoft.Web.Http;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http.Formatting;
    using System.Reflection;
    using static System.Reflection.BindingFlags;
    using static System.Linq.Expressions.Expression;
    using static System.Net.Http.Headers.MediaTypeHeaderValue;

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

            var clone = NewActivator( type );
            return instance => CloneMediaTypes( clone( instance ) );
        }

        static Func<MediaTypeFormatter, MediaTypeFormatter> NewActivator( Type type )
        {
            Contract.Requires( type != null );
            Contract.Ensures( Contract.Result<Func<MediaTypeFormatter, MediaTypeFormatter>>() != null );

            var ctor = ResolveConstructor( type );
            var formatter = Parameter( typeof( MediaTypeFormatter ), "formatter" );
            var @new = New( ctor, Convert( formatter, type ) );
            var lambda = Lambda<Func<MediaTypeFormatter, MediaTypeFormatter>>( @new, formatter );

            return lambda.Compile();
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

        static ConstructorInfo ResolveConstructor( Type type )
        {
            var constructors = from ctor in type.GetConstructors( Public | NonPublic | Instance )
                               let args = ctor.GetParameters()
                               where args.Length == 1 && type.Equals( args[0].ParameterType )
                               select ctor;
            var constructor = constructors.SingleOrDefault();

            if ( constructor == null )
            {
                throw new InvalidOperationException( LocalSR.MediaTypeFormatterNotCloneable.FormatDefault( type.Name, typeof( ICloneable ).Name ) );
            }

            return constructor;
        }
    }
}