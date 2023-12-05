// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if NETFRAMEWORK
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
#endif
using Microsoft.OData.Edm;
#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.ModelBuilder.Config;
#endif
#if NETFRAMEWORK
using System.Web.Http.Description;
using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
using static Microsoft.AspNet.OData.Query.SelectExpandType;
#else
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;
using static Microsoft.OData.ModelBuilder.SelectExpandType;
#endif
using static System.StringSplitOptions;

internal sealed partial class ODataAttributeVisitor
{
    private static readonly char[] Comma = [','];
    private readonly ODataQueryOptionDescriptionContext context;

    internal ODataAttributeVisitor(
        ODataQueryOptionDescriptionContext context,
        AllowedQueryOptions allowedQueryOptions )
    {
        this.context = context;
        AllowedQueryOptions = allowedQueryOptions;
    }

    internal AllowedQueryOptions AllowedQueryOptions { get; private set; }

    internal void Visit( ApiDescription apiDescription )
    {
        var modelType = context.ReturnType;

        if ( modelType != null )
        {
            VisitModel( modelType );
        }

        VisitAction( apiDescription.ActionDescriptor );
    }

    private void VisitModel( IEdmStructuredType modelType )
    {
        var querySettings = context.Model.GetAnnotationValue<ModelBoundQuerySettings>( modelType );

        if ( querySettings == null )
        {
            return;
        }

        var properties = new HashSet<string>(
            modelType.Properties().Select( p => p.Name ),
            StringComparer.OrdinalIgnoreCase );

        VisitSelect( querySettings, properties );
        VisitExpand( querySettings, properties );

        if ( context.IsSingleResult )
        {
            return;
        }

        VisitCount( querySettings );
        VisitFilter( querySettings, properties );
        VisitOrderBy( querySettings, properties );
        VisitMaxTop( querySettings );
    }

    private void VisitEnableQuery( IReadOnlyList<EnableQueryAttribute> attributes )
    {
        var @default = new EnableQueryAttribute();

        for ( var i = 0; i < attributes.Count; i++ )
        {
            var attribute = attributes[i];

            context.AllowedArithmeticOperators = attribute.AllowedArithmeticOperators;
            context.AllowedFunctions = attribute.AllowedFunctions;
            context.AllowedLogicalOperators = attribute.AllowedLogicalOperators;

            AllowedQueryOptions = attribute.AllowedQueryOptions;

            if ( attribute.MaxAnyAllExpressionDepth != @default.MaxAnyAllExpressionDepth )
            {
                context.MaxAnyAllExpressionDepth = attribute.MaxAnyAllExpressionDepth;
            }

            if ( attribute.MaxExpansionDepth != @default.MaxExpansionDepth )
            {
                context.MaxExpansionDepth = attribute.MaxExpansionDepth;
            }

            if ( attribute.MaxNodeCount != @default.MaxNodeCount )
            {
                context.MaxNodeCount = attribute.MaxNodeCount;
            }

            if ( attribute.MaxOrderByNodeCount != @default.MaxOrderByNodeCount )
            {
                context.MaxOrderByNodeCount = attribute.MaxOrderByNodeCount;
            }

            if ( attribute.MaxSkip != @default.MaxSkip )
            {
                context.MaxSkip = attribute.MaxSkip;
            }

            if ( attribute.MaxTop != @default.MaxTop )
            {
                context.MaxTop = attribute.MaxTop;
            }

            if ( string.IsNullOrEmpty( attribute.AllowedOrderByProperties ) )
            {
                continue;
            }

            var properties = attribute.AllowedOrderByProperties.Split( Comma, RemoveEmptyEntries );
            var allowedOrderByProperties = context.AllowedOrderByProperties;
            var comparer = StringComparer.OrdinalIgnoreCase;

            for ( var j = 0; j < properties.Length; j++ )
            {
                var property = properties[j].Trim();

                if ( !string.IsNullOrEmpty( property ) &&
                     !allowedOrderByProperties.Contains( property, comparer ) )
                {
                    allowedOrderByProperties.Add( property );
                }
            }
        }
    }

    private void VisitSelect( ModelBoundQuerySettings querySettings, ICollection<string> properties ) =>
        Visit(
            querySettings,
            properties,
            Select,
            IsSelectEnabled,
            context.AllowedSelectProperties,
            querySettings.SelectConfigurations,
            setting => setting != Disabled );

    private void VisitExpand( ModelBoundQuerySettings querySettings, ICollection<string> properties )
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

        Visit(
            querySettings,
            properties,
            Expand,
            IsExpandEnabled,
            context.AllowedExpandProperties,
            querySettings.ExpandConfigurations,
            IsExpandAllowed );
    }

    private void VisitFilter( ModelBoundQuerySettings querySettings, ICollection<string> properties ) =>
        Visit(
            querySettings,
            properties,
            Filter,
            IsFilterEnabled,
            context.AllowedFilterProperties,
            querySettings.FilterConfigurations,
            setting => setting );

    private void VisitOrderBy( ModelBoundQuerySettings querySettings, ICollection<string> properties ) =>
        Visit(
            querySettings,
            properties,
            OrderBy,
            IsOrderByEnabled,
            context.AllowedOrderByProperties,
            querySettings.OrderByConfigurations,
            setting => setting );

    private void VisitCount( ModelBoundQuerySettings querySettings )
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

    private void VisitMaxTop( ModelBoundQuerySettings querySettings )
    {
        if ( querySettings.MaxTop.Unset() )
        {
            return;
        }

        context.MaxTop = querySettings.MaxTop;

        // calling the Page() configuration sets MaxTop and PageSize,
        // which is implied to enable $top and $skip
        AllowedQueryOptions |= Skip | Top;
    }

    private void Visit<TSetting>(
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

    private bool IsSelectEnabled( ModelBoundQuerySettings querySettings )
    {
        if ( !querySettings.DefaultSelectType.HasValue )
        {
            return AllowedQueryOptions.HasFlag( Select ) ||
                   querySettings.SelectConfigurations.Any( p => p.Value != Disabled );
        }

        return querySettings.DefaultSelectType.Value != Disabled ||
               querySettings.SelectConfigurations.Any( p => p.Value != Disabled );
    }

    private bool IsExpandEnabled( ModelBoundQuerySettings querySettings )
    {
        if ( !querySettings.DefaultExpandType.HasValue )
        {
            return AllowedQueryOptions.HasFlag( Expand ) ||
                   querySettings.ExpandConfigurations.Any( p => p.Value.ExpandType != Disabled );
        }

        return querySettings.DefaultExpandType.Value != Disabled ||
               querySettings.ExpandConfigurations.Any( p => p.Value.ExpandType != Disabled );
    }

    private bool IsFilterEnabled( ModelBoundQuerySettings querySettings )
    {
        if ( !querySettings.DefaultEnableFilter.HasValue )
        {
            return AllowedQueryOptions.HasFlag( Filter ) ||
                   querySettings.FilterConfigurations.Any( p => p.Value );
        }

        return querySettings.DefaultEnableFilter.Value ||
               querySettings.FilterConfigurations.Any( p => p.Value );
    }

    private bool IsOrderByEnabled( ModelBoundQuerySettings querySettings )
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