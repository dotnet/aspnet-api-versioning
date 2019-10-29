namespace Microsoft.Web.Http.Description
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Web.Http.Description;
    using static System.Linq.Expressions.Expression;

    /// <summary>
    /// Represents a versioned API description.
    /// </summary>
    [DebuggerDisplay( "{DebuggerDisplay,nq}" )]
    public class VersionedApiDescription : ApiDescription
    {
        static readonly Lazy<Action<ApiDescription, ResponseDescription>> setResponseDescription =
            new Lazy<Action<ApiDescription, ResponseDescription>>( CreateSetResponseDescriptionMutator );

        /// <summary>
        /// Gets or sets the name of the group for the API description.
        /// </summary>
        /// <value>The API version description group name.</value>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the API version.
        /// </summary>
        /// <value>The described <see cref="Http.ApiVersion">API version</see>.</value>
        public ApiVersion ApiVersion { get; set; } = default!;

        /// <summary>
        /// Gets or sets a value indicating whether API is deprecated.
        /// </summary>
        /// <value>True if the API is deprecated; otherwise, false. The default value is <c>false</c>.</value>
        public bool IsDeprecated { get; set; }

        /// <summary>
        /// Gets or sets the response description.
        /// </summary>
        /// <value>The <see cref="ResponseDescription">response description</see>.</value>
        public new ResponseDescription ResponseDescription
        {
            get => base.ResponseDescription;
            set => setResponseDescription.Value( this, value ); // HACK: the base setter is only internally assignable
        }

        /// <summary>
        /// Gets arbitrary metadata properties associated with the API description.
        /// </summary>
        /// <value>A <see cref="IDictionary{TKey, TValue}">collection</see> of arbitrary metadata properties
        /// associated with the <see cref="VersionedApiDescription">API description</see>.</value>
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        static Action<ApiDescription, ResponseDescription> CreateSetResponseDescriptionMutator()
        {
            var api = Parameter( typeof( ApiDescription ), "api" );
            var value = Parameter( typeof( ResponseDescription ), "value" );
            var property = Property( api, nameof( ResponseDescription ) );
            var body = Assign( property, value );
            var lambda = Lambda<Action<ApiDescription, ResponseDescription>>( body, api, value );

            return lambda.Compile();
        }

        string DebuggerDisplay => $"{HttpMethod.Method} {RelativePath} ({ApiVersion})";
    }
}