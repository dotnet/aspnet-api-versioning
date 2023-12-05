// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApplicationModels;

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Reflection;

/// <summary>
/// Represents a specification that matches API controllers by the presence of API behaviors.
/// </summary>
[CLSCompliant( false )]
public sealed class ApiBehaviorSpecification : IApiControllerSpecification
{
    /// <inheritdoc />
    public bool IsSatisfiedBy( ControllerModel controller )
    {
        ArgumentNullException.ThrowIfNull( controller );

        // REF: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/ApplicationModels/ApiBehaviorApplicationModelProvider.cs
        if ( controller.Attributes.OfType<IApiBehaviorMetadata>().Any() )
        {
            return true;
        }

        var assembly = controller.ControllerType.Assembly;

        return assembly.GetCustomAttributes().OfType<IApiBehaviorMetadata>().Any();
    }
}