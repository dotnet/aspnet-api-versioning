namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents a specification that matches API controllers by the presence of API behaviors.
    /// </summary>
    [CLSCompliant( false )]
    public sealed class ApiBehaviorSpecification : IApiControllerSpecification
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiBehaviorSpecification"/> class.
        /// </summary>
        public ApiBehaviorSpecification()
        {
            const string ApiBehaviorApplicationModelProviderTypeName = "Microsoft.AspNetCore.Mvc.ApplicationModels.ApiBehaviorApplicationModelProvider, Microsoft.AspNetCore.Mvc.Core";
            var type = Type.GetType( ApiBehaviorApplicationModelProviderTypeName, throwOnError: true )!;
            var method = type.GetRuntimeMethods().Single( m => m.Name == "IsApiController" );

            IsApiController = (Func<ControllerModel, bool>) method.CreateDelegate( typeof( Func<ControllerModel, bool> ) );
        }

        Func<ControllerModel, bool> IsApiController { get; }

        /// <inheritdoc />
        public bool IsSatisfiedBy( ControllerModel controller ) => IsApiController( controller );
    }
}