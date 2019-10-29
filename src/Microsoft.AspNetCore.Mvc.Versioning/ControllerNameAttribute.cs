namespace Microsoft.AspNetCore.Mvc
{
    using Microsoft.AspNetCore.Mvc.Routing;
    using System;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public sealed partial class ControllerNameAttribute : RouteValueAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerNameAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the controller.</param>
        public ControllerNameAttribute( string name ) : base( "controller", name ) => Name = name;
    }
}