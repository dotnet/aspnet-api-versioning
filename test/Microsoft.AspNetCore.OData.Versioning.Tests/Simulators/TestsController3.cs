namespace Microsoft.Simulators
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion( "3.0" )]
    [ApiVersion( "3.0-Beta", Deprecated = true )]
    [ODataRoutePrefix( "Tests" )]
    public class TestsController3 : ODataController
    {
        [ODataRoute]
        public IActionResult Get() => Ok();

        [ODataRoute( "({id})" )]
        public IActionResult Get( int id ) => Ok();
    }
}