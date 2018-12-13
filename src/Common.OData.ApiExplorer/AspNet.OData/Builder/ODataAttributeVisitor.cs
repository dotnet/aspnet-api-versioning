﻿namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNet.OData.Query;
    using Microsoft.OData.Edm;
#if WEBAPI
    using Microsoft.Web.Http;
#else
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
#endif
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
#if WEBAPI
    using System.Web.Http.Description;
#endif
    using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
    using static Microsoft.AspNet.OData.Query.SelectExpandType;
    using static System.StringSplitOptions;

    sealed partial class ODataAttributeVisitor
    {
        readonly ODataQueryOptionDescriptionContext context;
        readonly Type resultType;
        readonly IEdmModel model;
        readonly StructuredTypeResolver typeResolver;

        internal ODataAttributeVisitor(
            ODataQueryOptionDescriptionContext context,
            IEdmModel model,
            AllowedQueryOptions allowedQueryOptions,
            Type resultType,
            bool singleResult )
        {
            Contract.Requires( context != null );

            this.context = context;
            AllowedQueryOptions = allowedQueryOptions;
            this.resultType = resultType;
            IsSingleResult = singleResult;
            this.model = model;
            typeResolver = new StructuredTypeResolver( model );
        }

        internal AllowedQueryOptions AllowedQueryOptions { get; private set; }

        bool IsSingleResult { get; }

        internal void Visit( ApiDescription apiDescription )
        {
            Contract.Requires( apiDescription != null );

            VisitAction( apiDescription.ActionDescriptor );

            if ( resultType == null )
            {
                return;
            }

            var modelType = typeResolver.GetStructuredType( resultType );

            if ( modelType != null )
            {
                VisitModel( modelType );
            }
        }

        static void ClearCollectionIfItContainsAllProperties( IList<string> list, ISet<string> set )
        {
            Contract.Requires( list != null );
            Contract.Requires( set != null );

            if ( list.Count != set.Count )
            {
                return;
            }

            for ( var i = 0; i < list.Count; i++ )
            {
                if ( !set.Contains( list[i] ) )
                {
                    return;
                }
            }

            // avoid verbose messages by clearing the list when it contains all possible properties
            list.Clear();
        }

        void VisitModel( IEdmStructuredType modelType )
        {
            Contract.Requires( modelType != null );

            var querySettings = model.GetAnnotationValue<ModelBoundQuerySettings>( modelType );

            if ( querySettings == null )
            {
                return;
            }

            var properties = new HashSet<string>( modelType.Properties().Select( p => p.Name ), StringComparer.OrdinalIgnoreCase );

            VisitSelect( querySettings, properties );
            VisitExpand( querySettings, properties );

            if ( IsSingleResult )
            {
                return;
            }

            VisitCount( querySettings );
            VisitFilter( querySettings, properties );
            VisitOrderBy( querySettings, properties );
            VisitMaxTop( querySettings );
        }

        void VisitEnableQuery( IReadOnlyList<EnableQueryAttribute> attributes )
        {
            Contract.Requires( attributes != null );

            var @default = new EnableQueryAttribute();

            for ( var i = 0; i < attributes.Count; i++ )
            {
                var attribute = attributes[i];

                if ( attribute.AllowedArithmeticOperators == AllowedArithmeticOperators.None )
                {
                    attribute.AllowedArithmeticOperators = AllowedArithmeticOperators.None;
                }
                else
                {
                    context.AllowedArithmeticOperators |= attribute.AllowedArithmeticOperators;
                }

                if ( attribute.AllowedFunctions == AllowedFunctions.None )
                {
                    attribute.AllowedFunctions = AllowedFunctions.None;
                }
                else
                {
                    context.AllowedFunctions |= attribute.AllowedFunctions;
                }

                if ( attribute.AllowedLogicalOperators == AllowedLogicalOperators.None )
                {
                    attribute.AllowedLogicalOperators = AllowedLogicalOperators.None;
                }
                else
                {
                    context.AllowedLogicalOperators |= attribute.AllowedLogicalOperators;
                }

                if ( attribute.AllowedQueryOptions == AllowedQueryOptions.None )
                {
                    attribute.AllowedQueryOptions = AllowedQueryOptions.None;
                }
                else
                {
                    AllowedQueryOptions |= attribute.AllowedQueryOptions;
                }

                if ( context.MaxAnyAllExpressionDepth == @default.MaxAnyAllExpressionDepth )
                {
                    context.MaxAnyAllExpressionDepth = attribute.MaxAnyAllExpressionDepth;
                }

                if ( context.MaxExpansionDepth == @default.MaxExpansionDepth )
                {
                    context.MaxExpansionDepth = attribute.MaxExpansionDepth;
                }

                if ( context.MaxNodeCount == @default.MaxNodeCount )
                {
                    context.MaxNodeCount = attribute.MaxNodeCount;
                }

                if ( context.MaxOrderByNodeCount == @default.MaxOrderByNodeCount )
                {
                    context.MaxOrderByNodeCount = attribute.MaxOrderByNodeCount;
                }

                if ( context.MaxSkip != @default.MaxSkip )
                {
                    context.MaxSkip = attribute.MaxSkip;
                }

                if ( context.MaxTop != @default.MaxTop )
                {
                    context.MaxTop = attribute.MaxTop;
                }

                if ( !string.IsNullOrEmpty( attribute.AllowedOrderByProperties ) )
                {
                    var properties = attribute.AllowedOrderByProperties.Split( new[] { ',' }, RemoveEmptyEntries );
                    var allowedOrderByProperties = context.AllowedOrderByProperties;
                    var comparer = StringComparer.OrdinalIgnoreCase;

                    for ( var j = 0; j < properties.Length; j++ )
                    {
                        var property = properties[j].Trim();

                        if ( !string.IsNullOrEmpty( property ) && !allowedOrderByProperties.Contains( property, comparer ) )
                        {
                            allowedOrderByProperties.Add( property );
                        }
                    }
                }
            }
        }

        void VisitSelect( ModelBoundQuerySettings querySettings, ICollection<string> properties ) =>
            Visit( querySettings, properties, Select, IsSelectEnabled, context.AllowedSelectProperties, querySettings.SelectConfigurations, s => s != Disabled );

        void VisitExpand( ModelBoundQuerySettings querySettings, ICollection<string> properties )
        {
            Contract.Requires( querySettings != null );
            Contract.Requires( properties != null );

            var @default = new ExpandConfiguration();

            bool IsExpandAllowed( ExpandConfiguration expand )
            {
                if ( expand.ExpandType == Disabled )
                {
                    return false;
                }

                if ( expand.MaxDepth != @default.MaxDepth )
                {
                    context.MaxExpansionDepth = expand.MaxDepth;
                }

                return true;
            }

            Visit( querySettings, properties, Expand, IsExpandEnabled, context.AllowedExpandProperties, querySettings.ExpandConfigurations, IsExpandAllowed );
        }

        void VisitFilter( ModelBoundQuerySettings querySettings, ICollection<string> properties ) =>
            Visit( querySettings, properties, Filter, IsFilterEnabled, context.AllowedFilterProperties, querySettings.FilterConfigurations, s => s );

        void VisitOrderBy( ModelBoundQuerySettings querySettings, ICollection<string> properties ) =>
            Visit( querySettings, properties, OrderBy, IsOrderByEnabled, context.AllowedOrderByProperties, querySettings.OrderByConfigurations, s => s );

        void VisitCount( ModelBoundQuerySettings querySettings )
        {
            Contract.Requires( querySettings != null );

            if ( querySettings.Countable == true )
            {
                AllowedQueryOptions |= Count;
            }
            else
            {
                AllowedQueryOptions &= ~Count;
            }
        }

        void VisitMaxTop( ModelBoundQuerySettings querySettings )
        {
            Contract.Requires( querySettings != null );

            if ( querySettings.MaxTop != null && querySettings.MaxTop.Value > 0 )
            {
                context.MaxTop = querySettings.MaxTop;
            }
        }

        void Visit<TSetting>(
            ModelBoundQuerySettings querySettings,
            ICollection<string> properties,
            AllowedQueryOptions option,
            Func<ModelBoundQuerySettings, bool> enabled,
            IList<string> queryableProperties,
            Dictionary<string, TSetting> configurations,
            Func<TSetting, bool> allowed )
        {
            Contract.Requires( querySettings != null );
            Contract.Requires( properties != null );
            Contract.Requires( enabled != null );
            Contract.Requires( queryableProperties != null );
            Contract.Requires( configurations != null );
            Contract.Requires( allowed != null );

            if ( !enabled( querySettings ) )
            {
                AllowedQueryOptions &= ~option;
                queryableProperties.Clear();
                return;
            }

            AllowedQueryOptions |= option;

            if ( configurations.Count == 0 )
            {
                // skip property-specific configurations; everything is allowed
                return;
            }

            var comparer = StringComparer.OrdinalIgnoreCase;
            var allowedProperties = new HashSet<string>( comparer );
            var disallowedProperties = new HashSet<string>( comparer );

            foreach ( var property in configurations )
            {
                var name = property.Key;

                // note: remember that model bound attributes might be using hardcode attributes. we need
                // to account for a substituted type on a down-level model where the property does not exist
                if ( !properties.Contains( name ) )
                {
                    continue;
                }

                if ( allowed( property.Value ) )
                {
                    allowedProperties.Add( name );
                }
                else
                {
                    disallowedProperties.Add( name );
                }
            }

            // if there's no specifically allowed properties, allow them all
            if ( allowedProperties.Count == 0 )
            {
                foreach ( var property in properties )
                {
                    allowedProperties.Add( property );
                }
            }

            // remove any disallowed properties
            allowedProperties.ExceptWith( disallowedProperties );

            // if the final allowed set results in all properties, then clear the
            // properties to keep message less verbose
            if ( allowedProperties.Count == properties.Count )
            {
                queryableProperties.Clear();
                return;
            }

            foreach ( var property in allowedProperties )
            {
                queryableProperties.Add( property );
            }
        }

        static bool IsSelectEnabled( ModelBoundQuerySettings querySettings ) =>
            ( querySettings.DefaultSelectType != null && querySettings.DefaultSelectType.Value != Disabled ) ||
            querySettings.SelectConfigurations.Any( p => p.Value != Disabled );

        static bool IsExpandEnabled( ModelBoundQuerySettings querySettings ) =>
            ( querySettings.DefaultExpandType != null && querySettings.DefaultExpandType.Value != Disabled ) ||
            querySettings.ExpandConfigurations.Any( p => p.Value.ExpandType != Disabled );

        static bool IsFilterEnabled( ModelBoundQuerySettings querySettings ) =>
            querySettings.DefaultEnableFilter == true || querySettings.FilterConfigurations.Any( p => p.Value );

        static bool IsOrderByEnabled( ModelBoundQuerySettings querySettings ) =>
            querySettings.DefaultEnableOrderBy == true || querySettings.OrderByConfigurations.Any( p => p.Value );
    }
}