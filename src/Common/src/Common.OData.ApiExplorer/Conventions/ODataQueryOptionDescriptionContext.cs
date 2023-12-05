// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Asp.Versioning.OData;
using Microsoft.OData.Edm;
#if NETFRAMEWORK
using Microsoft.AspNet.OData.Query;
using System.Web.Http.Description;
#else
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
#endif

/// <summary>
/// Represents the context used to describe a query option.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public partial class ODataQueryOptionDescriptionContext
{
    private bool? scalar;
    private IEdmModel? model;
    private IEdmStructuredType? returnType;
    private List<string>? allowedSelectProperties;
    private List<string>? allowedExpandProperties;
    private List<string>? allowedFilterProperties;
    private List<string>? allowedOrderByProperties;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataQueryOptionDescriptionContext"/> class.
    /// </summary>
    /// <param name="apiDescription">The associated <see cref="ApiDescription">API description</see>.</param>
    public ODataQueryOptionDescriptionContext( ApiDescription apiDescription ) =>
        ApiDescription = apiDescription ?? throw new System.ArgumentNullException( nameof( apiDescription ) );

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataQueryOptionDescriptionContext"/> class.
    /// </summary>
    /// <param name="apiDescription">The associated <see cref="ApiDescription">API description</see>.</param>
    /// <param name="validationSettings">The <see cref="ODataValidationSettings">validation settings</see> to
    /// derive the description context from.</param>
    protected internal ODataQueryOptionDescriptionContext(
        ApiDescription apiDescription,
        ODataValidationSettings validationSettings )
    {
        ArgumentNullException.ThrowIfNull( apiDescription );
        ArgumentNullException.ThrowIfNull( validationSettings );

        ApiDescription = apiDescription;
        AllowedArithmeticOperators = validationSettings.AllowedArithmeticOperators;
        AllowedFunctions = validationSettings.AllowedFunctions;
        AllowedLogicalOperators = validationSettings.AllowedLogicalOperators;
        allowedOrderByProperties = [.. validationSettings.AllowedOrderByProperties];
        MaxOrderByNodeCount = validationSettings.MaxOrderByNodeCount;
        MaxAnyAllExpressionDepth = validationSettings.MaxAnyAllExpressionDepth;
        MaxNodeCount = validationSettings.MaxNodeCount;
        MaxSkip = validationSettings.MaxSkip;
        MaxTop = validationSettings.MaxTop;
        MaxExpansionDepth = validationSettings.MaxExpansionDepth;
    }

    /// <summary>
    /// Gets the associated API description.
    /// </summary>
    /// <value>The associated <see cref="ApiDescription">API description</see>.</value>
    public ApiDescription ApiDescription { get; }

    /// <summary>
    /// Gets or sets the allowed arithmetic operators.
    /// </summary>
    /// <value>One or more of the <see cref="AllowedArithmeticOperators"/> values.</value>
    public AllowedArithmeticOperators AllowedArithmeticOperators { get; set; }

    /// <summary>
    /// Gets or sets the allowed functions.
    /// </summary>
    /// <value>One or more of the <see cref="AllowedFunctions"/> values.</value>
    public AllowedFunctions AllowedFunctions { get; set; }

    /// <summary>
    /// Gets or sets the allowed logical operators.
    /// </summary>
    /// <value>One or more of the <see cref="AllowedLogicalOperators"/> values.</value>
    public AllowedLogicalOperators AllowedLogicalOperators { get; set; }

    /// <summary>
    /// Gets the names of properties that can be selected.
    /// </summary>
    /// <value>A <see cref="IList{T}">list</see> of selectable property names.</value>
    public IList<string> AllowedSelectProperties => allowedSelectProperties ??= [];

    /// <summary>
    /// Gets the names of properties that can be expanded.
    /// </summary>
    /// <value>A <see cref="IList{T}">list</see> of expandable property names.</value>
    public IList<string> AllowedExpandProperties => allowedExpandProperties ??= [];

    /// <summary>
    /// Gets the names of properties that can be filtered.
    /// </summary>
    /// <value>A <see cref="IList{T}">list</see> of filterable property names.</value>
    public IList<string> AllowedFilterProperties => allowedFilterProperties ??= [];

    /// <summary>
    /// Gets the names of properties that can be sorted.
    /// </summary>
    /// <value>A <see cref="IList{T}">list</see> of sortable property names.</value>
    public IList<string> AllowedOrderByProperties => allowedOrderByProperties ??= [];

    /// <summary>
    /// Gets or sets the maximum number of expressions that can be present in the $orderby query option.
    /// </summary>
    /// <value>The maximum number of expressions that can be present in the $orderby query option.</value>
    public int MaxOrderByNodeCount { get; set; }

    /// <summary>
    /// Gets or sets the maximum depth of the Any or All elements nested inside the query.
    /// </summary>
    /// <value>The maximum depth of the Any or All elements nested inside the query.</value>
    public int MaxAnyAllExpressionDepth { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of the nodes inside the $filter syntax tree.
    /// </summary>
    /// <value>The maximum number of the nodes inside the $filter syntax tree.</value>
    public int MaxNodeCount { get; set; }

    /// <summary>
    /// Gets or sets the max value of $skip that a client can request.
    /// </summary>
    /// <value>The max value of $skip that a client can request.</value>
    public int? MaxSkip { get; set; }

    /// <summary>
    /// Gets or sets the max value of $top that a client can request.
    /// </summary>
    /// <value>The max value of $top that a client can request.</value>
    public int? MaxTop { get; set; }

    /// <summary>
    /// Gets or sets the max expansion depth for the $expand query option.
    /// </summary>
    /// <value>The max expansion depth for the $expand query option.</value>
    public int MaxExpansionDepth { get; set; }

    /// <summary>
    /// Gets the Entity Data Model (EDM) associated with the context.
    /// </summary>
    /// <value>The associated <see cref="IEdmModel">EDM</see> or <c>null</c>.</value>
    public IEdmModel? Model => model ??= ResolveModel( ApiDescription );

    /// <summary>
    /// Gets the API return type.
    /// </summary>
    /// <value>The API <see cref="IEdmStructuredType">return type</see>, if any.</value>
    public IEdmStructuredType? ReturnType
    {
        get
        {
            if ( scalar.HasValue )
            {
                return returnType;
            }

            scalar = HasSingleResult( ApiDescription, out var type );

            if ( type != null )
            {
                var resolver = new StructuredTypeResolver( Model );
                returnType = resolver.GetStructuredType( type );
            }

            return returnType;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the API return type is scalar.
    /// </summary>
    /// <value>True if the API return type is scalar; otherwise, false.</value>
    public bool IsSingleResult
    {
        get
        {
            if ( scalar.HasValue )
            {
                return scalar.Value;
            }

            scalar = HasSingleResult( ApiDescription, out var type );

            if ( type != null )
            {
                var resolver = new StructuredTypeResolver( Model );
                returnType = resolver.GetStructuredType( type );
            }

            return scalar.Value;
        }
    }
}