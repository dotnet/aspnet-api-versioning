﻿namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using System;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Core.
    /// </content>
    /// <typeparam name="T">The <see cref="Type">type</see> of <see cref="ICommonModel">model</see>.</typeparam>
    [CLSCompliant( false )]
    public partial class ActionApiVersionConventionBuilder<T> : IActionConventionBuilder<T>
    {
    }
}