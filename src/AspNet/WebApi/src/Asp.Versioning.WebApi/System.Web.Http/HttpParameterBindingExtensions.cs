// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http;

using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;

internal static class HttpParameterBindingExtensions
{
    internal static bool WillReadUri( this HttpParameterBinding parameterBinding )
    {
        if ( parameterBinding is not IValueProviderParameterBinding valueProviderParameterBinding )
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