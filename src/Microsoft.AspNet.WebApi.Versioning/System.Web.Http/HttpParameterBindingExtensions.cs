namespace System.Web.Http
{
    using System.Linq;
    using System.Web.Http.Controllers;
    using System.Web.Http.ModelBinding;
    using System.Web.Http.ValueProviders;

    static class HttpParameterBindingExtensions
    {
        internal static bool WillReadUri( this HttpParameterBinding parameterBinding )
        {
            if ( !( parameterBinding is IValueProviderParameterBinding valueProviderParameterBinding ) )
            {
                return false;
            }

            var valueProviderFactories = valueProviderParameterBinding.ValueProviderFactories;

            if ( valueProviderFactories.Any() && valueProviderFactories.All( factory => factory is IUriValueProviderFactory ) )
            {
                return true;
            }

            return false;
        }
    }
}