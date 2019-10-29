namespace Microsoft.Web.Http.Controllers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    /// <content>
    /// Provides additional content for the <see cref="ApiVersionActionSelector"/> class.
    /// </content>
    public partial class ApiVersionActionSelector
    {
        sealed class CandidateHttpActionDescriptor : HttpActionDescriptor
        {
            internal CandidateHttpActionDescriptor( CandidateActionWithParams action )
            {
                CandidateAction = action;
                Configuration = action.ActionDescriptor.Configuration;
                ControllerDescriptor = action.ActionDescriptor.ControllerDescriptor;
            }

            internal HttpActionDescriptor Inner => CandidateAction.ActionDescriptor;

            internal CandidateActionWithParams CandidateAction { get; }

            public override HttpActionBinding ActionBinding
            {
                get => Inner.ActionBinding;
                set => Inner.ActionBinding = value;
            }

            public override string ActionName => Inner.ActionName;

            public override Task<object> ExecuteAsync( HttpControllerContext controllerContext, IDictionary<string, object> arguments, CancellationToken cancellationToken ) =>
                Inner.ExecuteAsync( controllerContext, arguments, cancellationToken );

            public override Collection<T> GetCustomAttributes<T>() => Inner.GetCustomAttributes<T>();

            public override Collection<T> GetCustomAttributes<T>( bool inherit ) => Inner.GetCustomAttributes<T>( inherit );

            public override Collection<FilterInfo> GetFilterPipeline() => Inner.GetFilterPipeline();

            public override Collection<IFilter> GetFilters() => Inner.GetFilters();

            public override Collection<HttpParameterDescriptor> GetParameters() => Inner.GetParameters();

            public override ConcurrentDictionary<object, object> Properties => Inner.Properties;

            public override IActionResultConverter ResultConverter => Inner.ResultConverter;

            public override Type ReturnType => Inner.ReturnType;

            public override Collection<HttpMethod> SupportedHttpMethods => Inner.SupportedHttpMethods;
        }
    }
}