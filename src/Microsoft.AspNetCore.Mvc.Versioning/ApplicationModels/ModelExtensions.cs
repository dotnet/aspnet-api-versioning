namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    using System;
    using System.Linq;
    using Versioning;

    /// <summary>
    /// Provides extension methods for <see cref="ApplicationModel">application models</see>, <see cref="ControllerModel">controller models</see>,
    /// and <see cref="ActionModel">action models</see>.
    /// </summary>
    [CLSCompliant( false )]
    public static class ModelExtensions
    {
        /// <summary>
        /// Gets the property associated with the controller model.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type">type</see> and key of the property.</typeparam>
        /// <param name="controller">The <see cref="ControllerModel">model</see> to get the property from.</param>
        /// <returns>The property value of <typeparamref name="T"/> or its default value.</returns>
        public static T GetProperty<T>( this ControllerModel controller ) where T : class
        {
            Arg.NotNull( controller, nameof( controller ) );
            return controller.Properties.GetOrDefault( typeof( T ), default( T ) );
        }

        /// <summary>
        /// Sets the property associated with the controller model.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type">type</see> and key of the property.</typeparam>
        /// <param name="controller">The <see cref="ControllerModel">model</see> to set the property for.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetProperty<T>( this ControllerModel controller, T value ) where T : class
        {
            Arg.NotNull( controller, nameof( controller ) );
            controller.Properties.SetOrRemove( typeof( T ), value );
        }

        /// <summary>
        /// Gets the property associated with the action model.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type">type</see> and key of the property.</typeparam>
        /// <param name="action">The <see cref="ActionModel">model</see> to get the property from.</param>
        /// <returns>The property value of <typeparamref name="T"/> or its default value.</returns>
        public static T GetProperty<T>( this ActionModel action ) where T : class
        {
            Arg.NotNull( action, nameof( action ) );
            return action.Properties.GetOrDefault( typeof( T ), default( T ) );
        }

        /// <summary>
        /// Sets the property associated with the action model.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type">type</see> and key of the property.</typeparam>
        /// <param name="action">The <see cref="ActionModel">model</see> to set the property for.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetProperty<T>( this ActionModel action, T value ) where T : class
        {
            Arg.NotNull( action, nameof( action ) );
            action.Properties.SetOrRemove( typeof( T ), value );
        }
    }
}