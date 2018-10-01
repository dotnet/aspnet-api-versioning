namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Builder;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core.
    /// </content>
    public partial class ApiVersioningOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the API versioning middleware
        /// should automatically be registered.
        /// </summary>
        /// <value>True if the API versioning middleware should be automatically
        /// registered; otherwise, false. The default value is <c>true</c>.</value>
        /// <remarks>If this property is set to false, then
        /// <see cref="IApplicationBuilderExtensions.UseApiVersioning(IApplicationBuilder)"/>
        /// must be called.</remarks>
        public bool RegisterMiddleware { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to use web API behaviors.
        /// </summary>
        /// <value>True to use web API behaviors; otherwise, false. The default value is <c>false</c>.</value>
        /// <remarks>Setting this property to <c>true</c> applies API versioning policies only to controllers that
        /// have the ApiControllerAttribute applied. A value of <c>true</c> is only effective when using ASP.NET Core
        /// 2.1 or above. The default value of <c>false</c> retains backward capability with the existing behaviors
        /// before the API behavior feature was introduced.</remarks>
        public bool UseApiBehavior { get; set; }
    }
}