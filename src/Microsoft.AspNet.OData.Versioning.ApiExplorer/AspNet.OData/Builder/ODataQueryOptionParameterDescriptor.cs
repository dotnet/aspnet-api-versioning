namespace Microsoft.AspNet.OData.Builder
{
    using System;
    using System.Web.Http.Controllers;

    sealed class ODataQueryOptionParameterDescriptor : HttpParameterDescriptor
    {
        internal ODataQueryOptionParameterDescriptor( string name, Type type, object defaultValue )
        {
            ParameterName = name;
            ParameterType = type;
            DefaultValue = defaultValue;
        }

        public override string ParameterName { get; }

        public override Type ParameterType { get; }

        public override object DefaultValue { get; }

        public override string Prefix => "$";

        public override bool IsOptional => true;
    }
}