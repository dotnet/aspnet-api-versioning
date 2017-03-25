namespace Microsoft.Web.Http.Description.Models
{
    using System.Collections.Generic;

    public class GenericMutableObject<T> : List<T>
    {
        public string Foo { get; set; }

        public string Bar { get; set; }
    }
}