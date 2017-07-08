namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using ActionConstraints;
    using Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Extensions.Logging.LoggerMessage;
    using static Extensions.Logging.LogLevel;

    static class ILoggerExtensions
    {
        static readonly Action<ILogger, string, Exception> ambiguousActions =
            Define<string>( Error, 1, "Request matched multiple actions resulting in ambiguity. Matching actions: {AmbiguousActions}" );

        static readonly Action<ILogger, string, string, IActionConstraint, Exception> constraintMismatch =
            Define<string, string, IActionConstraint>( Debug, 2, "Action '{ActionName}' with id '{ActionId}' did not match the constraint '{ActionConstraint}'" );

        static readonly Action<ILogger, string, Exception> apiVersionUnspecified =
            Define<string>( Information, 3, "Request did not specify a service API version, but multiple candidate actions were found. Candidate actions: {CandidateActions}" );

        static readonly Action<ILogger, ApiVersion, string, Exception> apiVersionUnspecifiedWithDefaultVersion =
            Define<ApiVersion, string>( Information, 4, "Request did not specify a service API version, but multiple candidate actions were found; however, none matched the selected default API version '{ApiVersion}'. Candidate actions: {CandidateActions}" );

        static readonly Action<ILogger, ApiVersion, string, Exception> apiVersionUnmatched =
            Define<ApiVersion, string>( Information, 5, "Multiple candidate actions were found, but none matched the requested service API version '{ApiVersion}'. Candidate actions: {CandidateActions}" );

        static readonly Action<ILogger, string, Exception> apiVersionInvalid =
            Define<string>( Information, 6, "Request contained the service API version '{ApiVersion}', which is not valid" );

        static readonly Action<ILogger, string[], Exception> noActionsMatched =
            Define<string[]>( Debug, 3, "No actions matched the current request. Route values: {RouteValues}" );

        internal static void AmbiguousActions( this ILogger logger, string actionNames ) => ambiguousActions( logger, actionNames, null );

        internal static void ConstraintMismatch( this ILogger logger, string actionName, string actionId, IActionConstraint actionConstraint ) =>
            constraintMismatch( logger, actionName, actionId, actionConstraint, null );

        internal static void ApiVersionUnspecified( this ILogger logger, string actionNames ) => apiVersionUnspecified( logger, actionNames, null );

        internal static void ApiVersionUnspecified( this ILogger logger, ApiVersion apiVersion, string actionNames ) => apiVersionUnspecifiedWithDefaultVersion( logger, apiVersion, actionNames, null );

        internal static void ApiVersionUnmatched( this ILogger logger, ApiVersion apiVersion, string actionNames ) => apiVersionUnmatched( logger, apiVersion, actionNames, null );

        internal static void ApiVersionInvalid( this ILogger logger, string apiVersion ) => apiVersionInvalid( logger, apiVersion, null );

        internal static void NoActionsMatched( this ILogger logger, IDictionary<string, object> routeValueDictionary )
        {
            if ( !logger.IsEnabled( Debug ) )
            {
                return;
            }

            var routeValues = default( string[] );

            if ( routeValueDictionary != null )
            {
                routeValues = routeValueDictionary.Select( pair => pair.Key + "=" + Convert.ToString( pair.Value ) ).ToArray();
            }

            noActionsMatched( logger, routeValues, null );
        }
    }
}