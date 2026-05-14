// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Provides extension methods for <see cref="IEndpointConventionBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public static class IEndpointConventionBuilderExtensions
{
    extension( IEndpointConventionBuilder builder )
    {
        /// <summary>
        /// Enables generating one OpenAPI document per APi Version for the associated endpoint builder.
        /// </summary>
        /// <remarks>
        /// This method is only intended to apply API Versioning conventions the OpenAPI endpoint. Applying this
        /// method to other endpoints may have unintended effects.
        /// </remarks>
        /// <returns>The original <see cref="IEndpointConventionBuilder">endpoint convention builder</see>.</returns>
        public IEndpointConventionBuilder WithDocumentPerVersion()
        {
            return builder;
        }
    }
}