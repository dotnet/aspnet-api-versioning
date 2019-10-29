namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using System;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial interface IActionConventionBuilder<out T> where T : notnull
    {
    }
}