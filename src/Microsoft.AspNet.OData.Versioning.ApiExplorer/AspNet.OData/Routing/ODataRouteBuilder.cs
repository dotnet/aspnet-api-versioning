namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.Web.Http.Description;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using static System.Globalization.CultureInfo;

    partial class ODataRouteBuilder
    {
        string UpdateRoutePrefixAndRemoveApiVersionParameterIfNecessary( string routePrefix )
        {
            Contract.Requires( !string.IsNullOrEmpty( routePrefix ) );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            var parameters = Context.ParameterDescriptions;
            var parameter = parameters.FirstOrDefault( p => p.ParameterDescriptor is ApiVersionParameterDescriptor pd && pd.FromPath );

            if ( parameter == null )
            {
                return routePrefix;
            }

            var apiVersionFormat = Context.Options.SubstitutionFormat;
            var token = string.Concat( '{', parameter.Name, '}' );
            var value = Context.ApiVersion.ToString( apiVersionFormat, InvariantCulture );
            var newRoutePrefix = routePrefix.Replace( token, value );

            if ( routePrefix == newRoutePrefix )
            {
                return routePrefix;
            }

            parameters.Remove( parameter );
            return newRoutePrefix;
        }
    }
}