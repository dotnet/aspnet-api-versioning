// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using static System.StringComparison;
using static System.Web.Http.Dispatcher.DefaultHttpControllerSelector;

/// <content>
/// Provides additional implementation specific to ASP.NET Web API.
/// </content>
public partial class OriginalControllerNameConvention
{
    /// <inheritdoc />
    public virtual string NormalizeName( string controllerName )
    {
        if ( string.IsNullOrEmpty( controllerName ) )
        {
            return controllerName;
        }

        var length = controllerName.Length;
        var suffixLength = ControllerSuffix.Length;

        if ( length <= suffixLength || !controllerName.EndsWith( ControllerSuffix, Ordinal ) )
        {
            return controllerName;
        }

        return controllerName.Substring( 0, length - suffixLength );
    }
}