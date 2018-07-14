namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using static System.Globalization.CultureInfo;
    using static System.String;

    partial class ODataRouteBuilder
    {
        internal string BuildPath()
        {
            var segments = new List<string>();
            AppendEntitySetOrOperation( segments );
            return Join( "/", segments );
        }

        string UpdateRoutePrefixAndRemoveApiVersionParameterIfNecessary( string routePrefix )
        {
            Contract.Requires( !IsNullOrEmpty( routePrefix ) );
            Contract.Ensures( !IsNullOrEmpty( Contract.Result<string>() ) );

            var parameters = Context.ParameterDescriptions;
            var parameter = parameters.FirstOrDefault( pd => pd.Source == BindingSource.Path && pd.ModelMetadata?.DataTypeName == nameof( ApiVersion ) );

            if ( parameter == null )
            {
                return routePrefix;
            }

            var apiVersionFormat = Context.Options.SubstitutionFormat;
            var token = Concat( '{', parameter.Name, '}' );
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