namespace Microsoft.Web.Http.Description
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http.Description;

    static class ApiParameterDescriptionExtensions
    {
        internal static IEnumerable<PropertyInfo> GetBindableProperties( this ApiParameterDescription description ) =>
            description.ParameterDescriptor.ParameterType.GetBindableProperties();

        internal static bool CanConvertPropertiesFromString( this ApiParameterDescription description ) =>
            description.GetBindableProperties().All( p => TypeHelper.CanConvertFromString( p.PropertyType ) );
    }
}