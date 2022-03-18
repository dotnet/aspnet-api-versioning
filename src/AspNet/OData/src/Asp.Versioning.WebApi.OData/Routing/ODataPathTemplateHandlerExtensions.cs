// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNet.OData.Routing;

using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.OData;

internal static class ODataPathTemplateHandlerExtensions
{
    internal static ODataPathTemplate? SafeParseTemplate(
        this IODataPathTemplateHandler handler,
        string pathTemplate,
        IServiceProvider serviceProvider )
    {
        try
        {
            return handler.ParseTemplate( pathTemplate, serviceProvider );
        }
        catch ( ODataException )
        {
            // this 'should' mean the controller does not map to the current edm model. there's no way to know this without
            // forcing a developer to explicitly map it. while it could be a mistake, simply yield null. this results in the
            // template being skipped and will ultimately result in a 4xx if requested, which is acceptable.
            return default;
        }
    }
}