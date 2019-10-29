namespace Microsoft.AspNet.OData.Builder
{
    using System;
    using System.Web.Http.Controllers;

    sealed class ODataParameterDescriptor : HttpParameterDescriptor
    {
        internal ODataParameterDescriptor( string name, Type type, bool optional = false, object? defaultValue = default )
        {
            ParameterName = name;
            ParameterType = type;
            IsOptional = optional;
            DefaultValue = defaultValue;
        }

        public override string ParameterName { get; }

        public override Type ParameterType { get; }

        public override object? DefaultValue { get; }

        public override string Prefix { get; } = string.Empty;

        public override bool IsOptional { get; }
    }
}