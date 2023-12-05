// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApplicationModels;

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Attributes;

/// <summary>
/// Represents a specification that matches API controllers if they use the OData protocol.
/// </summary>
[CLSCompliant( false )]
public sealed class ODataControllerSpecification : IApiControllerSpecification
{
    /// <inheritdoc />
    public bool IsSatisfiedBy( ControllerModel controller )
    {
        ArgumentNullException.ThrowIfNull( controller );

        if ( ODataControllerSpecification.IsSatisfiedBy( controller ) )
        {
            return true;
        }

        var actions = controller.Actions;

        for ( var i = 0; i < actions.Count; i++ )
        {
            if ( IsSatisfiedBy( actions[i] ) )
            {
                return true;
            }
        }

        return false;
    }

    internal static bool IsSatisfiedBy( ICommonModel model )
    {
        var attributes = model.Attributes;

        for ( var i = 0; i < attributes.Count; i++ )
        {
            if ( attributes[i] is ODataAttributeRoutingAttribute )
            {
                return true;
            }
        }

        return false;
    }
}