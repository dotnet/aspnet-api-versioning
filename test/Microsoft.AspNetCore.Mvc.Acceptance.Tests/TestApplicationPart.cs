namespace Microsoft.AspNetCore.Mvc
{
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    sealed class TestApplicationPart : ApplicationPart, IApplicationPartTypeProvider
    {
        public TestApplicationPart() => Types = Enumerable.Empty<TypeInfo>();

        public TestApplicationPart( params TypeInfo[] types ) => Types = types;

        public TestApplicationPart( IEnumerable<TypeInfo> types ) => Types = types;

        public TestApplicationPart( IEnumerable<Type> types ) : this( types.Select( t => t.GetTypeInfo() ) ) { }

        public TestApplicationPart( params Type[] types ) : this( types.Select( t => t.GetTypeInfo() ) ) { }

        public override string Name => "Test Part";

        public IEnumerable<TypeInfo> Types { get; }
    }
}