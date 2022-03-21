﻿namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using System;

    /// <content>
    /// Provides additional implementation specific ASP.NET Core.
    /// </content>
    public partial class OriginalControllerNameConvention
    {
        /// <inheritdoc />
        public virtual string NormalizeName( string controllerName ) => controllerName; // already normalized
    }
}