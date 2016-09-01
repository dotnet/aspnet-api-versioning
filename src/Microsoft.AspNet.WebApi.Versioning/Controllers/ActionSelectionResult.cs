namespace Microsoft.Web.Http.Controllers
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional content for the <see cref="ApiVersionActionSelector"/> class.
    /// </content>
    public partial class ApiVersionActionSelector
    {
        private sealed class ActionSelectionResult
        {
            internal ActionSelectionResult( HttpActionDescriptor action )
            {
                Contract.Requires( action != null );
                Action = action;
            }

            internal ActionSelectionResult( Exception exception )
            {
                Contract.Requires( exception != null );
                Exception = exception;
            }

            internal bool Succeeded => Exception == null;

            internal HttpActionDescriptor Action { get; }

            internal Exception Exception { get; }
        }
    }
}
