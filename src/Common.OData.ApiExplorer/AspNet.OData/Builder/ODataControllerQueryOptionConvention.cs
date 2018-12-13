namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNet.OData.Query;
#if !WEBAPI
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Controllers;
#endif
    using System.Diagnostics.Contracts;
#if WEBAPI
    using System.Web.Http.Description;
    using ControllerActionDescriptor = System.Web.Http.Controllers.ReflectedHttpActionDescriptor;
#endif

    sealed class ODataControllerQueryOptionConvention : IODataQueryOptionsConvention
    {
        readonly ODataActionQueryOptionConventionLookup lookup;
        readonly ODataQueryOptionSettings settings;

        internal ODataControllerQueryOptionConvention(
            ODataActionQueryOptionConventionLookup lookup,
            ODataQueryOptionSettings settings )
        {
            Contract.Requires( lookup != null );
            Contract.Requires( lookup != null );
            Contract.Requires( settings != null );

            this.lookup = lookup;
            this.settings = settings;
        }

        public void ApplyTo( ApiDescription apiDescription )
        {
            Arg.NotNull( apiDescription, nameof( apiDescription ) );

            if ( !( apiDescription.ActionDescriptor is ControllerActionDescriptor action ) )
            {
                return;
            }

            if ( !lookup( action.MethodInfo, settings, out var convention ) )
            {
                convention = ImplicitActionConvention( settings );
            }

            convention.ApplyTo( apiDescription );
        }

        static IODataQueryOptionsConvention ImplicitActionConvention( ODataQueryOptionSettings settings )
        {
            Contract.Requires( settings != null );
            Contract.Ensures( Contract.Result<IODataQueryOptionsConvention>() != null );

            var validationSettings = new ODataValidationSettings()
            {
                AllowedArithmeticOperators = AllowedArithmeticOperators.None,
                AllowedFunctions = AllowedFunctions.None,
                AllowedLogicalOperators = AllowedLogicalOperators.None,
                AllowedQueryOptions = AllowedQueryOptions.None,
            };

            return new ODataValidationSettingsConvention( validationSettings, settings );
        }
    }
}