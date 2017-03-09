#if WEBAPI
namespace Microsoft.Web
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
    using System;

    public class OneApiErrorResponse
    {
        public OneApiError Error { get; set; }
    }
}