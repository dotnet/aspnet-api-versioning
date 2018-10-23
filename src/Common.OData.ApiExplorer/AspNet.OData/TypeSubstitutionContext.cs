﻿namespace Microsoft.AspNet.OData
{
#if !WEBAPI
    using Microsoft.AspNetCore.Mvc;
#endif
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.Edm;
#if WEBAPI
    using Microsoft.Web.Http;
#endif
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Represents a type substitution context.
    /// </summary>
    public class TypeSubstitutionContext
    {
        private readonly Lazy<IEdmModel> model;
        private readonly Lazy<ApiVersion> apiVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeSubstitutionContext"/> class.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> to compare against.</param>
        /// <param name="assemblies">The <see cref="IEnumerable{T}">sequence</see> of application <see cref="Assembly">assemblies</see>.</param>
        /// <param name="modelTypeBuilder">The associated <see cref="IModelTypeBuilder">model type builder</see>.</param>
        public TypeSubstitutionContext( IEdmModel model, IEnumerable<Assembly> assemblies, IModelTypeBuilder modelTypeBuilder )
        {
            Arg.NotNull( model, nameof( model ) );
            Arg.NotNull( assemblies, nameof( assemblies ) );
            Arg.NotNull( modelTypeBuilder, nameof( modelTypeBuilder ) );

            this.model = new Lazy<IEdmModel>( () => model );
            apiVersion = new Lazy<ApiVersion>( () => Model.GetAnnotationValue<ApiVersionAnnotation>( Model )?.ApiVersion ?? ApiVersion.Default );
            Assemblies = assemblies;
            ModelTypeBuilder = modelTypeBuilder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeSubstitutionContext"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider">service provider</see> that the
        /// <see cref="IEdmModel">EDM model</see> can be resolved from.</param>
        /// <param name="assemblies">The <see cref="IEnumerable{T}">sequence</see> of application <see cref="Assembly">assemblies</see>.</param>
        /// <param name="modelTypeBuilder">The associated <see cref="IModelTypeBuilder">model type builder</see>.</param>
        public TypeSubstitutionContext( IServiceProvider serviceProvider, IEnumerable<Assembly> assemblies, IModelTypeBuilder modelTypeBuilder )
        {
            Arg.NotNull( serviceProvider, nameof( serviceProvider ) );
            Arg.NotNull( assemblies, nameof( assemblies ) );
            Arg.NotNull( modelTypeBuilder, nameof( modelTypeBuilder ) );

            model = new Lazy<IEdmModel>( serviceProvider.GetRequiredService<IEdmModel> );
            apiVersion = new Lazy<ApiVersion>( () => Model.GetAnnotationValue<ApiVersionAnnotation>( Model )?.ApiVersion ?? ApiVersion.Default );
            Assemblies = assemblies;
            ModelTypeBuilder = modelTypeBuilder;
        }

        /// <summary>
        /// Gets the source Entity Data Model (EDM).
        /// </summary>
        /// <value>The associated <see cref="IEdmModel">EDM model</see> compared against for substitutions.</value>
        public IEdmModel Model => model.Value;

        /// <summary>
        /// Gets API version associated with the source model.
        /// </summary>
        /// <value>The associated <see cref="ApiVersion">API version</see>.</value>
        public ApiVersion ApiVersion => apiVersion.Value;

        /// <summary>
        /// Gets the sequence of known application assemblies.
        /// </summary>
        /// <value>The <see cref="IEnumerable{T}">sequence</see> of application <see cref="Assembly">assemblies</see>.</value>
        public IEnumerable<Assembly> Assemblies { get; }

        /// <summary>
        /// Gets the model type builder used to create substitution types.
        /// </summary>
        /// <value>The associated <see cref="IModelTypeBuilder">model type builder</see>.</value>
        public IModelTypeBuilder ModelTypeBuilder { get; }
    }
}