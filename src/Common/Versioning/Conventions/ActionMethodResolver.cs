#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
#if WEBAPI
    using System.Web.Http;
#endif

    static class ActionMethodResolver
    {
        internal static MethodInfo Resolve( Type controllerType, string methodName, Type[] argumentTypes )
        {
            Contract.Requires( controllerType != null );
            Contract.Requires( !string.IsNullOrEmpty( methodName ) );

            var methods = controllerType.GetRuntimeMethods().Where( m => m.Name == methodName && IsAction( m ) ).ToArray();

            switch ( methods.Length )
            {
                case 0:
                    throw new MissingMethodException( SR.ActionMethodNotFound.FormatDefault( methodName ) );
                case 1:
                    return methods[0];
            }

            argumentTypes = argumentTypes ?? Type.EmptyTypes;
            methods = methods.Where( m => SignatureMatches( m, argumentTypes ) ).ToArray();

            if ( methods.Length == 1 )
            {
                return methods[0];
            }

            throw new AmbiguousMatchException( SR.AmbiguousActionMethod.FormatDefault( methodName ) );
        }

        static bool IsAction( MethodInfo method )
        {
            Contract.Requires( method != null );

            if ( !method.IsPublic || method.IsStatic )
            {
                return false;
            }

            return method.GetCustomAttribute<NonActionAttribute>() == null;
        }

        static bool SignatureMatches( MethodInfo method, Type[] argumentTypes )
        {
            Contract.Requires( method != null );
            Contract.Requires( argumentTypes != null );

            var argTypes = method.GetParameters().Select( p => p.ParameterType ).ToArray();
            return argTypes.SequenceEqual( argumentTypes );
        }
    }
}