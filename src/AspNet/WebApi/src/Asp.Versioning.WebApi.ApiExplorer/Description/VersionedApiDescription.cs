// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Description;

using System.Diagnostics;
using System.Web.Http.Description;
using static System.Linq.Expressions.Expression;

/// <summary>
/// Represents a versioned API description.
/// </summary>
[DebuggerDisplay( "{DebuggerDisplay,nq}" )]
public class VersionedApiDescription : ApiDescription
{
    private static Action<ApiDescription, ResponseDescription>? setResponseDescription;
    private string? groupName;
    private ApiVersion? apiVersion;
    private Dictionary<object, object>? properties;

    /// <summary>
    /// Gets or sets the name of the group for the API description.
    /// </summary>
    /// <value>The API version description group name.</value>
    public string GroupName
    {
        get => groupName ??= string.Empty;
        set => groupName = value;
    }

    /// <summary>
    /// Gets or sets the API version.
    /// </summary>
    /// <value>The described <see cref="ApiVersion">API version</see>.</value>
    public ApiVersion ApiVersion
    {
        get => apiVersion ??= ApiVersion.Neutral;
        set => apiVersion = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether API is deprecated.
    /// </summary>
    /// <value>True if the API is deprecated; otherwise, false. The default value is <c>false</c>.</value>
    public bool IsDeprecated { get; set; }

    /// <summary>
    /// Gets or sets the described API sunset policy.
    /// </summary>
    /// <value>The defined <see cref="Versioning.SunsetPolicy">sunset policy</see> defined for the API, if any.</value>
    public SunsetPolicy? SunsetPolicy { get; set; }

    /// <summary>
    /// Gets or sets the response description.
    /// </summary>
    /// <value>The <see cref="ResponseDescription">response description</see>.</value>
    public new ResponseDescription ResponseDescription
    {
        get => base.ResponseDescription;
        set
        {
            // HACK: the base setter is only internally assignable
            setResponseDescription ??= CreateSetResponseDescriptionMutator();
            setResponseDescription( this, value );
        }
    }

    /// <summary>
    /// Gets arbitrary metadata properties associated with the API description.
    /// </summary>
    /// <value>A <see cref="IDictionary{TKey, TValue}">collection</see> of arbitrary metadata properties
    /// associated with the <see cref="VersionedApiDescription">API description</see>.</value>
    public IDictionary<object, object> Properties => properties ??= [];

    private static Action<ApiDescription, ResponseDescription> CreateSetResponseDescriptionMutator()
    {
        var api = Parameter( typeof( ApiDescription ), "api" );
        var value = Parameter( typeof( ResponseDescription ), "value" );
        var property = Property( api, nameof( ResponseDescription ) );
        var body = Assign( property, value );
        var lambda = Lambda<Action<ApiDescription, ResponseDescription>>( body, api, value );

        return lambda.Compile();
    }

    private string DebuggerDisplay => $"{HttpMethod.Method} {RelativePath} ({ApiVersion})";
}