namespace Microsoft.AspNetCore.Mvc
{
    using Abstractions;
    using ApplicationModels;
    using Filters;
    using Http;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Versioning;
    using static System.String;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class ReportApiVersionsAttribute
    {
        /// <summary>
        /// Reports the discovered service API versions for the given context after an action has executed.
        /// </summary>
        /// <param name="context">The <see cref="ActionExecutedContext">context</see> for the executed action.</param>
        /// <remarks>This method will write the "api-supported-versions" and "api-deprecated-versions" HTTP headers into the
        /// response provided that there is a response and the executed action was not version-neutral.</remarks>
        public override void OnActionExecuted( ActionExecutedContext context )
        {
            var response = context.HttpContext.Response;

            if ( response == null )
            {
                return;
            }

            var model = context.ActionDescriptor.GetProperty<ApiVersionModel>();

            if ( model == null || model.IsApiVersionNeutral )
            {
                return;
            }

            var headers = response.Headers;

            AddApiVersionHeader( headers, ApiSupportedVersions, model.SupportedApiVersions );
            AddApiVersionHeader( headers, ApiDeprecatedVersions, model.DeprecatedApiVersions );
        }

        static void AddApiVersionHeader( IHeaderDictionary headers, string headerName, IReadOnlyList<ApiVersion> versions )
        {
            Contract.Requires( headers != null );
            Contract.Requires( !IsNullOrEmpty( headerName ) );
            Contract.Requires( versions != null );

            if ( versions.Count > 0 && !headers.ContainsKey( headerName ) )
            {
                headers.Add( headerName, Join( ValueSeparator, versions.Select( v => v.ToString() ).ToArray() ) );
            }
        }
    }
}