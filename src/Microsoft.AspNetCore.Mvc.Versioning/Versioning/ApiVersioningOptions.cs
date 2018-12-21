namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;

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
        /// <value>True to use web API behaviors; otherwise, false. The default value is <c>true</c>.</value>
        /// <remarks>When this property is set to <c>true</c>, API versioning policies only apply to controllers
        /// that remain after the <see cref="IApiControllerFilter"/> has been applied. When this property is set
        /// to <c>false</c>, API versioning policies are considers for all controllers. This was default behavior
        /// behavior in previous versions.</remarks>
        public bool UseApiBehavior { get; set; } = true;
    }
}