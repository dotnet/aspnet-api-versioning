namespace Microsoft.Simulators
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion( "2.0" )]
    [ODataRoutePrefix( "Tests" )]
    public class TestsController2 : ODataController
    {
        [ODataRoute]
        public IActionResult Get() => Ok();

        [ODataRoute( "({id})" )]
        public IActionResult Get( int id ) => Ok();
    }
}