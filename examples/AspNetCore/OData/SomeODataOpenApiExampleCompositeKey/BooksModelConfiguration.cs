using Asp.Versioning;
using Asp.Versioning.OData;
using Microsoft.OData.ModelBuilder;

namespace ApiVersioning.Examples
{
    public class BooksModelConfiguration : IModelConfiguration
    {
        private void ConfigureV1(ODataModelBuilder builder) => ConfigureCurrent(builder);

        private EntityTypeConfiguration<Book> ConfigureCurrent(ODataModelBuilder builder)
        {
            var model = builder.EntitySet<Book>("Books").EntityType;
            model.HasKey(p => p.IdFirst);
            model.HasKey(p => p.IdSecond);

            return model;
        }

        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix)
        {
            switch (apiVersion.MajorVersion)
            {
                case 1:
                    ConfigureV1(builder);
                    break;
                default:
                    ConfigureCurrent(builder);
                    break;
            }
        }
    }
}
