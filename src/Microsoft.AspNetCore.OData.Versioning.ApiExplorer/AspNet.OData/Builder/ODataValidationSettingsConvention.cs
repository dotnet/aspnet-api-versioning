namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
    using static Microsoft.AspNetCore.Http.StatusCodes;
    using static Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class ODataValidationSettingsConvention
    {
        /// <inheritdoc />
        public virtual void ApplyTo( ApiDescription apiDescription )
        {
            if ( apiDescription == null )
            {
                throw new ArgumentNullException( nameof( apiDescription ) );
            }

            if ( !IsSupported( apiDescription ) )
            {
                return;
            }

            var context = new ODataQueryOptionDescriptionContext( ValidationSettings );
            var model = apiDescription.EdmModel();
            var queryOptions = GetQueryOptions( Settings.DefaultQuerySettings!, context );
            var singleResult = IsSingleResult( apiDescription, out var resultType );
            var visitor = new ODataAttributeVisitor( context, model, queryOptions, resultType, singleResult );

            visitor.Visit( apiDescription );

            var options = visitor.AllowedQueryOptions;
            var parameterDescriptions = apiDescription.ParameterDescriptions;

            if ( options.HasFlag( Select ) )
            {
                parameterDescriptions.Add( NewSelectParameter( context ) );
            }

            if ( options.HasFlag( Expand ) )
            {
                parameterDescriptions.Add( NewExpandParameter( context ) );
            }

            if ( singleResult )
            {
                return;
            }

            if ( options.HasFlag( Filter ) )
            {
                parameterDescriptions.Add( NewFilterParameter( context ) );
            }

            if ( options.HasFlag( OrderBy ) )
            {
                parameterDescriptions.Add( NewOrderByParameter( context ) );
            }

            if ( options.HasFlag( Top ) )
            {
                parameterDescriptions.Add( NewTopParameter( context ) );
            }

            if ( options.HasFlag( Skip ) )
            {
                parameterDescriptions.Add( NewSkipParameter( context ) );
            }

            if ( options.HasFlag( Count ) )
            {
                parameterDescriptions.Add( NewCountParameter( context ) );
            }
        }

        /// <summary>
        /// Creates a new API parameter description.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="description">The parameter description.</param>
        /// <param name="type">The parameter value type.</param>
        /// <param name="defaultValue">The parameter default value, if any.</param>
        /// <returns>A new <see cref="ApiParameterDescription">parameter description</see>.</returns>
        protected virtual ApiParameterDescription NewParameterDescription( string name, string description, Type type, object? defaultValue = default )
        {
            return new ApiParameterDescription()
            {
                DefaultValue = defaultValue,
                IsRequired = false,
                ModelMetadata = new ODataQueryOptionModelMetadata( Settings.ModelMetadataProvider!, type, description ),
                Name = name,
                ParameterDescriptor = new ParameterDescriptor()
                {
                    Name = name,
                    ParameterType = type,
                },
                Source = Query,
                Type = type,
            };
        }

        static bool IsSingleResult( ApiDescription description, out Type? resultType )
        {
            if ( description.SupportedResponseTypes.Count == 0 )
            {
                resultType = default;
                return true;
            }

            var supportedResponseTypes = description.SupportedResponseTypes;
            var candidates = new List<ApiResponseType>( supportedResponseTypes.Count );

            for ( var i = 0; i < supportedResponseTypes.Count; i++ )
            {
                var supportedResponseType = supportedResponseTypes[i];

                if ( supportedResponseType.Type == null )
                {
                    continue;
                }

                var statusCode = supportedResponseType.StatusCode;

                if ( statusCode >= Status200OK && statusCode < Status300MultipleChoices )
                {
                    candidates.Add( supportedResponseType );
                }
            }

            if ( candidates.Count == 0 )
            {
                resultType = default;
                return true;
            }

            candidates.Sort( ( r1, r2 ) => r1.StatusCode.CompareTo( r2.StatusCode ) );

            var responseType = candidates[0].Type.ExtractInnerType();

            if ( responseType.IsEnumerable( out resultType ) )
            {
                return false;
            }

            resultType = responseType;
            return true;
        }

        static bool IsSupported( ApiDescription apiDescription )
        {
            return apiDescription.HttpMethod.ToUpperInvariant() switch
            {
                "GET" => true,
                "POST" => apiDescription.Operation()?.IsAction() == true,
                _ => false,
            };
        }
    }
}