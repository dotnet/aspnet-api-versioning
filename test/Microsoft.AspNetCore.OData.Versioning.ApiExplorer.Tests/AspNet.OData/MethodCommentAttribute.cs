using System;

namespace Microsoft.AspNet.OData
{
    [AttributeUsage( AttributeTargets.Method )]
    public class MethodCommentAttribute : Attribute
    {
        public MethodCommentAttribute(string comment)
        {
            Comment = comment;
        }
        public string Comment { get; }
    }
}