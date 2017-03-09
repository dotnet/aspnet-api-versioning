namespace Microsoft.Web.Http.Controllers
{
    using Dispatcher;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using static System.Threading.Interlocked;

    /// <summary>
    /// Represents the logic for selecting a versioned, action method.
    /// </summary>
    public partial class ApiVersionActionSelector : IHttpActionSelector
    {
        readonly object cacheKey = new object();
        ActionSelectorCacheItem fastCache;

        ActionSelectorCacheItem GetInternalSelector( HttpControllerDescriptor controllerDescriptor )
        {
            Contract.Requires( controllerDescriptor != null );
            Contract.Ensures( Contract.Result<ActionSelectorCacheItem>() != null );

            if ( fastCache == null )
            {
                var selector = new ActionSelectorCacheItem( controllerDescriptor );
                CompareExchange( ref fastCache, selector, null );
                return selector;
            }
            else if ( fastCache.HttpControllerDescriptor == controllerDescriptor )
            {
                return fastCache;
            }
            else
            {
                if ( controllerDescriptor.Properties.TryGetValue( cacheKey, out var cacheValue ) )
                {
                    return (ActionSelectorCacheItem) cacheValue;
                }

                var selector = new ActionSelectorCacheItem( controllerDescriptor );
                controllerDescriptor.Properties.TryAdd( cacheKey, selector );
                return selector;
            }
        }

        /// <summary>
        /// Selects the version of an action using the provided controller context and candidate action descriptors.
        /// </summary>
        /// <param name="controllerContext">The current <see cref="HttpControllerContext">controller context</see>.</param>
        /// <param name="candidateActions">The <see cref="IReadOnlyList{T}">read-only list</see> of candidate
        /// <see cref="HttpActionDescriptor">action descriptors</see> to select from.</param>
        /// <returns>The matching <see cref="HttpActionDescriptor">action descriptor</see> or <c>null</c> is no
        /// match is found.</returns>
        /// <remarks>This method should return <c>null</c> if either no match is found or the matched action is
        /// ambiguous among the provided list of <paramref name="candidateActions">candidate actions</paramref>.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        protected virtual HttpActionDescriptor SelectActionVersion( HttpControllerContext controllerContext, IReadOnlyList<HttpActionDescriptor> candidateActions )
        {
            Arg.NotNull( controllerContext, nameof( controllerContext ) );
            Arg.NotNull( candidateActions, nameof( candidateActions ) );

            if ( candidateActions.Count == 0 )
            {
                return null;
            }

            var request = controllerContext.Request;
            var requestedVersion = request.GetRequestedApiVersion();
            var exceptionFactory = new HttpResponseExceptionFactory( request );

            if ( candidateActions.Count == 1 )
            {
                var action = candidateActions[0];
                var versions = action.GetApiVersions();
                var matched = versions.Count == 0 || versions.Contains( requestedVersion );
                return matched ? action : null;
            }

            var implicitMatches = new List<HttpActionDescriptor>();
            var explicitMatches = new List<HttpActionDescriptor>();

            foreach ( var action in candidateActions )
            {
                var versions = action.GetApiVersions();

                if ( versions.Count == 0 )
                {
                    implicitMatches.Add( action );
                }
                else if ( versions.Contains( requestedVersion ) )
                {
                    explicitMatches.Add( action );
                }
            }

            switch ( explicitMatches.Count )
            {
                case 0:
                    switch ( implicitMatches.Count )
                    {
                        case 0:
                            break;
                        case 1:
                            return implicitMatches[0];
                        default:
                            throw CreateAmbiguousActionException( implicitMatches );
                    }
                    break;
                case 1:
                    return explicitMatches[0];
                default:
                    throw CreateAmbiguousActionException( explicitMatches );
            }

            return null;
        }

        Exception CreateAmbiguousActionException( IEnumerable<HttpActionDescriptor> matches )
        {
            var ambiguityList = ActionSelectorCacheItem.CreateAmbiguousMatchList( matches );
            return new InvalidOperationException( SR.ApiControllerActionSelector_AmbiguousMatch.FormatDefault( ambiguityList ) );
        }

        /// <summary>
        /// Selects and returns the action descriptor to invoke given the provided controller context.
        /// </summary>
        /// <param name="controllerContext">The current <see cref="HttpControllerContext">controller context</see>.</param>
        /// <returns>The <see cref="HttpActionDescriptor">action descriptor</see> that matches the current
        /// <paramref name="controllerContext">controller context</paramref>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public virtual HttpActionDescriptor SelectAction( HttpControllerContext controllerContext )
        {
            Arg.NotNull( controllerContext, nameof( controllerContext ) );
            Contract.Ensures( Contract.Result<HttpActionDescriptor>() != null );

            var internalSelector = GetInternalSelector( controllerContext.ControllerDescriptor );
            return internalSelector.SelectAction( controllerContext, SelectActionVersion );
        }

        /// <summary>
        /// Creates and returns an action descriptor mapping for the specified controller descriptor.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller descriptor</see> to create a mapping for.</param>
        /// <returns>A <see cref="ILookup{TKey,TValue}">lookup</see>, which represents the route-to-action mapping for the
        /// specified <paramref name="controllerDescriptor">controller descriptor</paramref>.</returns>
        public virtual ILookup<string, HttpActionDescriptor> GetActionMapping( HttpControllerDescriptor controllerDescriptor )
        {
            Arg.NotNull( controllerDescriptor, nameof( controllerDescriptor ) );
            Contract.Ensures( Contract.Result<ILookup<string, HttpActionDescriptor>>() != null );

            var actionMappings = ( from descriptor in controllerDescriptor.AsEnumerable()
                                   let selector = GetInternalSelector( descriptor )
                                   select selector.GetActionMapping() ).ToArray();

            return actionMappings.Length == 1 ? actionMappings[0] : new AggregatedActionMapping( actionMappings );
        }
    }
}