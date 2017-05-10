namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Routing;
    using System;

    /// <summary>
    /// Represents a matched action descriptor.
    /// </summary>
    [CLSCompliant( false )]
    public class ActionDescriptorMatch : IEquatable<ActionDescriptorMatch>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionDescriptorMatch"/> class.
        /// </summary>
        /// <param name="action">The matched <see cref="ActionDescriptor">action</see>.</param>
        /// <param name="routeData">The <see cref="RouteData">route data</see> for the matched <paramref name="action"/>.</param>
        public ActionDescriptorMatch( ActionDescriptor action, RouteData routeData )
        {
            Arg.NotNull( action, nameof( action ) );
            Arg.NotNull( routeData, nameof( routeData ) );

            Action = action;
            RouteData = routeData;
        }

        /// <summary>
        /// Gets the matched action.
        /// </summary>
        /// <value>The matched <see cref="ActionDescriptor">action</see>.</value>
        public ActionDescriptor Action { get; }

        /// <summary>
        /// Gets the route data for the matched action.
        /// </summary>
        /// <value>The <see cref="RouteData">route data</see> for the matched <see cref="Action">action</see>.</value>
        public RouteData RouteData { get; }

        /// <summary>
        /// Determines whether the current object equals the specified object.
        /// </summary>
        /// <param name="other">The object to evaluate.</param>
        /// <returns>True if the current object equals the other object; otherwise, false.</returns>
        public virtual bool Equals( ActionDescriptorMatch other ) => Equals( Action, other?.Action );

        /// <summary>
        /// Determines whether the current object equals the specified object.
        /// </summary>
        /// <param name="obj">The object to evaluate.</param>
        /// <returns>True if the current object equals the other object; otherwise, false.</returns>
        public override bool Equals( object obj ) => Equals( obj as ActionDescriptorMatch );

        /// <summary>
        /// Gets hash code for the current to object.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode() => Action.GetHashCode();

        /// <summary>
        /// Determines whether two objects are equal.
        /// </summary>
        /// <param name="match1">The first object to compare.</param>
        /// <param name="match2">The second object to compare against.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public static bool operator ==( ActionDescriptorMatch match1, ActionDescriptorMatch match2 )
        {
            if ( ReferenceEquals( match1, null ) )
            {
                return ReferenceEquals( match2, null );
            }
            else if ( ReferenceEquals( match2, null ) )
            {
                return false;
            }

            return Equals( match1.Action, match2.Action );
        }

        /// <summary>
        /// Determines whether two objects are not equal.
        /// </summary>
        /// <param name="match1">The first object to compare.</param>
        /// <param name="match2">The second object to compare against.</param>
        /// <returns>True if the objects are not equal; otherwise, false.</returns>
        public static bool operator !=( ActionDescriptorMatch match1, ActionDescriptorMatch match2 )
        {
            if ( ReferenceEquals( match1, null ) )
            {
                return !ReferenceEquals( match2, null );
            }
            else if ( ReferenceEquals( match2, null ) )
            {
                return true;
            }

            return !Equals( match1.Action, match2.Action );
        }
    }
}