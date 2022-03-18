// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http;

using System.Net.Http.Formatting;

internal static class MediaTypeFormatterExtensions
{
    internal static MediaTypeFormatter Clone( this MediaTypeFormatter formatter ) =>
        MediaTypeFormatterAdapterFactory.GetOrCreateCloneFunction( formatter )( formatter );
}