namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNet.OData.Query;
    using System;

    /// <summary>
    /// Represents an OData controller action query options convention builder.
    /// </summary>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public abstract class ODataActionQueryOptionsConventionBuilderBase : IODataQueryOptionsConventionBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataActionQueryOptionsConventionBuilderBase"/> class.
        /// </summary>
        protected ODataActionQueryOptionsConventionBuilderBase() { }

        /// <summary>
        /// Gets the validation settings used for the query options convention.
        /// </summary>
        /// <value>The <see cref="ODataValidationSettings">validation settings</see> for the convention.</value>
        protected ODataValidationSettings ValidationSettings { get; } = new ODataValidationSettings()
        {
            AllowedArithmeticOperators = AllowedArithmeticOperators.None,
            AllowedFunctions = AllowedFunctions.None,
            AllowedLogicalOperators = AllowedLogicalOperators.None,
            AllowedQueryOptions = AllowedQueryOptions.None,
        };

        /// <inheritdoc />
        public virtual IODataQueryOptionsConvention Build( ODataQueryOptionSettings settings ) =>
            new ODataValidationSettingsConvention( ValidationSettings, settings );
    }
}