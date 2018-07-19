namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.Extensions.Options;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class ApiVersionCollatorTest
    {
        [Theory]
        [MemberData( nameof( ActionDescriptorProviderContexts ) )]
        public void on_providers_executed_should_aggregate_api_version_models_by_controller( ActionDescriptorProviderContext context )
        {
            // arrange
            var options = Options.Create( new ApiVersioningOptions() { ReportApiVersions = true } );
            var collator = new ApiVersionCollator( options );
            var expected = new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) };

            // act
            collator.OnProvidersExecuted( context );

            // assert
            var actions = context.Results.Where( a => a.GetProperty<ApiVersionModel>() != null );

            actions.All( a => a.GetProperty<ApiVersionModel>().SupportedApiVersions.SequenceEqual( expected ) ).Should().BeTrue();
        }

        public static IEnumerable<object[]> ActionDescriptorProviderContexts
        {
            get
            {
                yield return new object[] { ActionsWithRouteValues };
                yield return new object[] { ActionsByControllerName };
            }
        }

        private static ActionDescriptorProviderContext ActionsWithRouteValues =>
            new ActionDescriptorProviderContext()
            {
                Results =
                {
                    new ActionDescriptor()
                    {
                        RouteValues = new Dictionary<string, string>()
                        {
                            ["controller"] = "Values",
                            ["action"] = "Get",
                        },
                        Properties = new Dictionary<object, object>()
                        {
                            [typeof(ApiVersionModel)] = new ApiVersionModel( new ApiVersion( 1, 0 ) ),
                        },
                    },
                    new ActionDescriptor()
                    {
                        RouteValues = new Dictionary<string, string>()
                        {
                            ["page"] = "/Some/Page",
                        },
                    },
                    new ActionDescriptor()
                    {
                        RouteValues = new Dictionary<string, string>()
                        {
                            ["controller"] = "Values",
                            ["action"] = "Get",
                        },
                        Properties = new Dictionary<object, object>()
                        {
                            [typeof(ApiVersionModel)] = new ApiVersionModel( new ApiVersion( 2, 0 ) ),
                        },
                    },
                    new ActionDescriptor()
                    {
                        RouteValues = new Dictionary<string, string>()
                        {
                            ["controller"] = "Values",
                            ["action"] = "Get",
                        },
                        Properties = new Dictionary<object, object>()
                        {
                            [typeof(ApiVersionModel)] = new ApiVersionModel( new ApiVersion( 3, 0 ) ),
                        },
                    },
                },
            };

        private static ActionDescriptorProviderContext ActionsByControllerName =>
            new ActionDescriptorProviderContext()
            {
                Results =
                {
                    new ControllerActionDescriptor()
                    {
                        ControllerName = "Values",
                        RouteValues = new Dictionary<string, string>()
                        {
                            ["action"] = "Get",
                        },
                        Properties = new Dictionary<object, object>()
                        {
                            [typeof(ApiVersionModel)] = new ApiVersionModel( new ApiVersion( 1, 0 ) ),
                        },
                    },
                    new ActionDescriptor()
                    {
                        RouteValues = new Dictionary<string, string>()
                        {
                            ["page"] = "/Some/Page",
                        },
                    },
                    new ControllerActionDescriptor()
                    {
                        ControllerName = "Values2",
                        RouteValues = new Dictionary<string, string>()
                        {
                            ["action"] = "Get",
                        },
                        Properties = new Dictionary<object, object>()
                        {
                            [typeof(ApiVersionModel)] = new ApiVersionModel( new ApiVersion( 2, 0 ) ),
                        },
                    },
                    new ControllerActionDescriptor()
                    {
                        ControllerName = "Values3",
                        RouteValues = new Dictionary<string, string>()
                        {
                            ["action"] = "Get",
                        },
                        Properties = new Dictionary<object, object>()
                        {
                            [typeof(ApiVersionModel)] = new ApiVersionModel( new ApiVersion( 3, 0 ) ),
                        },
                    },
                },
            };
    }
}