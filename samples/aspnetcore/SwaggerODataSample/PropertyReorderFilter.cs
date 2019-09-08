namespace Microsoft.Examples
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// Reorders schema properties according to DataMember.Order attribute
    /// </summary>
    public class PropertyReorderFilter : ISchemaFilter
    {
        /// <inheritdoc />
        public void Apply( Schema schema, SchemaFilterContext context )
        {
            if ( schema.Properties == null
                 || context.SystemType == null
                 || context.SystemType.GetCustomAttribute<DataContractAttribute>() == null ) return;

            schema.Properties = ReorderProperties( schema.Properties,
                context.SystemType.GetProperties().ExtractPropertyOrder() );
        }

        private static IDictionary<string, Schema> ReorderProperties( IDictionary<string, Schema> properties, IEnumerable<PropertyOrder> clrProps )
        {
            return properties
                .Join( clrProps, pair => pair.Key, info => info.Name, ( pair, info ) => new
                {
                    pair.Key,
                    info.Order,
                    pair.Value
                } )
                .OrderBy( p => p.Order )
                .ToDictionary( arg => arg.Key, arg => arg.Value );
        }
    }

    internal static class OrderExtension
    {
        internal static IEnumerable<PropertyOrder> ExtractPropertyOrder( this IEnumerable<PropertyInfo> props )
        {
            return props
                .Select( p => new { p, attr = p.GetCustomAttribute<DataMemberAttribute>() } )
                .Select( _ => new PropertyOrder
                {
                    Name = _.attr != null && _.attr.IsNameSetExplicitly
                        ? _.attr.Name
                        : _.p.Name.ToLowerInvariant(),
                    Order = _.attr?.Order >= 0 // if no Order defined in the attribute then it has value -1 
                        ? _.attr.Order
                        : _.p.MetadataToken // use original declaration order
                } );
        }
    }

    internal class PropertyOrder
    {
        public string Name { get; set; }
        public int Order { get; set; }
    }
}