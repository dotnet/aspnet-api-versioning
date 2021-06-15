namespace Microsoft.AspNetCore.Mvc.Routing
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using static Microsoft.AspNetCore.Mvc.ApiVersion;
    using static Microsoft.AspNetCore.Mvc.Versioning.ErrorCodes;
    using static System.Environment;
    using static System.Linq.Enumerable;
    using static System.String;

    sealed class ClientErrorBuilder
    {
        internal ApiVersioningOptions? Options { get; set; }

        internal IReportApiVersions? ApiVersionReporter { get; set; }

        internal HttpContext? HttpContext { get; set; }

        internal IReadOnlyCollection<ActionDescriptor>? Candidates { get; set; }

        internal ILogger? Logger { get; set; }

        IErrorResponseProvider ErrorResponseProvider => Options!.ErrorResponses;

        internal RequestHandler Build()
        {
            var feature = HttpContext!.ApiVersioningFeature();
            var request = HttpContext!.Request;
            var method = request.Method;
            var requestedVersion = feature.RawRequestedApiVersion;
            var parsedVersion = feature.RequestedApiVersion;
            var actionNames = new Lazy<string>( () => Join( NewLine, Candidates!.Select( a => a.DisplayName ) ) );
            var allowedMethods = new Lazy<HashSet<string>>( () => AllowedMethodsFromCandidates( Candidates!, parsedVersion ) );
            var apiVersions = new Lazy<ApiVersionModel>( Candidates!.Select( a => a.GetApiVersionModel() ).Aggregate );
            var handlerContext = new RequestHandlerContext( ErrorResponseProvider, ApiVersionReporter!, apiVersions );
            var url = new Uri( request.GetDisplayUrl() ).SafeFullPath();

            if ( parsedVersion == null )
            {
                if ( IsNullOrEmpty( requestedVersion ) )
                {
                    if ( Options!.AssumeDefaultVersionWhenUnspecified || Candidates!.Any( c => c.GetApiVersionModel().IsApiVersionNeutral ) )
                    {
                        return VersionNeutralUnmatched( handlerContext, url, method, allowedMethods.Value, actionNames.Value );
                    }

                    return UnspecifiedApiVersion( handlerContext, actionNames.Value );
                }
                else if ( !TryParse( requestedVersion, out parsedVersion ) )
                {
                    return MalformedApiVersion( handlerContext, url, requestedVersion );
                }
            }
            else if ( IsNullOrEmpty( requestedVersion ) )
            {
                return VersionNeutralUnmatched( handlerContext, url, method, allowedMethods.Value, actionNames.Value );
            }
            else
            {
                requestedVersion = parsedVersion.ToString();
            }

            return Unmatched( handlerContext, url, method, allowedMethods.Value, actionNames.Value, parsedVersion!, requestedVersion, apiVersions );
        }

        static HashSet<string> AllowedMethodsFromCandidates( IEnumerable<ActionDescriptor> candidates, ApiVersion? apiVersion )
        {
            var httpMethods = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

            foreach ( var candidate in candidates )
            {
                if ( candidate.IsMappedTo( apiVersion ) )
                {
                    httpMethods.AddRange( GetHttpMethods( candidate ) );
                }
            }

            return httpMethods;
        }

        static IEnumerable<string> GetHttpMethods( ActionDescriptor action )
        {
            if ( action.ActionConstraints != null )
            {
                foreach ( var constraint in action.ActionConstraints.OfType<HttpMethodActionConstraint>() )
                {
                    foreach ( var method in constraint.HttpMethods )
                    {
                        yield return method;
                    }
                }
            }

            if ( action is ControllerActionDescriptor controllerAction )
            {
                foreach ( var attribute in controllerAction.MethodInfo.GetCustomAttributes( inherit: false ).OfType<IActionHttpMethodProvider>() )
                {
                    foreach ( var method in attribute.HttpMethods )
                    {
                        yield return method;
                    }
                }
            }
        }

        bool MethodSupportedInAnyOtherVersion( string method, ApiVersion version )
        {
            var comparer = StringComparer.OrdinalIgnoreCase;

            foreach ( var candidate in Candidates! )
            {
                if ( candidate.IsMappedTo( version ) )
                {
                    continue;
                }

                var methods = GetHttpMethods( candidate );

                if ( methods.Contains( method, comparer ) )
                {
                    return true;
                }
            }

            return false;
        }

        RequestHandler VersionNeutralUnmatched(
            RequestHandlerContext context,
            string requestUrl,
            string method,
            IReadOnlyCollection<string> allowedMethods,
            string actionNames )
        {
            Logger!.ApiVersionUnspecified( actionNames );
            context.Code = UnsupportedApiVersion;

            if ( allowedMethods.Count == 0 || allowedMethods.Contains( method ) )
            {
                context.Message = SR.VersionNeutralResourceNotSupported.FormatDefault( requestUrl );
                return new BadRequestHandler( context );
            }

            context.Message = SR.VersionNeutralMethodNotSupported.FormatDefault( requestUrl, method );
            context.AllowedMethods = allowedMethods.ToArray();

            return new MethodNotAllowedHandler( context );
        }

        RequestHandler UnspecifiedApiVersion( RequestHandlerContext context, string actionNames )
        {
            Logger!.ApiVersionUnspecified( actionNames );
            context.Code = ApiVersionUnspecified;
            context.Message = SR.ApiVersionUnspecified;

            return new BadRequestHandler( context );
        }

        RequestHandler MalformedApiVersion( RequestHandlerContext context, string requestUrl, string? requestedVersion )
        {
            Logger!.ApiVersionInvalid( requestedVersion );
            context.Code = InvalidApiVersion;
            context.Message = SR.VersionedResourceNotSupported.FormatDefault( requestUrl, requestedVersion );

            return new BadRequestHandler( context );
        }

        RequestHandler Unmatched(
            RequestHandlerContext context,
            string requestUrl,
            string method,
            IReadOnlyCollection<string> allowedMethods,
            string actionNames,
            ApiVersion version,
            string? rawVersion,
            Lazy<ApiVersionModel> aggregateModel )
        {
            Logger!.ApiVersionUnmatched( version, actionNames );
            context.Code = UnsupportedApiVersion;

            var methodNotAllowed =
                ( allowedMethods.Count > 0 &&
                 !allowedMethods.Contains( method ) ) ||
                ( allowedMethods.Count == 0 &&
                  aggregateModel.Value.ImplementedApiVersions.Contains( version ) &&
                  MethodSupportedInAnyOtherVersion( method, version ) );

            if ( methodNotAllowed )
            {
                context.Message = SR.VersionedMethodNotSupported.FormatDefault( requestUrl, rawVersion, method );
                context.AllowedMethods = allowedMethods.Count == 0 ? new[] { method } : allowedMethods.ToArray();

                return new MethodNotAllowedHandler( context );
            }

            context.Message = SR.VersionedResourceNotSupported.FormatDefault( requestUrl, rawVersion );
            return new BadRequestHandler( context );
        }
    }
}