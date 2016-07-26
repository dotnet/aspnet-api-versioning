namespace Microsoft.Web.OData.Routing
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using System.Web.OData;
    using System.Web.OData.Routing;
    using System.Web.OData.Routing.Conventions;

    /// <summary>
    /// Represents the <see cref="IODataRoutingConvention">OData routing convention</see> for versioned service and metadata documents.
    /// </summary>
    public class VersionedMetadataRoutingConvention : IODataRoutingConvention
    {
        /// <summary>
        /// Selects the controller for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="request">The request.</param>
        /// <returns>The name of the selected controller or <c>null</c> if the request isn't handled by this convention.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public virtual string SelectController( ODataPath odataPath, HttpRequestMessage request )
        {
            Arg.NotNull( odataPath, nameof( odataPath ) );
            Arg.NotNull( request, nameof( request ) );
            return odataPath.PathTemplate == "~" || odataPath.PathTemplate == "~/$metadata" ? "VersionedMetadata" : null;
        }

        /// <summary>
        /// Selects the action for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionMap">The action map.</param>
        /// <returns>The name of the selected action or <c>null</c> if the request isn't handled by this convention.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public virtual string SelectAction( ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap )
        {
            Arg.NotNull( odataPath, nameof( odataPath ) );
            Arg.NotNull( controllerContext, nameof( controllerContext ) );
            Arg.NotNull( actionMap, nameof( actionMap ) );

            if ( odataPath.PathTemplate == "~" )
            {
                return nameof( MetadataController.GetServiceDocument );
            }

            if ( odataPath.PathTemplate == "~/$metadata" )
            {
                return nameof( MetadataController.GetMetadata );
            }

            return null;
        }
    }
}
