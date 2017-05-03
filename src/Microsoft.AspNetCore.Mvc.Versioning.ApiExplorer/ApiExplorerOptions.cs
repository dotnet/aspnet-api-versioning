namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using System;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core.
    /// </content>
    public partial class ApiExplorerOptions
    {
        ApiVersion defaultApiVersion = ApiVersion.Default;

        /// <summary>
        /// Gets or sets the default API version applied to services that do not have explicit versions.
        /// </summary>
        /// <value>The default <see cref="ApiVersion">API version</see>. The default value is <see cref="ApiVersion.Default"/>.</value>
        public ApiVersion DefaultApiVersion
        {
            get => defaultApiVersion;
            set
            {
                Arg.NotNull( value, nameof( value ) );
                defaultApiVersion = value;
            }
        }
    }
}