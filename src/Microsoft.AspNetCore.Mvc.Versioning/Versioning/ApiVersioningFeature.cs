namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// Represents the API versioning feature.
    /// </summary>
    [CLSCompliant( false )]
    public sealed class ApiVersioningFeature : IApiVersioningFeature
    {
        readonly HttpContext context;
        string? rawApiVersion;
        ApiVersion? apiVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersioningFeature"/> class.
        /// </summary>
        /// <param name="context">The current <see cref="HttpContext">HTTP context</see>.</param>
        [CLSCompliant( false )]
        public ApiVersioningFeature( HttpContext context ) => this.context = context;

        /// <summary>
        /// Gets or sets the raw, unparsed API version for the current request.
        /// </summary>
        /// <value>The unparsed API version value for the current request.</value>
        public string? RawRequestedApiVersion
        {
            get
            {
                if ( rawApiVersion == null )
                {
                    var reader = context.RequestServices.GetService<IApiVersionReader>() ?? new QueryStringApiVersionReader();
                    rawApiVersion = reader.Read( context.Request );
                }

                return rawApiVersion;
            }
            set => rawApiVersion = value;
        }

        /// <summary>
        /// Gets or sets the API version for the current request.
        /// </summary>
        /// <value>The current <see cref="ApiVersion">API version</see> for the current request.</value>
        /// <remarks>If an API version was not provided for the current request or the value
        /// provided is invalid, this property will return <c>null</c>.</remarks>
        public ApiVersion? RequestedApiVersion
        {
            get
            {
                if ( apiVersion == null )
                {
#pragma warning disable CA1806 // Do not ignore method results
                    ApiVersion.TryParse( RawRequestedApiVersion, out apiVersion );
#pragma warning restore CA1806
                }

                return apiVersion;
            }
            set => apiVersion = value;
        }

        /// <summary>
        /// Gets the action selection result associated with the current request.
        /// </summary>
        /// <value>The <see cref="ActionSelectionResult">action selection result</see> associated with the current request.</value>
        public ActionSelectionResult SelectionResult { get; } = new ActionSelectionResult();
    }
}