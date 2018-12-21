﻿namespace Microsoft.AspNetCore.Mvc.Routing
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Routing.Matching;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading.Tasks;
    using static Microsoft.AspNetCore.Mvc.Versioning.ApiVersionMapping;
    using static Microsoft.AspNetCore.Mvc.Versioning.ErrorCodes;
    using static System.Threading.Tasks.Task;

    /// <summary>
    /// Represents the <see cref="IEndpointSelectorPolicy">endpoint selector policy</see> for API versions.
    /// </summary>
    [CLSCompliant( false )]
    public sealed class ApiVersionMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        readonly IOptions<ApiVersioningOptions> options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionMatcherPolicy"/> class.
        /// </summary>
        /// <param name="options">The <see cref="ApiVersioningOptions">options</see> associated with the action selector.</param>
        /// <param name="reportApiVersions">The <see cref="IReportApiVersions">object</see> used to report API versions.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory">factory</see> used to create <see cref="ILogger">loggers</see>.</param>
        public ApiVersionMatcherPolicy(
            IOptions<ApiVersioningOptions> options,
            IReportApiVersions reportApiVersions,
            ILoggerFactory loggerFactory )
        {
            Arg.NotNull( options, nameof( options ) );
            Arg.NotNull( reportApiVersions, nameof( reportApiVersions ) );
            Arg.NotNull( loggerFactory, nameof( loggerFactory ) );

            this.options = options;
            ApiVersionReporter = reportApiVersions;
            Logger = loggerFactory.CreateLogger( GetType() );
        }

        /// <inheritdoc />
        public override int Order => 0;

        ApiVersioningOptions Options => options.Value;

        IApiVersionSelector ApiVersionSelector => Options.ApiVersionSelector;

        IReportApiVersions ApiVersionReporter { get; }

        ILogger Logger { get; }

        /// <inheritdoc />
        public bool AppliesToEndpoints( IReadOnlyList<Endpoint> endpoints )
        {
            Arg.NotNull( endpoints, nameof( endpoints ) );

            for ( var i = 0; i < endpoints.Count; i++ )
            {
                var action = endpoints[i].Metadata?.GetMetadata<ActionDescriptor>();

                if ( action?.GetProperty<ApiVersionModel>() != null )
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public Task ApplyAsync( HttpContext httpContext, EndpointSelectorContext context, CandidateSet candidates )
        {
            Arg.NotNull( httpContext, nameof( httpContext ) );
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( candidates, nameof( candidates ) );

            if ( IsRequestedApiVersionAmbiguous( httpContext, context, out var apiVersion ) )
            {
                return CompletedTask;
            }

            if ( apiVersion == null && Options.AssumeDefaultVersionWhenUnspecified )
            {
                apiVersion = TrySelectApiVersion( httpContext, candidates );
                httpContext.Features.Get<IApiVersioningFeature>().RequestedApiVersion = apiVersion;
            }

            var finalMatches = EvaluateApiVersion( httpContext, candidates, apiVersion );

            if ( finalMatches.Count == 0 )
            {
                context.Endpoint = ClientError( httpContext, candidates );
            }
            else
            {
                for ( var i = 0; i < finalMatches.Count; i++ )
                {
                    candidates.SetValidity( finalMatches[i].index, true );
                }
            }

            return CompletedTask;
        }

        static IReadOnlyList<(int index, ActionDescriptor action)> EvaluateApiVersion(
            HttpContext httpContext,
            CandidateSet candidates,
            ApiVersion apiVersion )
        {
            Contract.Requires( httpContext != null );
            Contract.Requires( candidates != null );
            Contract.Ensures( Contract.Result<IReadOnlyList<(int index, ActionDescriptor action)>>() != null );

            var bestMatches = new List<(int index, ActionDescriptor action)>();
            var implicitMatches = new List<(int, ActionDescriptor)>();

            for ( var i = 0; i < candidates.Count; i++ )
            {
                if ( !candidates.IsValidCandidate( i ) )
                {
                    continue;
                }

                ref var candidate = ref candidates[i];
                var action = candidate.Endpoint.Metadata?.GetMetadata<ActionDescriptor>();

                if ( action == null )
                {
                    candidates.SetValidity( i, false );
                    continue;
                }

                switch ( action.MappingTo( apiVersion ) )
                {
                    case Explicit:
                        bestMatches.Add( (i, action) );
                        break;
                    case Implicit:
                        implicitMatches.Add( (i, action) );
                        break;
                }

                // perf: always make the candidate invalid so we only need to loop through the
                // final, best matches for any remaining, valid candidates
                candidates.SetValidity( i, false );
            }

            switch ( bestMatches.Count )
            {
                case 0:
                    bestMatches.AddRange( implicitMatches );
                    break;
                case 1:
                    var model = bestMatches[0].action.GetApiVersionModel();

                    if ( model.IsApiVersionNeutral )
                    {
                        bestMatches.AddRange( implicitMatches );
                    }

                    break;
            }

            return bestMatches.ToArray();
        }

        bool IsRequestedApiVersionAmbiguous( HttpContext httpContext, EndpointSelectorContext context, out ApiVersion apiVersion )
        {
            Contract.Requires( httpContext != null );
            Contract.Requires( context != null );

            try
            {
                apiVersion = httpContext.GetRequestedApiVersion();
            }
            catch ( AmbiguousApiVersionException ex )
            {
                Logger.LogInformation( ex.Message );
                apiVersion = default;

                var handlerContext = new RequestHandlerContext( Options.ErrorResponses )
                {
                    Code = AmbiguousApiVersion,
                    Message = ex.Message,
                };

                context.Endpoint = new BadRequestHandler( handlerContext );
                return true;
            }

            return false;
        }

        ApiVersion TrySelectApiVersion( HttpContext httpContext, CandidateSet candidates )
        {
            Contract.Requires( httpContext != null );
            Contract.Requires( candidates != null );

            var models = new List<ApiVersionModel>();

            for ( var i = 0; i < candidates.Count; i++ )
            {
                ref var candidate = ref candidates[i];
                var model = candidate.Endpoint.Metadata?.GetMetadata<ActionDescriptor>()?.GetApiVersionModel();

                if ( model != null )
                {
                    models.Add( model );
                }
            }

            return ApiVersionSelector.SelectVersion( httpContext.Request, models.Aggregate() );
        }

        RequestHandler ClientError( HttpContext httpContext, CandidateSet candidateSet )
        {
            var candidates = new List<ActionDescriptor>( candidateSet.Count );

            for ( var i = 0; i < candidateSet.Count; i++ )
            {
                ref var candidate = ref candidateSet[i];
                var action = candidate.Endpoint.Metadata?.GetMetadata<ActionDescriptor>();

                if ( action != null )
                {
                    candidates.Add( action );
                }
            }

            var builder = new ClientErrorBuilder()
            {
                Options = Options,
                ApiVersionReporter = ApiVersionReporter,
                HttpContext = httpContext,
                Candidates = candidates,
                Logger = Logger,
            };

            return builder.Build();
        }
    }
}