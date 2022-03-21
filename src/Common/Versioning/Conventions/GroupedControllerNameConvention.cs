﻿#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using static ControllerNameConvention;

    /// <summary>
    /// Represents the grouped <see cref="IControllerNameConvention">controller name convention</see>.
    /// </summary>
    /// <remarks>This convention will apply the original convention which strips the <b>Controller</b> suffix from the
    /// controller name. Any trailing numbers will also be stripped from controller name, but only for the purposes
    /// of grouping.</remarks>
    public class GroupedControllerNameConvention : OriginalControllerNameConvention
    {
        /// <inheritdoc />
        public override string GroupName( string controllerName ) => TrimTrailingNumbers( controllerName );
    }
}