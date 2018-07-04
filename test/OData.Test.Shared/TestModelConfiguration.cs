namespace Microsoft
{
    using Microsoft.AspNet.OData.Builder;
#if WEBAPI
    using Microsoft.Web.Http;
#else
    using Microsoft.AspNetCore.Mvc;
#endif
    using System;

    public class TestModelConfiguration : IModelConfiguration
    {
        public void Apply( ODataModelBuilder builder, ApiVersion apiVersion )
        {
            var tests = builder.EntitySet<TestEntity>( "Tests" ).EntityType;
            var neutralTests = builder.EntitySet<TestNeutralEntity>( "NeutralTests" ).EntityType;

            tests.HasKey( t => t.Id );
            neutralTests.HasKey( t => t.Id );
        }
    }
}