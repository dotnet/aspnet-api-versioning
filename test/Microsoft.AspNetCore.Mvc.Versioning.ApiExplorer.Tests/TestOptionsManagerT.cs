namespace Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer
{
    using Microsoft.Extensions.Options;
    using static System.Linq.Enumerable;

    sealed class TestOptionsManager<T> : OptionsManager<T> where T : class, new()
    {
        internal TestOptionsManager() : base( Empty<IConfigureOptions<T>>() ) { }
    }
}