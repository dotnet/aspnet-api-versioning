namespace Microsoft.AspNetCore.Mvc
{
    using System;

    public abstract partial class AcceptanceTest
    {
        public bool UsingEndpointRouting => fixture.EnableEndpointRouting;
    }
}