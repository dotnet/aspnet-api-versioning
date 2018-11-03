namespace System.Web.Http.Description
{
    using Microsoft.Web.Http;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    static class ApiParameterDescriptionExtensions
    {
        internal static IEnumerable<PropertyInfo> GetBindableProperties( this ApiParameterDescription description ) =>
            description.ParameterDescriptor.ParameterType.GetBindableProperties();

        internal static bool CanConvertPropertiesFromString( this ApiParameterDescription description ) =>
            description.GetBindableProperties().All( p => p.PropertyType.CanConvertFromString() );
    }
}