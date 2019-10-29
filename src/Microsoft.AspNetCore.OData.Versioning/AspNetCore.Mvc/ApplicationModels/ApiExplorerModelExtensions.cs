namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    using System;

    /// <summary>
    /// Provides extension methods for the <see cref="ApiExplorerModel"/> class.
    /// </summary>
    [CLSCompliant( false )]
    public static class ApiExplorerModelExtensions
    {
        /// <summary>
        /// Returns a value indicating whether the specified model is visible to OData.
        /// </summary>
        /// <param name="model">The <see cref="ApiExplorerModel">model</see> to evaluate.</param>
        /// <returns>True if the associated controller or action is visible to OData; otherwise, false.</returns>
        public static bool? IsODataVisible( this ApiExplorerModel model )
        {
            if ( model == null )
            {
                throw new ArgumentNullException( nameof( model ) );
            }

            return model is ODataApiExplorerModel odataModel ? odataModel.IsODataVisible : model.IsVisible;
        }
    }
}