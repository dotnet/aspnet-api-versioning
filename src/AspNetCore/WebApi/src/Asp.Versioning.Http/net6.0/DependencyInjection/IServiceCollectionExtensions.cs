// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning;
using Microsoft.AspNetCore.Http;

/// <content>
/// Provides additional implementation specific to .NET 6.0.
/// </content>
public static partial class IServiceCollectionExtensions
{
    /// <summary>
    /// Enables binding the <see cref="ApiVersion"/> type in Minimal API parameters..
    /// </summary>
    /// <param name="builder">The extended <see cref="IApiVersioningBuilder">API versioning builder</see>.</param>
    /// <returns>The original <paramref name="builder"/>.</returns>
    public static IApiVersioningBuilder EnableApiVersionBinding( this IApiVersioningBuilder builder )
    {
        if ( builder == null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        // currently required because there is no other hook.
        // 1. TryParse does not work because:
        //    a. Parsing is delegated to IApiVersionParser.TryParse
        //    b. The result can come from multiple locations
        //    c. There can be multiple results
        // 2. BindAsync does not work because:
        //    a. It is static and must be on the ApiVersion type
        //    b. It is specific to ASP.NET Core
        builder.Services.AddHttpContextAccessor();

        // this registration is 'truthy'. it is possible for the requested API version to be null; however, but the time this is
        // resolved for a request delegate it can only be null if the API is version-neutral and no API version was requested. this
        // should be a rare and nonsensical scenario. declaring the parameter as ApiVersion? should be expect and solve the issue
        //
        // it should also be noted that this registration allows resolving the requested API version from virtually any context.
        // that is not intended, which is why this extension is not named something more general such as AddApiVersionAsService.
        // if/when a better parameter binding mechanism becomes available, this method is expected to become obsolete, no-op, and
        // eventually go away.
        builder.Services.AddTransient( sp => sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.GetRequestedApiVersion()! );

        return builder;
    }
}