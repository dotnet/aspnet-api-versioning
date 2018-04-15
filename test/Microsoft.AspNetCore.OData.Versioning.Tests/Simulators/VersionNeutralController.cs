namespace Microsoft.Simulators
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersionNeutral]
    [ODataRoutePrefix( "Tests" )]
    public class VersionNeutralController : ODataController
    {
        [ODataRoute]
        public IActionResult Get() => Ok();

        [ODataRoute( "({id})" )]
        public IActionResult Get( int id ) => Ok();
    }
}
