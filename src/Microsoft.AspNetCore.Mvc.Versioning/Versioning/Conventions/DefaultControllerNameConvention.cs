namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using System;
    using static ControllerNameConvention;

    /// <summary>
    /// Represents the default <see cref="IControllerNameConvention">controller name convention</see>.
    /// </summary>
    /// <remarks>This convention will strip the <b>Controller</b> suffix as well as any trailing numeric values.</remarks>
    public class DefaultControllerNameConvention : OriginalControllerNameConvention
    {
        /// <inheritdoc />
        public override string NormalizeName( string controllerName ) => TrimTrailingNumbers( base.NormalizeName( controllerName ) );
    }
}