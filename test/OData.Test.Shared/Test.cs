namespace Microsoft
{
#if !WEBAPI
    using Microsoft.AspNet.OData.Builder;
#endif
    using Microsoft.OData.Edm;
    using System;
#if WEBAPI
    using System.Web.OData.Builder;
#endif

    internal static class Test
    {
        static Test()
        {
            var builder = new ODataModelBuilder();
            var tests = builder.EntitySet<TestEntity>( "Tests" ).EntityType;

            tests.HasKey( t => t.Id );
            Model = builder.GetEdmModel();
        }

        internal static IEdmModel Model { get; }

        internal static IEdmModel EmptyModel { get; } = new ODataModelBuilder().GetEdmModel();
    }
}