namespace Microsoft.AspNetCore.Mvc.Routing
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using System;
    using System.Collections.Generic;

    public class ApiVersionRouteCandidates
    {
        public ICollection<ActionDescriptor> Actions { get; } = new HashSet<ActionDescriptor>();

        /// <summary>
        /// Gets the read-only list of controller actions matching the current route.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ActionDescriptor">actions</see> matching the current route.</value>
        public ICollection<ActionDescriptor> MatchingActions { get; } = new HashSet<ActionDescriptor>();

        /// <summary>
        /// Gets or sets the currently requested API version.
        /// </summary>
        /// <value>The currently requested <see cref="ApiVersion">API version</see>.</value>
        /// <remarks>This property may be <c>null</c> if the client did request an explicit version. Implementors should update this property when
        /// implicit API version matching is allowed and a version has been selected.</remarks>
        public ApiVersion RequestedVersion { get; set; }
    }
}