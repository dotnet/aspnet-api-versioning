namespace Microsoft.Web.Http
{
    using Microsoft.Web.Http.Versioning;
    using System.Diagnostics.CodeAnalysis;
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

            if ( model?.IsApiVersionNeutral == false )
            {
                var dependencyResolver = actionExecutedContext.ActionContext.ControllerContext.Configuration.DependencyResolver;
                var reporter = ( (IReportApiVersions) dependencyResolver.GetService( typeof( IReportApiVersions ) ) ) ?? DefaultApiVersionReporter.Instance;

                reporter.Report( response.Headers, model );
            }
        }
    }
}