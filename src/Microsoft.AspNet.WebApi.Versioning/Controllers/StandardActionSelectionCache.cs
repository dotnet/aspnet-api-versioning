namespace Microsoft.Web.Http.Controllers
{
    using Microsoft.Web.Http.Routing;
    using System.Linq;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional content for the <see cref="ApiVersionActionSelector"/> class.
    /// </content>
    public partial class ApiVersionActionSelector
    {
        sealed class StandardActionSelectionCache
        {
            internal ILookup<string, HttpActionDescriptor>? StandardActionNameMapping { get; set; }

            internal CandidateAction[]? StandardCandidateActions { get; set; }

            internal CandidateAction[][]? CacheListVerbs { get; set; }
        }
    }
}