// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.Extensions.Options;
using System.Globalization;

// ApiVersion.Neutral does not have the same meaning as IApiVersionNeutral. setting
// ApiVersioningOptions.DefaultApiVersion this value will not make all APIs version-neutral
// and will likely lead to many unexpected side effects. this is a best-effort, one-time
// validation check to help prevent people from going off the rails. if someone bypasses
// this validation by removing the check or updating the value later, then caveat emptor.
//
// REF: https://github.com/dotnet/aspnet-api-versioning/issues/1011
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
internal sealed class ValidateApiVersioningOptions : IValidateOptions<ApiVersioningOptions>
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
{
    public ValidateOptionsResult Validate( string? name, ApiVersioningOptions options )
    {
        if ( name is not null && name != Options.DefaultName )
        {
            return ValidateOptionsResult.Skip;
        }

        if ( options.DefaultApiVersion == ApiVersion.Neutral )
        {
            var message = string.Format(
                CultureInfo.CurrentCulture,
                Format.InvalidDefaultApiVersion,
                nameof( ApiVersion ),
                nameof( ApiVersion.Neutral ),
                nameof( ApiVersioningOptions ),
                nameof( ApiVersioningOptions.DefaultApiVersion ),
                nameof( IApiVersionNeutral ) );
            return ValidateOptionsResult.Fail( message );
        }

        return ValidateOptionsResult.Success;
    }
}