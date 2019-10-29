namespace Microsoft.Web.Http.Controllers
{
    using Microsoft.Web.Http.Routing;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Web.Http.Controllers;
    using System.Web.Http.Routing;

    /// <content>
    /// Provides additional content for the <see cref="ApiVersionActionSelector"/> class.
    /// </content>
    public partial class ApiVersionActionSelector
    {
        [DebuggerDisplay( "{DebuggerToString()}" )]
        sealed class CandidateActionWithParams
        {
            internal CandidateActionWithParams( CandidateAction candidateAction, ISet<string> parameters, IHttpRouteData routeDataSource )
            {
                CandidateAction = candidateAction;
                CombinedParameterNames = parameters;
                RouteDataSource = routeDataSource;
            }

            internal CandidateAction CandidateAction { get; }

            internal ISet<string> CombinedParameterNames { get; }

            internal IHttpRouteData RouteDataSource { get; }

            internal HttpActionDescriptor ActionDescriptor => CandidateAction.ActionDescriptor;

            string DebuggerToString()
            {
                var sb = new StringBuilder();

                sb.Append( CandidateAction.DebuggerToString() );

                if ( CombinedParameterNames.Count < 1 )
                {
                    return sb.ToString();
                }

                sb.Append( ", Params =" );

                foreach ( var param in CombinedParameterNames )
                {
                    sb.Append( ' ' );
                    sb.Append( param );
                }

                return sb.ToString();
            }
        }
    }
}