namespace Microsoft.AspNet.OData.Builder
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
        readonly Type? resultType;
        readonly IEdmModel? model;
        readonly StructuredTypeResolver typeResolver;

        internal ODataAttributeVisitor(
            ODataQueryOptionDescriptionContext context,
            IEdmModel? model,
            AllowedQueryOptions allowedQueryOptions,
            Type? resultType,
            bool singleResult )
        {
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

        void VisitModel( IEdmStructuredType modelType )
        {
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
            var @default = new EnableQueryAttribute();

            for ( var i = 0; i < attributes.Count; i++ )
            {
                var attribute = attributes[i];

                if ( attribute.AllowedArithmeticOperators == AllowedArithmeticOperators.None )
                {
                    context.AllowedArithmeticOperators = AllowedArithmeticOperators.None;
                }
                else
                {
                    context.AllowedArithmeticOperators |= attribute.AllowedArithmeticOperators;
                }

                if ( attribute.AllowedFunctions == AllowedFunctions.None )
                {
                    context.AllowedFunctions = AllowedFunctions.None;
                }
                else
                {
                    context.AllowedFunctions |= attribute.AllowedFunctions;
                }

                if ( attribute.AllowedLogicalOperators == AllowedLogicalOperators.None )
                {
                    context.AllowedLogicalOperators = AllowedLogicalOperators.None;
                }
                else
                {
                    context.AllowedLogicalOperators |= attribute.AllowedLogicalOperators;
                }

                if ( attribute.AllowedQueryOptions == AllowedQueryOptions.None )
                {
                    AllowedQueryOptions = AllowedQueryOptions.None;
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
            if ( !querySettings.Countable.HasValue )
            {
                return;
            }

            if ( querySettings.Countable.Value )
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

                // note: remember that model bound attributes might be using hard-coded attributes. we need
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
                if ( !queryableProperties.Contains( property, comparer ) )
                {
                    queryableProperties.Add( property );
                }
            }
        }

        bool IsSelectEnabled( ModelBoundQuerySettings querySettings )
        {
            if ( !querySettings.DefaultSelectType.HasValue )
            {
                return AllowedQueryOptions.HasFlag( Select ) ||
                       querySettings.SelectConfigurations.Any( p => p.Value != Disabled );
            }

            return querySettings.DefaultSelectType.Value != Disabled ||
                   querySettings.SelectConfigurations.Any( p => p.Value != Disabled );
        }

        bool IsExpandEnabled( ModelBoundQuerySettings querySettings )
        {
            if ( !querySettings.DefaultExpandType.HasValue )
            {
                return AllowedQueryOptions.HasFlag( Expand ) ||
                       querySettings.ExpandConfigurations.Any( p => p.Value.ExpandType != Disabled );
            }

            return querySettings.DefaultExpandType.Value != Disabled ||
                   querySettings.ExpandConfigurations.Any( p => p.Value.ExpandType != Disabled );
        }

        bool IsFilterEnabled( ModelBoundQuerySettings querySettings )
        {
            if ( !querySettings.DefaultEnableFilter.HasValue )
            {
                return AllowedQueryOptions.HasFlag( Filter ) ||
                       querySettings.FilterConfigurations.Any( p => p.Value );
            }

            return querySettings.DefaultEnableFilter.Value ||
                   querySettings.FilterConfigurations.Any( p => p.Value );
        }

        bool IsOrderByEnabled( ModelBoundQuerySettings querySettings )
        {
            if ( !querySettings.DefaultEnableOrderBy.HasValue )
            {
                return AllowedQueryOptions.HasFlag( OrderBy ) ||
                       querySettings.OrderByConfigurations.Any( p => p.Value );
            }

            return querySettings.DefaultEnableOrderBy.Value ||
                   querySettings.OrderByConfigurations.Any( p => p.Value );
        }
    }
}