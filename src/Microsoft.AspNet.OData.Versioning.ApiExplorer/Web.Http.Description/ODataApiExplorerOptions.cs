namespace Microsoft.Web.Http.Description
{
    using System.Web.Http;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Web API.
    /// </content>
    public partial class ODataApiExplorerOptions : ApiExplorerOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataApiExplorerOptions"/> class.
        /// </summary>
        /// <param name="configuration">The current <see cref="HttpConfiguration">configuration</see> associated with the options.</param>
        public ODataApiExplorerOptions( HttpConfiguration configuration ) : base( configuration ) { }
    }
}