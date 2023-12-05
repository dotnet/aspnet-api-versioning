// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if NETFRAMEWORK
using Microsoft.AspNet.OData.Query;
using System.Web.Http.Description;
using ControllerActionDescriptor = System.Web.Http.Controllers.ReflectedHttpActionDescriptor;
#else
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
#endif

internal sealed class ODataControllerQueryOptionConvention : IODataQueryOptionsConvention
{
    private readonly ODataActionQueryOptionConventionLookup lookup;
    private readonly ODataQueryOptionSettings settings;

    internal ODataControllerQueryOptionConvention(
        ODataActionQueryOptionConventionLookup lookup,
        ODataQueryOptionSettings settings )
    {
        this.lookup = lookup;
        this.settings = settings;
    }

    public void ApplyTo( ApiDescription apiDescription )
    {
        if ( apiDescription.ActionDescriptor is not ControllerActionDescriptor action )
        {
            return;
        }

        if ( !lookup( action.MethodInfo, settings, out var convention ) )
        {
            convention = ImplicitActionConvention( settings );
        }

        convention!.ApplyTo( apiDescription );
    }

    private static ODataValidationSettingsConvention ImplicitActionConvention( ODataQueryOptionSettings settings )
    {
        var validationSettings = new ODataValidationSettings()
        {
            AllowedArithmeticOperators = AllowedArithmeticOperators.None,
            AllowedFunctions = AllowedFunctions.None,
            AllowedLogicalOperators = AllowedLogicalOperators.None,
            AllowedQueryOptions = AllowedQueryOptions.None,
        };

        return new( validationSettings, settings );
    }
}