﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Controllers;

using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

/// <summary>
/// Represents a HTTP controller descriptor that is a grouped set of other HTTP controller descriptors.
/// </summary>
public class HttpControllerDescriptorGroup : HttpControllerDescriptor, IReadOnlyList<HttpControllerDescriptor>
{
    private readonly IReadOnlyList<HttpControllerDescriptor> descriptors;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpControllerDescriptorGroup"/> class.
    /// </summary>
    /// <param name="controllerDescriptors">An <see cref="Array">array</see> of
    /// <see cref="HttpControllerDescriptor">HTTP controller descriptors</see>.</param>
    public HttpControllerDescriptorGroup( params HttpControllerDescriptor[] controllerDescriptors ) => descriptors = controllerDescriptors;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpControllerDescriptorGroup"/> class.
    /// </summary>
    /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> associated with the controller descriptor.</param>
    /// <param name="controllerName">The name of the controller the controller descriptor represents.</param>
    /// <param name="controllerDescriptors">An <see cref="Array">array</see> of
    /// <see cref="HttpControllerDescriptor">HTTP controller descriptors</see>.</param>
    public HttpControllerDescriptorGroup( HttpConfiguration configuration, string controllerName, params HttpControllerDescriptor[] controllerDescriptors )
        : base( configuration, controllerName, controllerDescriptors?[0].ControllerType ) =>
        descriptors = controllerDescriptors ?? throw new System.ArgumentNullException( nameof( controllerDescriptors ) );

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpControllerDescriptorGroup"/> class.
    /// </summary>
    /// <param name="controllerDescriptors">A <see cref="IReadOnlyList{T}">read-only list</see> of
    /// <see cref="HttpControllerDescriptor">HTTP controller descriptors</see>.</param>
    public HttpControllerDescriptorGroup( IReadOnlyList<HttpControllerDescriptor> controllerDescriptors ) => descriptors = controllerDescriptors;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpControllerDescriptorGroup"/> class.
    /// </summary>
    /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> associated with the controller descriptor.</param>
    /// <param name="controllerName">The name of the controller the controller descriptor represents.</param>
    /// <param name="controllerDescriptors">A <see cref="IReadOnlyList{T}">read-only list</see> of
    /// <see cref="HttpControllerDescriptor">HTTP controller descriptors</see>.</param>
    public HttpControllerDescriptorGroup( HttpConfiguration configuration, string controllerName, IReadOnlyList<HttpControllerDescriptor> controllerDescriptors )
        : base( configuration, controllerName, controllerDescriptors?[0].ControllerType ) =>
        descriptors = controllerDescriptors ?? throw new System.ArgumentNullException( nameof( controllerDescriptors ) );

    /// <summary>
    /// Creates and returns a controller for the specified request.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage">request</see> to create a controller for.</param>
    /// <returns>A new <see cref="IHttpController">controller</see> instance.</returns>
    /// <remarks>The default implementation matches the <see cref="ApiVersion">API version</see> specified in the
    /// <paramref name="request"/> to the <see cref="HttpControllerDescriptor">controller descriptor</see> with
    /// the matching, declared version. If a version was not specified in the <paramref name="request"/> or none of
    /// the <see cref="HttpControllerDescriptor">controller descriptors</see> match the requested version, then
    /// the <see cref="IHttpController">controller</see> is created using the first item in the group.</remarks>
    public override IHttpController CreateController( HttpRequestMessage request )
    {
        var properties = request.ApiVersionProperties();

        if ( properties.SelectedController is HttpControllerDescriptor descriptor )
        {
            return descriptor.CreateController( request );
        }

        var url = request.RequestUri.SafeFullPath();
        var message = string.Format( CultureInfo.CurrentCulture, SR.NoControllerSelected, url, properties.RawRequestedApiVersion );
        throw new InvalidOperationException( message );
    }

    /// <summary>
    /// Returns a collection of attributes for the controller descriptor.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type">type</see> of attribute requested.</typeparam>
    /// <param name="inherit">Indicates whether to search the controller descriptor's inheritance chain.</param>
    /// <returns>A <see cref="Collection{T}">collection</see> of the requested <typeparamref name="T">attributes</typeparamref>.</returns>
    /// <remarks>The default implementation aggregates the matching attributes from all
    /// <see cref="HttpControllerDescriptor">controller descriptors</see> in the group.</remarks>
    public override Collection<T> GetCustomAttributes<T>( bool inherit )
    {
        var attributes = new HashSet<T>();

        for ( var i = 0; i < descriptors.Count; i++ )
        {
            attributes.UnionWith( descriptors[i].GetCustomAttributes<T>( inherit ) );
        }

        return new( attributes.ToList() );
    }

    /// <summary>
    /// Returns a collection of filters for the controller descriptor.
    /// </summary>
    /// <returns>A <see cref="Collection{T}">collection</see> of <see cref="IFilter">filters</see>.</returns>
    /// <remarks>The default implementation aggregates the filters from all
    /// <see cref="HttpControllerDescriptor">controller descriptors</see> in the group.</remarks>
    public override Collection<IFilter> GetFilters()
    {
        var filters = new HashSet<IFilter>();

        for ( var i = 0; i < descriptors.Count; i++ )
        {
            filters.UnionWith( descriptors[i].GetFilters() );
        }

        return new( filters.ToList() );
    }

    /// <summary>
    /// Returns an iterator that can be used to enumerate the controller descriptors in the group.
    /// </summary>
    /// <returns>An <see cref="IEnumerator{T}">enumerator</see> for a sequence of
    /// <see cref="HttpControllerDescriptor">controller descriptors</see>.</returns>
    public IEnumerator<HttpControllerDescriptor> GetEnumerator() => descriptors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Gets the item in the group at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to retrieve.</param>
    /// <returns>The <see cref="HttpControllerDescriptor">controller descriptor</see> at the specified <paramref name="index"/>.</returns>
    public HttpControllerDescriptor this[int index] => descriptors[index];

    /// <summary>
    /// Gets the number of items in the group.
    /// </summary>
    /// <value>The total number of items in the group.</value>
    public int Count => descriptors.Count;
}