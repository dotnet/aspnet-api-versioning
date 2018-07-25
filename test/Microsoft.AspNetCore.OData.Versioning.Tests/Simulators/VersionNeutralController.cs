namespace Microsoft.Simulators
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;

    [ApiVersionNeutral]
    [ODataRoutePrefix( "NeutralTests" )]
    public class VersionNeutralController : ODataController
    {
        [ODataRoute]
        public IActionResult Get() => Ok( new[] { new TestNeutralEntity() { Id = 1 }, new TestNeutralEntity() { Id = 2 }, new TestNeutralEntity() { Id = 3 } } );

        [ODataRoute( "({id})" )]
        public IActionResult Get( int id ) => Ok( new TestNeutralEntity() { Id = id } );
    }
}
