namespace Microsoft.Web.Http
{
    using Microsoft.Web.Http.Versioning;
    using System;
    using System.Web.Http;
    using System.Web.Http.Filters;

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
        public override void OnActionExecuted( HttpActionExecutedContext actionExecutedContext )
        {
            if ( actionExecutedContext == null )
            {
                throw new ArgumentNullException( nameof( actionExecutedContext ) );
            }

            var response = actionExecutedContext.Response;

            if ( response == null )
            {
                return;
            }

            var action = actionExecutedContext.ActionContext.ActionDescriptor;
            var model = action.GetApiVersionModel();

            if ( model.IsApiVersionNeutral )
            {
                return;
            }

            var dependencyResolver = actionExecutedContext.ActionContext.ControllerContext.Configuration.DependencyResolver;
            var reporter = ( (IReportApiVersions) dependencyResolver.GetService( typeof( IReportApiVersions ) ) ) ?? DefaultApiVersionReporter.Instance;

            reporter.Report( response.Headers, model );
        }
    }
}