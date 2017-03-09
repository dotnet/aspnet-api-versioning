namespace Microsoft.Web.Http
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Web.Http;
    using System.Web.Http.Filters;
    using static System.String;

    /// <content>
    /// Provides the implementation for ASP.NET Web API.
    /// </content>
    public partial class ReportApiVersionsAttribute
    {
        /// <summary>
        /// Occurs after the controller action has executed.
        /// </summary>
        /// <param name="actionExecutedContext">The <see cref="HttpActionExecutedContext">HTTP action context</see> that executed.</param>
        /// <remarks>This method will write the "api-supported-versions" and "api-deprecated-versions" HTTP headers into the
        /// response provided that there is a response and the executed action was not version-neutral.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "The framework will never supply a null parameter." )]
        public override void OnActionExecuted( HttpActionExecutedContext actionExecutedContext )
        {
            var response = actionExecutedContext.Response;

            if ( response == null )
            {
                return;
            }

            var controller = actionExecutedContext.ActionContext.ActionDescriptor.ControllerDescriptor;
            var model = controller.GetApiVersionModel();

            if ( model.IsApiVersionNeutral )
            {
                return;
            }

            var headers = response.Headers;

            AddApiVersionHeader( headers, ApiSupportedVersions, model.SupportedApiVersions );
            AddApiVersionHeader( headers, ApiDeprecatedVersions, model.DeprecatedApiVersions );
        }

        static void AddApiVersionHeader( HttpHeaders headers, string headerName, IReadOnlyList<ApiVersion> versions )
        {
            Contract.Requires( headers != null );
            Contract.Requires( !IsNullOrEmpty( headerName ) );
            Contract.Requires( versions != null );

            if ( versions.Count > 0 && !headers.Contains( headerName ) )
            {
                headers.Add( headerName, Join( ValueSeparator, versions.Select( v => v.ToString() ) ) );
            }
        }
    }
}