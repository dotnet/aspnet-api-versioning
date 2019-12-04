namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    using System;
    using System.Diagnostics.CodeAnalysis;

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
#if NETCOREAPP3_1
        [return: MaybeNull]
        public static T GetProperty<T>( this ControllerModel controller )
#else
        public static T GetProperty<T>( this ControllerModel controller )
#endif
        {
            if ( controller == null )
            {
                throw new ArgumentNullException( nameof( controller ) );
            }

            return controller.Properties.GetOrDefault( typeof( T ), default( T )! );
        }

        /// <summary>
        /// Sets the property associated with the controller model.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type">type</see> and key of the property.</typeparam>
        /// <param name="controller">The <see cref="ControllerModel">model</see> to set the property for.</param>
        /// <param name="value">The property value to set.</param>
#if NETCOREAPP3_1
        public static void SetProperty<T>( this ControllerModel controller, [AllowNull] T value )
#else
        public static void SetProperty<T>( this ControllerModel controller, T value )
#endif
        {
            if ( controller == null )
            {
                throw new ArgumentNullException( nameof( controller ) );
            }

            controller.Properties.SetOrRemove( typeof( T ), value );
        }

        /// <summary>
        /// Gets the property associated with the action model.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type">type</see> and key of the property.</typeparam>
        /// <param name="action">The <see cref="ActionModel">model</see> to get the property from.</param>
        /// <returns>The property value of <typeparamref name="T"/> or its default value.</returns>
#if NETCOREAPP3_1
        [return: MaybeNull]
        public static T GetProperty<T>( this ActionModel action )
#else
        public static T GetProperty<T>( this ActionModel action )
#endif
        {
            if ( action == null )
            {
                throw new ArgumentNullException( nameof( action ) );
            }

            return action.Properties.GetOrDefault( typeof( T ), default( T )! );
        }

        /// <summary>
        /// Sets the property associated with the action model.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type">type</see> and key of the property.</typeparam>
        /// <param name="action">The <see cref="ActionModel">model</see> to set the property for.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetProperty<T>( this ActionModel action, T value )
        {
            if ( action == null )
            {
                throw new ArgumentNullException( nameof( action ) );
            }

            action.Properties.SetOrRemove( typeof( T ), value );
        }
    }
}