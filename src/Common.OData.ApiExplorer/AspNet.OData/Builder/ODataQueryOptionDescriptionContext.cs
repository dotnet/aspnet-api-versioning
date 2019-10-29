namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNet.OData.Query;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents the context used to describe a query option.
    /// </summary>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public class ODataQueryOptionDescriptionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryOptionDescriptionContext"/> class.
        /// </summary>
        public ODataQueryOptionDescriptionContext() => AllowedOrderByProperties = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryOptionDescriptionContext"/> class.
        /// </summary>
        /// <param name="validationSettings">The <see cref="ODataValidationSettings">validation settings</see> to derive the description context from.</param>
        protected internal ODataQueryOptionDescriptionContext( ODataValidationSettings validationSettings )
        {
            if ( validationSettings == null )
            {
                throw new ArgumentNullException( nameof( validationSettings ) );
            }

            AllowedArithmeticOperators = validationSettings.AllowedArithmeticOperators;
            AllowedFunctions = validationSettings.AllowedFunctions;
            AllowedLogicalOperators = validationSettings.AllowedLogicalOperators;
            AllowedOrderByProperties = validationSettings.AllowedOrderByProperties.ToList();
            MaxOrderByNodeCount = validationSettings.MaxOrderByNodeCount;
            MaxAnyAllExpressionDepth = validationSettings.MaxAnyAllExpressionDepth;
            MaxNodeCount = validationSettings.MaxNodeCount;
            MaxSkip = validationSettings.MaxSkip;
            MaxTop = validationSettings.MaxTop;
            MaxExpansionDepth = validationSettings.MaxExpansionDepth;
        }

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
        public IList<string> AllowedSelectProperties { get; } = new List<string>();

        /// <summary>
        /// Gets the names of properties that can be expanded.
        /// </summary>
        /// <value>A <see cref="IList{T}">list</see> of expandable property names.</value>
        public IList<string> AllowedExpandProperties { get; } = new List<string>();

        /// <summary>
        /// Gets the names of properties that can be filtered.
        /// </summary>
        /// <value>A <see cref="IList{T}">list</see> of filterable property names.</value>
        public IList<string> AllowedFilterProperties { get; } = new List<string>();

        /// <summary>
        /// Gets the names of properties that can be sorted.
        /// </summary>
        /// <value>A <see cref="IList{T}">list</see> of sortable property names.</value>
        public IList<string> AllowedOrderByProperties { get; private set; }

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
    }
}