namespace Microsoft.Web.Http.Description
{
    using System;
    using System.Web.Http.Controllers;

    sealed class ApiVersionParameterDescriptor : HttpParameterDescriptor
    {
        readonly string parameterName;
        readonly object defaultValue;
        readonly bool optional;

        internal ApiVersionParameterDescriptor( string parameterName, object defaultValue, bool optional = false, bool fromPath = false )
        {
            this.parameterName = parameterName;
            this.defaultValue = defaultValue;
            this.optional = optional;
            FromPath = fromPath;
        }

        public override string ParameterName => parameterName;

        public override Type ParameterType => typeof( string );

        public override object DefaultValue => defaultValue;

        public override bool IsOptional => optional;

        internal bool FromPath { get; }
    }
}