namespace Microsoft
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.OData.Edm;

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