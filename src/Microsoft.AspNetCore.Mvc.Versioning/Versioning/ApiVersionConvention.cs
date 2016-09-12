namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using ApplicationModels;
    using Conventions;
    using System;

    /// <summary>
    /// Represents an <see cref="IApplicationModelConvention">application model convention</see> which applies
    /// convention-based API versions controllers and their actions.
    /// </summary>
    [CLSCompliant( false )]
    public class ApiVersionConvention : IApplicationModelConvention
    {
        private readonly ApiVersionConventionBuilder conventionBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionConvention"/> class.
        /// </summary>
        /// <param name="conventionBuilder">The <see cref="ApiVersionConventionBuilder">convention builder</see>
        /// containing the configured conventions to aply.</param>
        public ApiVersionConvention( ApiVersionConventionBuilder conventionBuilder )
        {
            Arg.NotNull( conventionBuilder, nameof( conventionBuilder ) );
            this.conventionBuilder = conventionBuilder;
        }

        /// <summary>
        /// Applies the convention to the specified application.
        /// </summary>
        /// <param name="application">The <see cref="ApplicationModel">application</see> to apply the convention to.</param>
        public void Apply( ApplicationModel application )
        {
            foreach ( var controller in application.Controllers )
            {
                conventionBuilder.ApplyTo( controller );
            }
        }
    }
}