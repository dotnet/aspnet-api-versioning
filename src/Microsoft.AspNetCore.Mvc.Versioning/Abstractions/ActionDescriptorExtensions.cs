namespace Microsoft.AspNetCore.Mvc.Abstractions
{
    using ApplicationModels;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Versioning;

    /// <summary>
    /// Provides extension methods for the <see cref="ActionDescriptor"/> class.
    /// </summary>
    [CLSCompliant( false )]
    public static class ActionDescriptorExtensions
    {
        const string VersionPolicyIsAppliedKey = "MS_" + nameof( VersionPolicyIsApplied );

        static void VersionPolicyIsApplied( this ActionDescriptor action, bool value ) => action.Properties[VersionPolicyIsAppliedKey] = value;

        internal static bool VersionPolicyIsApplied( this ActionDescriptor action ) => action.Properties.GetOrDefault( VersionPolicyIsAppliedKey, false );

        internal static void AggregateAllVersions( this ActionDescriptor action, IEnumerable<ActionDescriptor> matchingActions )
        {
            Contract.Requires( action != null );
            Contract.Requires( matchingActions != null );

            if ( action.VersionPolicyIsApplied() )
            {
                return;
            }

            action.VersionPolicyIsApplied( true );

            var model = action.GetProperty<ApiVersionModel>();
            Contract.Assume( model != null );

            action.SetProperty( model.Aggregate( matchingActions.Select( a => a.GetProperty<ApiVersionModel>() ).Where( m => m != null ) ) );
        }

        /// <summary>
        /// Returns a value indicating whether the provided action implicitly maps to the specified version.
        /// </summary>
        /// <param name="action">The <see cref="ActionDescriptor">action</see> to evaluate.</param>
        /// <param name="version">The <see cref="ApiVersion">API version</see> to test the mapping for.</param>
        /// <returns>True if the <paramref name="action"/> implicitly maps to the specified <paramref name="version"/>; otherwise, false.</returns>
        public static bool IsImplicitlyMappedTo( this ActionDescriptor action, ApiVersion version )
        {
            Arg.NotNull( action, nameof( action ) );

            if ( version == null )
            {
                return false;
            }

            var model = action.GetProperty<ApiVersionModel>();

            if ( model != null && model.DeclaredApiVersions.Count > 0 )
            {
                return false;
            }

            model = action.GetProperty<ControllerModel>()?.GetProperty<ApiVersionModel>();

            return model != null && model.DeclaredApiVersions.Contains( version );
        }

        /// <summary>
        /// Returns a value indicating whether the provided action maps to the specified version.
        /// </summary>
        /// <param name="action">The <see cref="ActionDescriptor">action</see> to evaluate.</param>
        /// <param name="version">The <see cref="ApiVersion">API version</see> to test the mapping for.</param>
        /// <returns>True if the <paramref name="action"/> maps to the specified <paramref name="version"/>; otherwise, false.</returns>
        public static bool IsMappedTo( this ActionDescriptor action, ApiVersion version )
        {
            Arg.NotNull( action, nameof( action ) );

            if ( version == null )
            {
                return false;
            }

            var model = action.GetProperty<ApiVersionModel>();

            if ( model == null )
            {
                return false;
            }

            return model.DeclaredApiVersions.Contains( version );
        }

        /// <summary>
        /// Gets a value indicating whether the provided action is API version-neutral.
        /// </summary>
        /// <param name="action">The <see cref="ActionDescriptor">action</see> to evaluate.</param>
        /// <returns>True if the action is API version-neutral; otherwise, false.</returns>
        public static bool IsApiVersionNeutral( this ActionDescriptor action )
        {
            Arg.NotNull( action, nameof( action ) );

            var model = action.GetProperty<ApiVersionModel>();
            return model == null || model.IsApiVersionNeutral;
        }
    }
}