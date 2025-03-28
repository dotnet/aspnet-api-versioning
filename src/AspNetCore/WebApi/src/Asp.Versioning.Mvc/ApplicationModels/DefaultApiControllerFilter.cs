﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApplicationModels;

using Microsoft.AspNetCore.Mvc.ApplicationModels;

/// <summary>
/// Represents the default API controller filter.
/// </summary>
[CLSCompliant( false )]
public sealed class DefaultApiControllerFilter : IApiControllerFilter
{
    private readonly IApiControllerSpecification[] specifications;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultApiControllerFilter"/> class.
    /// </summary>
    /// <param name="specifications">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IApiControllerSpecification">specifications</see> used by the filter
    /// to identify API controllers.</param>
    public DefaultApiControllerFilter( IEnumerable<IApiControllerSpecification> specifications ) =>
        this.specifications = specifications.ToArray();

    /// <inheritdoc />
    public IList<ControllerModel> Apply( IList<ControllerModel> controllers )
    {
        if ( specifications.Length == 0 )
        {
            return controllers;
        }

        var filtered = controllers.ToList();

        for ( var i = filtered.Count - 1; i >= 0; i-- )
        {
            if ( !IsApiController( filtered[i] ) )
            {
                filtered.RemoveAt( i );
            }
        }

        return filtered;
    }

    private bool IsApiController( ControllerModel controller )
    {
        for ( var i = 0; i < specifications.Length; i++ )
        {
            if ( specifications[i].IsSatisfiedBy( controller ) )
            {
                return true;
            }
        }

        return false;
    }
}