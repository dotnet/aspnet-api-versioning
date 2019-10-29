namespace Microsoft.Web.Http.Dispatcher
{
    using System;
    using System.Web.Http.Controllers;

    sealed class ControllerSelectionResult
    {
        internal HttpControllerDescriptor? Controller { get; set; }

        internal string? ControllerName { get; set; }

        internal bool Succeeded => Controller != null;

        internal bool CouldMatchVersion => HasCandidates;

        internal bool HasCandidates { get; set; }

        internal ApiVersion? RequestedVersion { get; set; }
    }
}