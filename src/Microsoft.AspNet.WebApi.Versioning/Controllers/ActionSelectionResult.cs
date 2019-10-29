namespace Microsoft.Web.Http.Controllers
{
    using System;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional content for the <see cref="ApiVersionActionSelector"/> class.
    /// </content>
    public partial class ApiVersionActionSelector
    {
        sealed class ActionSelectionResult
        {
            internal ActionSelectionResult( HttpActionDescriptor action ) => Action = action;

            internal ActionSelectionResult( Exception exception ) => Exception = exception;

            internal bool Succeeded => Exception == null;

            internal HttpActionDescriptor? Action { get; }

            internal Exception? Exception { get; }
        }
    }
}