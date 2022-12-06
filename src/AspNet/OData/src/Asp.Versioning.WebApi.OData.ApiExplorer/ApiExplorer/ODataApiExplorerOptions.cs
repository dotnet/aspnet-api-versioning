// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNet.OData;
using System.Web.Http;
using System.Web.Http.Description;

/// <content>
/// Provides additional implementation specific to ASP.NET Web API.
/// </content>
public partial class ODataApiExplorerOptions : ApiExplorerOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ODataApiExplorerOptions"/> class.
    /// </summary>
    /// <param name="configuration">The current <see cref="HttpConfiguration">configuration</see> associated with the options.</param>
    public ODataApiExplorerOptions( HttpConfiguration configuration )
        : base( configuration ) => AdHocModelBuilder = new( configuration );

    /// <summary>
    /// Gets or sets a value indicating whether the API explorer settings are honored.
    /// </summary>
    /// <value>True if the <see cref="ApiExplorerSettingsAttribute"/> is ignored; otherwise, false.
    /// The default value is <c>false</c>.</value>
    /// <remarks>Most OData services inherit from the <see cref="ODataController"/>, which excludes the controller
    /// from the API explorer by setting <see cref="ApiExplorerSettingsAttribute.IgnoreApi"/>
    /// to <c>true</c>. By setting this property to <c>false</c>, these settings are ignored instead of reapplying
    /// <see cref="ApiExplorerSettingsAttribute.IgnoreApi"/> with a value of <c>false</c> to all OData controllers.</remarks>
    public bool UseApiExplorerSettings { get; set; }
}