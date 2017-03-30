namespace Microsoft.Web.Http.Description
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Web.Http.Description;
    using static System.ComponentModel.EditorBrowsableState;

    /// <summary>
    /// Represents a versioned API description.
    /// </summary>
    [DebuggerDisplay( "{DebuggerDisplay,nq}" )]
    public class VersionedApiDescription : ApiDescription
    {
        static readonly Lazy<Action<ApiDescription, ResponseDescription>> setResponseDescription =
            new Lazy<Action<ApiDescription, ResponseDescription>>( CreateSetResponseDescriptionMutator );

        /// <summary>
        /// Gets the unique API description identifier.
        /// </summary>
        /// <value>The unique identifier of the API description.</value>
        /// <remarks>This property should be preferred over <see cref="ID"/> because it will be unique by version.</remarks>
        public virtual string UniqueID => $"{base.ID}-{ApiVersion}";

        /// <summary>
        /// Gets the unique API description identifier.
        /// </summary>
        /// <value>The unique identifier of the API description.</value>
        [EditorBrowsable( Never )]
        new public string ID => UniqueID;

        /// <summary>
        /// Gets or sets the name of the group for the API description.
        /// </summary>
        /// <value>The API version description group name.</value>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the API version.
        /// </summary>
        /// <value>The described <see cref="Http.ApiVersion">API version</see>.</value>
        public ApiVersion ApiVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether API is deprecated.
        /// </summary>
        /// <value>True if the API is deprecated; otherwise, false. The default value is <c>false</c>.</value>
        public bool IsDeprecated { get; set; }

        /// <summary>
        /// Gets or sets the response description.
        /// </summary>
        /// <value>The <see cref="ResponseDescription">response description</see>.</value>
        new public ResponseDescription ResponseDescription
        {
            get
            {
                return base.ResponseDescription;
            }
            set
            {
                // HACK: the base setter is only internally assignable
                setResponseDescription.Value( this, value );
            }
        }

        static Action<ApiDescription, ResponseDescription> CreateSetResponseDescriptionMutator()
        {
            var api = Expression.Parameter( typeof( ApiDescription ), "api" );
            var value = Expression.Parameter( typeof( ResponseDescription ), "value" );
            var property = Expression.Property( api, nameof( ResponseDescription ) );
            var body = Expression.Assign( property, value );
            var lambda = Expression.Lambda<Action<ApiDescription, ResponseDescription>>( body, api, value );

            return lambda.Compile();
        }

        string DebuggerDisplay => $"{HttpMethod.Method} {RelativePath} ({ApiVersion})";
    }
}