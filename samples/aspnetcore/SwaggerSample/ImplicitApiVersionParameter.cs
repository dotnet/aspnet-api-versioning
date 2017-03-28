namespace Microsoft.Examples
{
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents the Swagger/Swashbuckle operation filter used to document the implicit API version parameter.
    /// </summary>
    public class ImplicitApiVersionParameter : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the specified operation using the given context.
        /// </summary>
        /// <param name="operation">The operation to apply the filter to.</param>
        /// <param name="context">The current operation filter context.</param>
        public void Apply( Operation operation, OperationFilterContext context )
        {
            var apiVersion = context.ApiDescription.GetApiVersion();

            // if the api explorer did not capture an API version for this operation
            // then the action must be API version-neutral; there's nothing to add
            if ( apiVersion == null )
            {
                return;
            }

            var parameters = operation.Parameters;

            if ( parameters == null )
            {
                operation.Parameters = parameters = new List<IParameter>();
            }

            // note: in most applications, service authors will choose a single, consistent
            // approach to how API versioning is applied. this sample uses a:
            //
            // 1. query string parameter method with the name "api-version"
            // 2. url path segement with the route parameter name "api-version"
            //
            // unless you allow multiple API versioning methods in your application,
            // your implementation should be even simpler.

            // consider the url path segment parameter first
            var parameter = parameters.SingleOrDefault( p => p.Name == "api-version" );

            if ( parameter == null )
            {
                // the only other method in this sample is by query string
                parameter = new NonBodyParameter()
                {
                    Name = "api-version",
                    Required = true,
                    Default = apiVersion.ToString(),
                    Description = "The requested API version",
                    In = "query",
                    Type = "string"
                };

                parameters.Add( parameter );
            }
            else if ( parameter is NonBodyParameter pathParameter )
            {
                // update the default value with the current API version so that
                // the route can be invoked in the "Try It!" feature
                pathParameter.Default = apiVersion.ToString();
            }
        }
    }
}