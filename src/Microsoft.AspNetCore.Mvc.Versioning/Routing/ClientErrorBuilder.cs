namespace Microsoft.AspNetCore.Mvc.Routing
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
#if NETSTANDARD2_0
    using Microsoft.AspNetCore.Mvc.Internal;
#endif
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using static Microsoft.AspNetCore.Mvc.ApiVersion;
    using static Microsoft.AspNetCore.Mvc.Versioning.ErrorCodes;
    using static System.Environment;
    using static System.Linq.Enumerable;
    using static System.String;

    sealed class ClientErrorBuilder
    {
        internal ApiVersioningOptions Options { get; set; }

        internal IReportApiVersions ApiVersionReporter { get; set; }

        internal HttpContext HttpContext { get; set; }

        internal IReadOnlyCollection<ActionDescriptor> Candidates { get; set; }

        internal ILogger Logger { get; set; }

        IErrorResponseProvider ErrorResponseProvider => Options.ErrorResponses;

        internal RequestHandler Build()
        {
            var feature = HttpContext.Features.Get<IApiVersioningFeature>();
            var request = HttpContext.Request;
            var method = request.Method;
            var requestedVersion = feature.RawRequestedApiVersion;
            var parsedVersion = feature.RequestedApiVersion;
            var actionNames = new Lazy<string>( () => Join( NewLine, Candidates.Select( a => a.DisplayName ) ) );
            var allowedMethods = new Lazy<HashSet<string>>( () => AllowedMethodsFromCandidates( Candidates, parsedVersion ) );
            var apiVersions = new Lazy<ApiVersionModel>( Candidates.Select( a => a.GetApiVersionModel() ).Aggregate );
            var handlerContext = new RequestHandlerContext( ErrorResponseProvider, ApiVersionReporter, apiVersions );
            var url = new Uri( request.GetDisplayUrl() ).SafeFullPath();

            if ( parsedVersion == null )
            {
                if ( IsNullOrEmpty( requestedVersion ) )
                {
                    if ( Options.AssumeDefaultVersionWhenUnspecified || Candidates.Any( c => c.GetApiVersionModel().IsApiVersionNeutral ) )
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

            return Unmatched( handlerContext, url, method, allowedMethods.Value, actionNames.Value, parsedVersion, requestedVersion );
        }

        static HashSet<string> AllowedMethodsFromCandidates( IEnumerable<ActionDescriptor> candidates, ApiVersion apiVersion )
        {
            Contract.Requires( candidates != null );
            Contract.Ensures( Contract.Result<HashSet<string>>() != null );

            var httpMethods = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

            foreach ( var candidate in candidates )
            {
                if ( candidate.ActionConstraints == null || !candidate.IsMappedTo( apiVersion ) )
                {
                    continue;
                }

                foreach ( var constraint in candidate.ActionConstraints.OfType<HttpMethodActionConstraint>() )
                {
                    httpMethods.AddRange( constraint.HttpMethods );
                }
            }

            return httpMethods;
        }

        RequestHandler VersionNeutralUnmatched(
            RequestHandlerContext context,
            string requestUrl,
            string method,
            IReadOnlyCollection<string> allowedMethods,
            string actionNames )
        {
            Contract.Requires( context != null );
            Contract.Requires( !IsNullOrEmpty( requestUrl ) );
            Contract.Requires( !IsNullOrEmpty( method ) );
            Contract.Requires( allowedMethods != null );

            Logger.ApiVersionUnspecified( actionNames );
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
            Contract.Requires( context != null );

            Logger.ApiVersionUnspecified( actionNames );
            context.Code = ApiVersionUnspecified;
            context.Message = SR.ApiVersionUnspecified;

            return new BadRequestHandler( context );
        }

        RequestHandler MalformedApiVersion( RequestHandlerContext context, string requestUrl, string requestedVersion )
        {
            Contract.Requires( context != null );
            Contract.Requires( !IsNullOrEmpty( requestUrl ) );
            Contract.Requires( !IsNullOrEmpty( requestedVersion ) );

            Logger.ApiVersionInvalid( requestedVersion );
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
            ApiVersion parsedVersion,
            string requestedVersion )
        {
            Contract.Requires( context != null );
            Contract.Requires( !IsNullOrEmpty( requestUrl ) );
            Contract.Requires( !IsNullOrEmpty( method ) );
            Contract.Requires( allowedMethods != null );
            Contract.Requires( parsedVersion != null );
            Contract.Requires( !IsNullOrEmpty( requestedVersion ) );

            Logger.ApiVersionUnmatched( parsedVersion, actionNames );
            context.Code = UnsupportedApiVersion;

            if ( allowedMethods.Count == 0 || allowedMethods.Contains( method ) )
            {
                context.Message = SR.VersionedResourceNotSupported.FormatDefault( requestUrl, requestedVersion );
                return new BadRequestHandler( context );
            }

            context.Message = SR.VersionedMethodNotSupported.FormatDefault( requestUrl, requestedVersion, method );
            context.AllowedMethods = allowedMethods.ToArray();

            return new MethodNotAllowedHandler( context );
        }
    }
}