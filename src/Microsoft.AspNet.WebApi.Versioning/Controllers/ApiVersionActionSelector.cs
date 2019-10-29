namespace Microsoft.Web.Http.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Services;
    using static Microsoft.Web.Http.Versioning.ApiVersionMapping;
    using static System.Threading.Interlocked;

    /// <summary>
    /// Represents the logic for selecting a versioned, action method.
    /// </summary>
    public partial class ApiVersionActionSelector : IHttpActionSelector
    {
        readonly object cacheKey = new object();
        ActionSelectorCacheItem? fastCache;

        /// <summary>
        /// Selects and returns the action descriptor to invoke given the provided controller context.
        /// </summary>
        /// <param name="controllerContext">The current <see cref="HttpControllerContext">controller context</see>.</param>
        /// <returns>The <see cref="HttpActionDescriptor">action descriptor</see> that matches the current
        /// <paramref name="controllerContext">controller context</paramref>.</returns>
        public virtual HttpActionDescriptor? SelectAction( HttpControllerContext controllerContext )
        {
            if ( controllerContext == null )
            {
                throw new ArgumentNullException( nameof( controllerContext ) );
            }

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
            if ( controllerDescriptor == null )
            {
                throw new ArgumentNullException( nameof( controllerDescriptor ) );
            }

            var actionMappings = ( from descriptor in controllerDescriptor.AsEnumerable()
                                   let selector = GetInternalSelector( descriptor )
                                   select selector.GetActionMapping() ).ToArray();

            return actionMappings.Length == 1 ? actionMappings[0] : new AggregatedActionMapping( actionMappings );
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
        protected virtual HttpActionDescriptor? SelectActionVersion( HttpControllerContext controllerContext, IReadOnlyList<HttpActionDescriptor> candidateActions )
        {
            if ( controllerContext == null )
            {
                throw new ArgumentNullException( nameof( controllerContext ) );
            }

            if ( candidateActions == null )
            {
                throw new ArgumentNullException( nameof( candidateActions ) );
            }

            if ( candidateActions.Count == 0 )
            {
                return null;
            }

            var request = controllerContext.Request;
            var requestedVersion = request.GetRequestedApiVersion();

            if ( candidateActions.Count == 1 )
            {
                var action = candidateActions[0];
                return action.MappingTo( requestedVersion ) != None ? action : null;
            }

            var bestMatches = new List<HttpActionDescriptor>( candidateActions.Count );
            var implicitMatches = new List<HttpActionDescriptor>( bestMatches.Count );

            for ( var i = 0; i < candidateActions.Count; i++ )
            {
                var action = candidateActions[i];

                switch ( action.MappingTo( requestedVersion ) )
                {
                    case Explicit:
                        bestMatches.Add( action );
                        break;
                    case Implicit:
                        implicitMatches.Add( action );
                        break;
                }
            }

            switch ( bestMatches.Count )
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
                    return bestMatches[0];
                default:
                    throw CreateAmbiguousActionException( bestMatches );
            }

            return null;
        }

        static Exception CreateAmbiguousActionException( IEnumerable<HttpActionDescriptor> matches )
        {
            var ambiguityList = ActionSelectorCacheItem.CreateAmbiguousMatchList( matches );
            return new InvalidOperationException( SR.ApiControllerActionSelector_AmbiguousMatch.FormatDefault( ambiguityList ) );
        }

        ActionSelectorCacheItem GetInternalSelector( HttpControllerDescriptor controllerDescriptor )
        {
            controllerDescriptor = Decorator.GetInner( controllerDescriptor );

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
    }
}