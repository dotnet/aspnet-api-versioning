#if WEBAPI
namespace Microsoft.Web
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
    using System;

    public class OneApiInnerError
    {
        public string Code { get; set; }

        public string Message { get; set; }
    }
}