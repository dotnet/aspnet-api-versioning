namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    using System;

    /// <summary>
    /// Represents a model for the API Explorer for an OData controller or action.
    /// </summary>
    [CLSCompliant( false )]
    public class ODataApiExplorerModel : ApiExplorerModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataApiExplorerModel"/> class.
        /// </summary>
        public ODataApiExplorerModel() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataApiExplorerModel"/> class.
        /// </summary>
        /// <param name="other">The other <see cref="ApiExplorerModel">model</see> to copy from.</param>
        public ODataApiExplorerModel( ApiExplorerModel other ) : base( other )
        {
            if ( other == null )
            {
                throw new ArgumentNullException( nameof( other ) );
            }

            IsVisible = null;
            IsODataVisible = other.IsVisible;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataApiExplorerModel"/> class.
        /// </summary>
        /// <param name="other">The other <see cref="ODataApiExplorerModel">model</see> to copy from.</param>
        public ODataApiExplorerModel( ODataApiExplorerModel other ) : base( other )
        {
            if ( other == null )
            {
                throw new ArgumentNullException( nameof( other ) );
            }

            IsVisible = null;
            IsODataVisible = other.IsODataVisible;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the OData controller or action will be visible in the API Explorer.
        /// </summary>
        /// <value>True if the OData controller or action will be visible in the API Explorer; otherwise, false.</value>
        public bool? IsODataVisible { get; set; }
    }
}