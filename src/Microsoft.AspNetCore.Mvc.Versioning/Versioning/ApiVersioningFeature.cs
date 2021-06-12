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
        ActionSelectionResult? selectionResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersioningFeature"/> class.
        /// </summary>
        /// <param name="context">The current <see cref="HttpContext">HTTP context</see>.</param>
        [CLSCompliant( false )]
        public ApiVersioningFeature( HttpContext context ) => this.context = context;

        /// <inheritdoc />
        public string? RouteParameter { get; set; }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public ActionSelectionResult SelectionResult => selectionResult ??= new();
    }
}