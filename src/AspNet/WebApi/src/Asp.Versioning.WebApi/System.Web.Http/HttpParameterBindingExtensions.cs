// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace System.Web.Http;

using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;

internal static class HttpParameterBindingExtensions
{
    extension( HttpParameterBinding parameterBinding )
    {
        internal bool WillReadUri
        {
            get
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
    }
}