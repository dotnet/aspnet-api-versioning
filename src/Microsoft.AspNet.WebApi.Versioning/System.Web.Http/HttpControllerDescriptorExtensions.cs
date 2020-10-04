namespace System.Web.Http
{
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Controllers;
    using Microsoft.Web.Http.Versioning;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Web.Http.Controllers;
    using System.Web.Http.Description;
    using static System.ComponentModel.EditorBrowsableState;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpControllerDescriptor"/> class.
    /// </summary>
    public static class HttpControllerDescriptorExtensions
    {
        const string AttributeRoutedPropertyKey = "MS_IsAttributeRouted";
        const string PossibleControllerCandidatesKey = "MS_PossibleControllerCandidates";

        /// <summary>
        /// Gets the API version information associated with a controller.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller</see> to evaluate.</param>
        /// <returns>The <see cref="ApiVersionModel">API version information</see> for the controller.</returns>
        /// <remarks>A controller only contains implicitly declared API versions relative to an action. Most scenarios
        /// should use <see cref="HttpActionDescriptorExtensions.GetApiVersionModel(HttpActionDescriptor)"/> or
        /// <see cref="HttpActionDescriptorExtensions.MappingTo(HttpActionDescriptor, ApiVersion)"/> instead. Components
        /// such as the <see cref="IApiExplorer"/> may need to know API versions declared by an action's defining controller.</remarks>
        [EditorBrowsable( Never )]
        public static ApiVersionModel GetApiVersionModel( this HttpControllerDescriptor controllerDescriptor ) =>
            controllerDescriptor.GetProperty<ApiVersionModel>() ?? ApiVersionModel.Empty;

        /// <summary>
        /// Enumerates a controller descriptor as a sequence of descriptors.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller descriptor</see> to enumerate.</param>
        /// <returns>A <see cref="IEnumerable{T}">sequence</see> of <see cref="HttpControllerDescriptor">controller descriptors</see>.</returns>
        /// <remarks>This method will flatten a sequence of composite descriptors such as <see cref="HttpControllerDescriptorGroup"/>.
        /// If the <paramref name="controllerDescriptor">controller descriptor</paramref> is not a composite, it yields itself.</remarks>
        public static IEnumerable<HttpControllerDescriptor> AsEnumerable( this HttpControllerDescriptor controllerDescriptor ) =>
            AsEnumerable( controllerDescriptor, includeCandidates: false );

        internal static IEnumerable<HttpControllerDescriptor> AsEnumerable( this HttpControllerDescriptor controllerDescriptor, bool includeCandidates )
        {
            if ( controllerDescriptor == null )
            {
                throw new ArgumentNullException( nameof( controllerDescriptor ) );
            }

            var visited = new HashSet<HttpControllerDescriptor>();

            if ( controllerDescriptor is IEnumerable<HttpControllerDescriptor> groupedDescriptors )
            {
                foreach ( var groupedDescriptor in groupedDescriptors )
                {
                    if ( visited.Add( groupedDescriptor ) )
                    {
                        yield return groupedDescriptor;
                    }
                }
            }
            else
            {
                visited.Add( controllerDescriptor );
                yield return controllerDescriptor;
            }

            if ( !includeCandidates || !controllerDescriptor.Properties.TryGetValue( PossibleControllerCandidatesKey, out IEnumerable<HttpControllerDescriptor> candidates ) )
            {
                yield break;
            }

            foreach ( var candidate in candidates )
            {
                if ( visited.Add( candidate ) )
                {
                    yield return candidate;
                }
            }

            visited.Clear();
        }

        internal static void SetApiVersionModel( this HttpControllerDescriptor controller, ApiVersionModel value ) => controller.SetProperty( value );

        internal static bool IsAttributeRouted( this HttpControllerDescriptor controller )
        {
            controller.Properties.TryGetValue( AttributeRoutedPropertyKey, out bool? value );
            return value ?? false;
        }

        internal static void SetPossibleCandidates( this HttpControllerDescriptor controllerDescriptor, IEnumerable<HttpControllerDescriptor> value ) =>
            controllerDescriptor.Properties.AddOrUpdate( PossibleControllerCandidatesKey, value, ( key, oldValue ) => value );

        static T GetProperty<T>( this HttpControllerDescriptor controller )
        {
            if ( controller == null )
            {
                throw new ArgumentNullException( nameof( controller ) );
            }

            if ( controller.Properties.TryGetValue( typeof( T ), out T value ) )
            {
                return value;
            }

            return default!;
        }

        static void SetProperty<T>( this HttpControllerDescriptor controller, T value )
        {
            controller.Properties.AddOrUpdate( typeof( T ), value, ( key, oldValue ) => value );

            if ( controller is IEnumerable<HttpControllerDescriptor> groupedControllerDescriptors )
            {
                foreach ( var groupedControllerDescriptor in groupedControllerDescriptors )
                {
                    groupedControllerDescriptor.Properties.AddOrUpdate( typeof( T ), value, ( key, oldValue ) => value );
                }
            }
        }
    }
}