namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using System;
    using System.Collections.Generic;
    using static System.Linq.Enumerable;

    class TestActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
    {
        readonly Lazy<ActionDescriptorCollection> collection = new Lazy<ActionDescriptorCollection>( CreateActionDescriptors );

        public ActionDescriptorCollection ActionDescriptors => collection.Value;

        static ActionDescriptorCollection CreateActionDescriptors()
        {
            var actions = new List<ActionDescriptor>();

            AddOrderActionDescriptors( actions );
            AddPeopleActionDescriptors( actions );

            return new ActionDescriptorCollection( actions.ToArray(), 0 );
        }

        static void AddOrderActionDescriptors( ICollection<ActionDescriptor> actions )
        {
            // api version 0.9 and 1.0
            actions.Add(
                NewActionDescriptor(
                    "GET-orders/{id}",
                    declared: new[] { new ApiVersion( 0, 9 ), new ApiVersion( 1, 0 ) },
                    supported: new[] { new ApiVersion( 1, 0 ) },
                    deprecated: new[] { new ApiVersion( 0, 9 ) } ) );

            actions.Add(
                NewActionDescriptor(
                    "POST-orders",
                    declared: new[] { new ApiVersion( 1, 0 ) },
                    supported: new[] { new ApiVersion( 1, 0 ) } ) );

            // api version 2.0
            actions.Add(
                NewActionDescriptor(
                    "GET-orders",
                    declared: new[] { new ApiVersion( 2, 0 ) },
                    supported: new[] { new ApiVersion( 2, 0 ) } ) );

            actions.Add(
                NewActionDescriptor(
                    "GET-orders/{id}",
                    declared: new[] { new ApiVersion( 2, 0 ) },
                    supported: new[] { new ApiVersion( 2, 0 ) } ) );

            actions.Add(
                NewActionDescriptor(
                    "POST-orders",
                    declared: new[] { new ApiVersion( 2, 0 ) },
                    supported: new[] { new ApiVersion( 2, 0 ) } ) );

            // api version 3.0
            actions.Add(
                NewActionDescriptor(
                    "GET-orders",
                    declared: new[] { new ApiVersion( 3, 0 ) },
                    supported: new[] { new ApiVersion( 3, 0 ) },
                    advertised: new[] { new ApiVersion( 4, 0 ) } ) );

            actions.Add(
                NewActionDescriptor(
                    "GET-orders/{id}",
                    declared: new[] { new ApiVersion( 3, 0 ) },
                    supported: new[] { new ApiVersion( 3, 0 ) },
                    advertised: new[] { new ApiVersion( 4, 0 ) } ) );

            actions.Add(
                NewActionDescriptor(
                    "POST-orders",
                    declared: new[] { new ApiVersion( 3, 0 ) },
                    supported: new[] { new ApiVersion( 3, 0 ) },
                    advertised: new[] { new ApiVersion( 4, 0 ) } ) );

            actions.Add(
                NewActionDescriptor(
                    "DELETE-orders/{id}",
                    declared: new[] { new ApiVersion( 3, 0 ) },
                    supported: new[] { new ApiVersion( 3, 0 ) },
                    advertised: new[] { new ApiVersion( 4, 0 ) } ) );
        }

        static void AddPeopleActionDescriptors( ICollection<ActionDescriptor> actions )
        {
            // api version 0.9 and 1.0
            actions.Add(
                NewActionDescriptor(
                    "GET-people/{id}",
                    declared: new[] { new ApiVersion( 0, 9 ), new ApiVersion( 1, 0 ) },
                    supported: new[] { new ApiVersion( 1, 0 ) },
                    deprecated: new[] { new ApiVersion( 0, 9 ) } ) );

            actions.Add(
                NewActionDescriptor(
                    "POST-people",
                    declared: new[] { new ApiVersion( 1, 0 ) },
                    supported: new[] { new ApiVersion( 1, 0 ) } ) );

            // api version 2.0
            actions.Add(
                NewActionDescriptor(
                    "GET-people",
                    declared: new[] { new ApiVersion( 2, 0 ) },
                    supported: new[] { new ApiVersion( 2, 0 ) } ) );

            actions.Add(
                NewActionDescriptor(
                    "GET-people/{id}",
                    declared: new[] { new ApiVersion( 2, 0 ) },
                    supported: new[] { new ApiVersion( 2, 0 ) } ) );

            actions.Add(
                NewActionDescriptor(
                    "POST-people",
                    declared: new[] { new ApiVersion( 2, 0 ) },
                    supported: new[] { new ApiVersion( 2, 0 ) } ) );

            // api version 3.0
            actions.Add(
                NewActionDescriptor(
                    "GET-people",
                    declared: new[] { new ApiVersion( 3, 0 ) },
                    supported: new[] { new ApiVersion( 3, 0 ) },
                    advertised: new[] { new ApiVersion( 4, 0 ) } ) );

            actions.Add(
                NewActionDescriptor(
                    "GET-people/{id}",
                    declared: new[] { new ApiVersion( 3, 0 ) },
                    supported: new[] { new ApiVersion( 3, 0 ) },
                    advertised: new[] { new ApiVersion( 4, 0 ) } ) );

            actions.Add(
                NewActionDescriptor(
                    "POST-people",
                    declared: new[] { new ApiVersion( 3, 0 ) },
                    supported: new[] { new ApiVersion( 3, 0 ) },
                    advertised: new[] { new ApiVersion( 4, 0 ) } ) );
        }

        static ActionDescriptor NewActionDescriptor(
            string displayName,
            IEnumerable<ApiVersion> declared,
            IEnumerable<ApiVersion> supported,
            IEnumerable<ApiVersion> deprecated = null,
            IEnumerable<ApiVersion> advertised = null,
            IEnumerable<ApiVersion> advertisedDeprecated = null )
        {
            deprecated = deprecated ?? Empty<ApiVersion>();
            advertised = advertised ?? Empty<ApiVersion>();
            advertisedDeprecated = advertisedDeprecated ?? Empty<ApiVersion>();

            var action = new ActionDescriptor() { DisplayName = displayName };
            var model = new ApiVersionModel( declared, supported, deprecated, advertised, advertisedDeprecated );

            action.SetProperty( model );

            return action;
        }
    }
}