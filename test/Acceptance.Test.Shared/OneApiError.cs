#if WEBAPI
namespace Microsoft.Web
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
    using System;

    public class OneApiError
    {
        public string Code { get; set; }

        public string Message { get; set; }

        public OneApiInnerError InnerError { get; set; }
    }
}