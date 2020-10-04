namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;

    sealed class PseudoModelBindingVisitor
    {
        internal PseudoModelBindingVisitor( ApiParameterContext context, ParameterDescriptor parameter )
        {
            Context = context;
            Parameter = parameter;
        }

        internal ApiParameterContext Context { get; }

        internal ParameterDescriptor Parameter { get; }

        HashSet<PropertyKey> Visited { get; } = new HashSet<PropertyKey>( new PropertyKeyEqualityComparer() );

        internal void WalkParameter( ApiParameterDescriptionContext context ) => Visit( context, BindingSource.ModelBinding, containerName: string.Empty );

        static string GetName( string containerName, ApiParameterDescriptionContext metadata )
        {
            if ( string.IsNullOrEmpty( metadata.BinderModelName ) )
            {
                return ModelNames.CreatePropertyModelName( containerName, metadata.PropertyName );
            }

            return metadata.BinderModelName;
        }

        void Visit( ApiParameterDescriptionContext bindingContext, BindingSource ambientSource, string containerName )
        {
            var source = bindingContext.BindingSource;

            if ( source != null && source.IsGreedy )
            {
                Context.Results.Add( CreateResult( bindingContext, source, containerName ) );
                return;
            }

            var modelMetadata = bindingContext.ModelMetadata;

            if ( modelMetadata.IsEnumerableType || !modelMetadata.IsComplexType || modelMetadata.Properties.Count == 0 )
            {
                Context.Results.Add( CreateResult( bindingContext, source ?? ambientSource, containerName ) );
                return;
            }

            var newContainerName = containerName;

            if ( modelMetadata.ContainerType != null )
            {
                newContainerName = GetName( containerName, bindingContext );
            }

            source ??= ambientSource;

            for ( var i = 0; i < modelMetadata.Properties.Count; i++ )
            {
                var propertyMetadata = modelMetadata.Properties[i];
                var key = new PropertyKey( propertyMetadata, source );
                var propertyContext = new ApiParameterDescriptionContext( propertyMetadata, bindingInfo: null, propertyName: null );

                if ( Visited.Add( key ) )
                {
                    Visit( propertyContext, source, newContainerName );
                }
                else
                {
                    Context.Results.Add( CreateResult( propertyContext, source, newContainerName ) );
                }
            }
        }

        ApiParameterDescription CreateResult( ApiParameterDescriptionContext bindingContext, BindingSource source, string containerName )
        {
            var modelMetadata = bindingContext.ModelMetadata;
            var type = modelMetadata.ModelType;

            if ( type.IsODataActionParameters() && Context.RouteContext.Operation?.IsAction() == true )
            {
                var action = (IEdmAction) Context.RouteContext.Operation;
                var apiVersion = Context.RouteContext.ApiVersion;

                type = Context.TypeBuilder.NewActionParameters( Context.Services, action, apiVersion, Context.RouteContext.ActionDescriptor.ControllerName );
            }
            else
            {
                type = type.SubstituteIfNecessary( new TypeSubstitutionContext( Context.Services, Context.TypeBuilder ) );
            }

            return new ApiParameterDescription()
            {
                ModelMetadata = modelMetadata.SubstituteIfNecessary( type ),
                Name = GetName( containerName, bindingContext ),
                Source = source,
                Type = type,
                ParameterDescriptor = Parameter,
            };
        }

        readonly struct PropertyKey
        {
            public readonly Type ContainerType;
            public readonly string PropertyName;
            public readonly BindingSource Source;

            public PropertyKey( ModelMetadata metadata, BindingSource source )
            {
                ContainerType = metadata.ContainerType;
                PropertyName = metadata.PropertyName;
                Source = source;
            }
        }

        sealed class PropertyKeyEqualityComparer : IEqualityComparer<PropertyKey>
        {
            public bool Equals( PropertyKey x, PropertyKey y ) => x.ContainerType == y.ContainerType && x.PropertyName == y.PropertyName && x.Source == y.Source;

            public int GetHashCode( PropertyKey obj ) => HashCode.Combine( obj.ContainerType, obj.PropertyName, obj.Source );
        }
    }
}