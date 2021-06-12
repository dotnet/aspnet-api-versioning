#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;

    /// <summary>
    /// Represents the original <see cref="IControllerNameConvention">controller name convention</see>.
    /// </summary>
    /// <remarks>This convention will apply the original convention which only strips the <b>Controller</b> suffix.</remarks>
    public partial class OriginalControllerNameConvention : IControllerNameConvention
    {
        /// <inheritdoc />
        public virtual string GroupName( string controllerName ) => controllerName;
    }
}