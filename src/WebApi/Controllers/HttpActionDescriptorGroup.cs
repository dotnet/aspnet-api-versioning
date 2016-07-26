namespace Microsoft.Web.Http.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    /// <summary>
    /// Represents a HTTP action descriptor that is a grouped set of other HTTP action descriptors.
    /// </summary>
    public class HttpActionDescriptorGroup : HttpActionDescriptor, IReadOnlyList<HttpActionDescriptor>
    {
        private readonly HttpActionDescriptor firstDescriptor;
        private readonly IReadOnlyList<HttpActionDescriptor> descriptors;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpActionDescriptorGroup"/> class.
        /// </summary>
        /// <param name="actionDescriptors">An <see cref="Array">array</see> of
        /// <see cref="HttpActionDescriptor">HTTP action descriptors</see>.</param>
        public HttpActionDescriptorGroup( params HttpActionDescriptor[] actionDescriptors )
        {
            Arg.NotNull( actionDescriptors, nameof( actionDescriptors ) );
            Arg.InRange( actionDescriptors.Length, 1, nameof( actionDescriptors ) );

            firstDescriptor = actionDescriptors[0];
            descriptors = actionDescriptors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpActionDescriptorGroup"/> class.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">HTTP controller descriptor</see>
        /// associated with the action descriptor.</param>
        /// <param name="actionDescriptors">An <see cref="Array">array</see> of
        /// <see cref="HttpActionDescriptor">HTTP action descriptors</see>.</param>
        public HttpActionDescriptorGroup( HttpControllerDescriptor controllerDescriptor, params HttpActionDescriptor[] actionDescriptors )
            : base( controllerDescriptor )
        {
            Arg.NotNull( controllerDescriptor, nameof( controllerDescriptor ) );
            Arg.NotNull( actionDescriptors, nameof( actionDescriptors ) );
            Arg.InRange( actionDescriptors.Length, 1, nameof( actionDescriptors ) );

            firstDescriptor = actionDescriptors[0];
            descriptors = actionDescriptors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpActionDescriptorGroup"/> class.
        /// </summary>
        /// <param name="actionDescriptors">A <see cref="IReadOnlyList{T}">read-only list</see> of
        /// <see cref="HttpActionDescriptor">HTTP action descriptors</see>.</param>
        public HttpActionDescriptorGroup( IReadOnlyList<HttpActionDescriptor> actionDescriptors )
        {
            Arg.NotNull( actionDescriptors, nameof( actionDescriptors ) );
            Arg.InRange( actionDescriptors.Count, 1, nameof( actionDescriptors ) );

            firstDescriptor = actionDescriptors[0];
            descriptors = actionDescriptors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpActionDescriptorGroup"/> class.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">HTTP controller descriptor</see>
        /// associated with the action descriptor.</param>
        /// <param name="actionDescriptors">A <see cref="IReadOnlyList{T}">read-only list</see> of
        /// <see cref="HttpActionDescriptor">HTTP action descriptors</see>.</param>
        public HttpActionDescriptorGroup( HttpControllerDescriptor controllerDescriptor, IReadOnlyList<HttpActionDescriptor> actionDescriptors )
            : base( controllerDescriptor )
        {
            Arg.NotNull( controllerDescriptor, nameof( controllerDescriptor ) );
            Arg.NotNull( actionDescriptors, nameof( actionDescriptors ) );
            Arg.InRange( actionDescriptors.Count, 1, nameof( actionDescriptors ) );

            firstDescriptor = actionDescriptors[0];
            descriptors = actionDescriptors;
        }

        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        /// <value>The name of the action.</value>
        public override string ActionName => firstDescriptor.ActionName;

        /// <summary>
        /// Executes the described action and returns the result.
        /// </summary>
        /// <param name="controllerContext">The <see cref="HttpControllerContext">controller context</see>.</param>
        /// <param name="arguments">A <see cref="IDictionary{TKey,TValue}">collection</see> of arguments.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken">cancellation token</see> that can be used
        /// to cancel the operation.</param>
        /// <returns>A <see cref="Task{T}">task</see> that once completed will contain the return value of the action.</returns>
        public override Task<object> ExecuteAsync( HttpControllerContext controllerContext, IDictionary<string, object> arguments, CancellationToken cancellationToken ) =>
            firstDescriptor.ExecuteAsync( controllerContext, arguments, cancellationToken );

        /// <summary>
        /// Returns a collection of attributes for the action descriptor.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type">type</see> of attribute requested.</typeparam>
        /// <param name="inherit">Indicates whether to search the action descriptor's inheritance chain.</param>
        /// <returns>A <see cref="Collection{T}">collection</see> of the requested <typeparamref name="T">attributes</typeparamref>.</returns>
        /// <remarks>The default implementation aggregates the matching attributes from all
        /// <see cref="HttpActionDescriptor">action descriptors</see> in the group.</remarks>
        public override Collection<T> GetCustomAttributes<T>( bool inherit )
        {
            var attributes = new List<T>();

            foreach ( var descriptor in descriptors )
            {
                attributes.AddRange( descriptor.GetCustomAttributes<T>( inherit ) );
            }

            return new Collection<T>( attributes.Distinct().ToList() );
        }

        private static bool AllowMultiple( object filterInstance ) => ( filterInstance as IFilter )?.AllowMultiple ?? true;

        /// <summary>
        /// Retrieves the filters for the given configuration and action.
        /// </summary>
        /// <returns>The <see cref="Collection{T}">collection</see> of <see cref="FilterInfo">filters</see> for the given
        /// configuration and action.</returns>
        /// <remarks>The default implementation aggregates the filters from all
        /// <see cref="HttpActionDescriptor">action descriptors</see> in the group.</remarks>
        public override Collection<FilterInfo> GetFilterPipeline()
        {
            var typeSet = new HashSet<Type>();
            var filters = new List<FilterInfo>();

            foreach ( var descriptor in descriptors )
            {
                foreach ( var filter in descriptor.GetFilterPipeline() )
                {
                    var instance = filter.Instance;
                    var type = instance.GetType();

                    if ( !typeSet.Contains( type ) || AllowMultiple( instance ) )
                    {
                        filters.Add( filter );
                        typeSet.Add( type );
                    }
                }
            }

            return new Collection<FilterInfo>( filters.ToList() );
        }

        /// <summary>
        /// Returns a collection of filters for the action descriptor.
        /// </summary>
        /// <returns>A <see cref="Collection{T}">collection</see> of <see cref="IFilter">filters</see>.</returns>
        /// <remarks>The default implementation aggregates the filters from all
        /// <see cref="HttpActionDescriptor">action descriptors</see> in the group.</remarks>
        public override Collection<IFilter> GetFilters()
        {
            var filters = new List<IFilter>();

            foreach ( var descriptor in descriptors )
            {
                filters.AddRange( descriptor.GetFilters() );
            }

            return new Collection<IFilter>( filters.Distinct().ToList() );
        }

        /// <summary>
        /// Retrieves the parameters for the action descriptor.
        /// </summary>
        /// <returns>The <see cref="Collection{T}">collection</see> of <see cref="HttpParameterDescriptor">parameters</see>
        /// for the action descriptor.</returns>
        public override Collection<HttpParameterDescriptor> GetParameters() => firstDescriptor.GetParameters();

        /// <summary>
        /// Gets or sets the binding that describes the action.
        /// </summary>
        /// <value>The <see cref="HttpActionBinding">binding</see> that describes the action.</value>
        /// <remarks>This action descriptor represents a group; therefore, the action binding cannot be set.</remarks>
        /// <exception cref="InvalidOperationException">Occurs during any attempt to set this property.</exception>
        public override HttpActionBinding ActionBinding
        {
            get
            {
                return firstDescriptor.ActionBinding;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Gets the converter for correctly transforming the result of calling <see cref="ExecuteAsync">execute</see>.
        /// </summary>
        /// <value>The <see cref="IActionResultConverter">action result converter</see>.</value>
        public override IActionResultConverter ResultConverter => firstDescriptor.ResultConverter;

        /// <summary>
        /// Gets the return type of the descriptor.
        /// </summary>
        /// <value>The return <see cref="Type">type</see> of the descriptor.</value>
        public override Type ReturnType => firstDescriptor.ReturnType;

        /// <summary>
        /// Gets the collection of supported HTTP methods for the descriptor.
        /// </summary>
        /// <value>The <see cref="Collection{T}">collection</see> of supported <see cref="HttpMethod">HTTP methods</see>
        /// for the descriptor.</value>
        public override Collection<HttpMethod> SupportedHttpMethods => firstDescriptor.SupportedHttpMethods;

        /// <summary>
        /// Returns an iterator that can be used to enumerate the action descriptors in the group.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}">enumerator</see> for a sequence of
        /// <see cref="HttpActionDescriptor">action descriptors</see>.</returns>
        public IEnumerator<HttpActionDescriptor> GetEnumerator() => descriptors.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets the item in the group at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to retrieve.</param>
        /// <returns>The <see cref="HttpActionDescriptor">action descriptor</see> at the specified <paramref name="index"/>.</returns>
        public HttpActionDescriptor this[int index] => descriptors[index];

        /// <summary>
        /// Gets the number of items in the group.
        /// </summary>
        /// <value>The total number of items in the group.</value>
        public int Count => descriptors.Count;
    }
}
