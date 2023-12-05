// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc.ApplicationModels;
#endif
using System.Globalization;
#if NETFRAMEWORK
using System.Web.Http.Controllers;
using ControllerModel = System.Web.Http.Controllers.HttpControllerDescriptor;
#endif

/// <summary>
/// Represents a convention which applies an API to a controller by its defined namespace.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public class VersionByNamespaceConvention : IControllerConvention
{
    private readonly NamespaceParser parser;

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionByNamespaceConvention"/> class.
    /// </summary>
    public VersionByNamespaceConvention() => parser = NamespaceParser.Default;

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionByNamespaceConvention"/> class.
    /// </summary>
    /// <param name="parser">The <see cref="NamespaceParser">parser</see> used by the convention.</param>
    public VersionByNamespaceConvention( NamespaceParser parser ) => this.parser = parser;

    /// <inheritdoc />
    public virtual bool Apply( IControllerConventionBuilder builder, ControllerModel controller )
    {
        ArgumentNullException.ThrowIfNull( builder );
        ArgumentNullException.ThrowIfNull( controller );

        var type = controller.ControllerType;
        var versions = parser.Parse( type );

        switch ( versions.Count )
        {
            case 0:
                return false;
            case 1:
                break;
            default:
                var message = string.Format( CultureInfo.CurrentCulture, MvcFormat.MultipleApiVersionsInferredFromNamespaces, type.Namespace );
                throw new InvalidOperationException( message );
        }

#if NETFRAMEWORK
        var deprecated = controller.GetCustomAttributes<ObsoleteAttribute>().Any();
#else
        var deprecated = controller.Attributes.OfType<ObsoleteAttribute>().Any();
#endif

        if ( deprecated )
        {
            builder.HasDeprecatedApiVersion( versions[0] );
        }
        else
        {
            builder.HasApiVersion( versions[0] );
        }

        return true;
    }
}