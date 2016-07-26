namespace System.Web.Http
{
    using Controllers;
    using Diagnostics.Contracts;
    using Linq;
    using ModelBinding;
    using ValueProviders;

    internal static class HttpParameterBindingExtensions
    {
        internal static bool WillReadUri( this HttpParameterBinding parameterBinding )
        {
            Contract.Requires( parameterBinding != null );

            var valueProviderParameterBinding = parameterBinding as IValueProviderParameterBinding;

            if ( valueProviderParameterBinding == null )
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
