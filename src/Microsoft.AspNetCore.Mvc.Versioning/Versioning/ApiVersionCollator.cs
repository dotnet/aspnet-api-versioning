namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents an object that collates <see cref="ApiVersion">API versions</see> per <see cref="ActionDescriptor">action</see>.
    /// </summary>
    [CLSCompliant( false )]
    public class ApiVersionCollator : IActionDescriptorProvider
    {
        readonly IOptions<ApiVersioningOptions> options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionCollator"/> class.
        /// </summary>
        /// <param name="options">The current <see cref="ApiVersioningOptions">API versioning options</see>.</param>
        public ApiVersionCollator( IOptions<ApiVersioningOptions> options ) => this.options = options;

        /// <summary>
        /// Gets the API versioning options associated with the collator.
        /// </summary>
        /// <value>The current <see cref="ApiVersioningOptions">API versioning options</see>.</value>
        protected ApiVersioningOptions Options => options.Value;

        /// <inheritdoc />
        public int Order { get; protected set; }

        /// <inheritdoc />
        public virtual void OnProvidersExecuted( ActionDescriptorProviderContext context )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            foreach ( var actions in GroupActionsByController( context.Results ) )
            {
                var collatedModel = CollateModel( actions );

                foreach ( var action in actions )
                {
                    var model = action.GetProperty<ApiVersionModel>();

                    if ( model != null && !model.IsApiVersionNeutral )
                    {
                        action.SetProperty( model.Aggregate( collatedModel ) );
                    }
                }
            }
        }

        /// <inheritdoc />
        public virtual void OnProvidersExecuting( ActionDescriptorProviderContext context ) { }

        /// <summary>
        /// Resolves and returns the logical controller name for the specified action.
        /// </summary>
        /// <param name="action">The <see cref="ActionDescriptor">action</see> to get the controller name from.</param>
        /// <returns>The logical name of the associated controller.</returns>
        /// <remarks>
        /// <para>
        /// The logical controller name is used to collate actions together and aggregate API versions. The
        /// default implementation uses the "controller" route parameter and falls back to the
        /// <see cref="ControllerActionDescriptor.ControllerName"/> property when available.
        /// </para>
        /// <para>
        /// The default implementation will also trim trailing numbers in the controller name by convention. For example,
        /// the type "Values2Controller" will have the controller name "Values2", which will be trimmed to just "Values".
        /// This behavior can be changed by using the <see cref="ControllerNameAttribute"/> or overriding the default
        /// implementation.
        /// </para>
        /// </remarks>
        protected virtual string GetControllerName( ActionDescriptor action )
        {
            if ( action == null )
            {
                throw new ArgumentNullException( nameof( action ) );
            }

            if ( !action.RouteValues.TryGetValue( "controller", out var key ) )
            {
                if ( action is ControllerActionDescriptor controllerAction )
                {
                    key = controllerAction.ControllerName;
                }
            }

            return TrimTrailingNumbers( key );
        }

        IEnumerable<IEnumerable<ActionDescriptor>> GroupActionsByController( IEnumerable<ActionDescriptor> actions )
        {
            var groups = new Dictionary<string, List<ActionDescriptor>>( StringComparer.OrdinalIgnoreCase );

            foreach ( var action in actions )
            {
                var key = GetControllerName( action );

                if ( string.IsNullOrEmpty( key ) )
                {
                    continue;
                }

                if ( !groups.TryGetValue( key, out var values ) )
                {
                    groups.Add( key, values = new List<ActionDescriptor>() );
                }

                values.Add( action );
            }

            foreach ( var value in groups.Values )
            {
                yield return value;
            }
        }

        static string TrimTrailingNumbers( string? name )
        {
            if ( string.IsNullOrEmpty( name ) )
            {
                return string.Empty;
            }

            var last = name!.Length - 1;

            for ( var i = last; i >= 0; i-- )
            {
                if ( !char.IsNumber( name[i] ) )
                {
                    if ( i < last )
                    {
                        return name.Substring( 0, i + 1 );
                    }

                    return name;
                }
            }

            return name;
        }

        static ApiVersionModel CollateModel( IEnumerable<ActionDescriptor> actions ) => actions.Select( a => a.GetApiVersionModel() ).Aggregate();
    }
}