namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNet.OData.Query;
#if !WEBAPI
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
#endif
    using System;
#if WEBAPI
    using System.Web.Http.Description;
#endif
    using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;

    /// <summary>
    /// Represents an OData query options convention based on <see cref="ODataValidationSettings">validation settings</see>.
    /// </summary>
    public partial class ODataValidationSettingsConvention : IODataQueryOptionsConvention
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataValidationSettingsConvention"/> class.
        /// </summary>
        /// <param name="validationSettings">The <see cref="ODataValidationSettings">validation settings</see> the convention is based on.</param>
        /// <param name="settings">The <see cref="ODataQueryOptionSettings">settings</see> used by the convention.</param>
        public ODataValidationSettingsConvention( ODataValidationSettings validationSettings, ODataQueryOptionSettings settings )
        {
            ValidationSettings = validationSettings;
            Settings = settings;
        }

        /// <summary>
        /// Gets the validation settings used for the query options convention.
        /// </summary>
        /// <value>The <see cref="ODataValidationSettings">validation settings</see> for the convention.</value>
        protected ODataValidationSettings ValidationSettings { get; }

        /// <summary>
        /// Gets the settings for OData query options.
        /// </summary>
        /// <value>The <see cref="ODataQueryOptionSettings">settings</see> used by the convention.</value>
        protected ODataQueryOptionSettings Settings { get; }

        /// <summary>
        /// Creates and returns a new parameter descriptor for the $filter query option.
        /// </summary>
        /// <param name="descriptionContext">The <see cref="ODataQueryOptionDescriptionContext">validation settings</see> used to create the parameter.</param>
        /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
        protected virtual ApiParameterDescription NewFilterParameter( ODataQueryOptionDescriptionContext descriptionContext )
        {
            var description = Settings.DescriptionProvider.Describe( Filter, descriptionContext );
            return NewParameterDescription( GetName( Filter ), description, typeof( string ) );
        }

        /// <summary>
        /// Creates and returns a new parameter descriptor for the $expand query option.
        /// </summary>
        /// <param name="descriptionContext">The <see cref="ODataQueryOptionDescriptionContext">validation settings</see> used to create the parameter.</param>
        /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
        protected virtual ApiParameterDescription NewExpandParameter( ODataQueryOptionDescriptionContext descriptionContext )
        {
            var description = Settings.DescriptionProvider.Describe( Expand, descriptionContext );
            return NewParameterDescription( GetName( Expand ), description, typeof( string ) );
        }

        /// <summary>
        /// Creates and returns a new parameter descriptor for the $select query option.
        /// </summary>
        /// <param name="descriptionContext">The <see cref="ODataQueryOptionDescriptionContext">validation settings</see> used to create the parameter.</param>
        /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
        protected virtual ApiParameterDescription NewSelectParameter( ODataQueryOptionDescriptionContext descriptionContext )
        {
            var description = Settings.DescriptionProvider.Describe( Select, descriptionContext );
            return NewParameterDescription( GetName( Select ), description, typeof( string ) );
        }

        /// <summary>
        /// Creates and returns a new parameter descriptor for the $orderby query option.
        /// </summary>
        /// <param name="descriptionContext">The <see cref="ODataQueryOptionDescriptionContext">validation settings</see> used to create the parameter.</param>
        /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
        protected virtual ApiParameterDescription NewOrderByParameter( ODataQueryOptionDescriptionContext descriptionContext )
        {
            var description = Settings.DescriptionProvider.Describe( OrderBy, descriptionContext );
            return NewParameterDescription( GetName( OrderBy ), description, typeof( string ) );
        }

        /// <summary>
        /// Creates and returns a new parameter descriptor for the $top query option.
        /// </summary>
        /// <param name="descriptionContext">The <see cref="ODataQueryOptionDescriptionContext">validation settings</see> used to create the parameter.</param>
        /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
        protected virtual ApiParameterDescription NewTopParameter( ODataQueryOptionDescriptionContext descriptionContext )
        {
            var description = Settings.DescriptionProvider.Describe( Top, descriptionContext );
            return NewParameterDescription( GetName( Top ), description, typeof( int ) );
        }

        /// <summary>
        /// Creates and returns a new parameter descriptor for the $skip query option.
        /// </summary>
        /// <param name="descriptionContext">The <see cref="ODataQueryOptionDescriptionContext">validation settings</see> used to create the parameter.</param>
        /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
        protected virtual ApiParameterDescription NewSkipParameter( ODataQueryOptionDescriptionContext descriptionContext )
        {
            var description = Settings.DescriptionProvider.Describe( Skip, descriptionContext );
            return NewParameterDescription( GetName( Skip ), description, typeof( int ) );
        }

        /// <summary>
        /// Creates and returns a new parameter descriptor for the $count query option.
        /// </summary>
        /// <param name="descriptionContext">The <see cref="ODataQueryOptionDescriptionContext">validation settings</see> used to create the parameter.</param>
        /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
        protected virtual ApiParameterDescription NewCountParameter( ODataQueryOptionDescriptionContext descriptionContext )
        {
            var description = Settings.DescriptionProvider.Describe( Count, descriptionContext );
            return NewParameterDescription( GetName( Count ), description, typeof( bool ), defaultValue: false );
        }

        string GetName( AllowedQueryOptions option )
        {
#pragma warning disable CA1308 // Normalize strings to uppercase
            var name = option.ToString().ToLowerInvariant();
#pragma warning restore CA1308
            return Settings.NoDollarPrefix ? name : name.Insert( 0, "$" );
        }

        AllowedQueryOptions GetQueryOptions( DefaultQuerySettings settings, ODataQueryOptionDescriptionContext context )
        {
            var queryOptions = ValidationSettings.AllowedQueryOptions;

            if ( settings.EnableCount )
            {
                queryOptions |= Count;
            }

            if ( settings.EnableExpand )
            {
                queryOptions |= Expand;
            }

            if ( settings.EnableFilter )
            {
                queryOptions |= Filter;
            }

            if ( settings.EnableOrderBy )
            {
                queryOptions |= OrderBy;
            }

            if ( settings.EnableSelect )
            {
                queryOptions |= Select;
            }

            if ( settings.MaxTop != null && settings.MaxTop.Value > 0 )
            {
                context.MaxTop = settings.MaxTop;
            }

            return queryOptions;
        }
    }
}