namespace Microsoft.Web.Http.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Http.Controllers;
    using static System.Environment;

    sealed class DirectRouteControllerSelector : ControllerSelector
    {
        public override ControllerSelectionResult SelectController( ControllerSelectionContext context )
        {
            var request = context.Request;
            var requestedVersion = context.RequestedVersion;
            var result = new ControllerSelectionResult()
            {
                HasCandidates = context.HasAttributeBasedRoutes,
                RequestedVersion = requestedVersion,
            };

            if ( !result.HasCandidates )
            {
                return result;
            }

            var bestMatches = SelectBestCandidates( context.DirectRouteCandidates!, requestedVersion );

            switch ( bestMatches.Count )
            {
                case 0:
                    break;
                case 1:
                    result.Controller = bestMatches.Single();
                    break;
                default:
                    if ( TryDisambiguateControllerByAction( request, bestMatches, out var resolvedController ) )
                    {
                        result.Controller = resolvedController;
                        break;
                    }

                    throw CreateAmbiguousControllerException( bestMatches );
            }

            return result;
        }

        static Exception CreateAmbiguousControllerException( IEnumerable<HttpControllerDescriptor> candidates )
        {
            var set = new HashSet<Type>( candidates.Select( c => c.ControllerType ) );
            var builder = new StringBuilder();

            foreach ( var type in set )
            {
                builder.AppendLine();
                builder.Append( type.FullName );
            }

            return new InvalidOperationException( SR.DirectRoute_AmbiguousController.FormatDefault( builder, NewLine ) );
        }
    }
}