namespace Microsoft.AspNetCore.Mvc
{
    using System;

    static partial class TypeExtensions
    {
        internal static bool IsPrimitive( this Type type )
        {
            switch ( Type.GetTypeCode( type ) )
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Char:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
            }

            return type == typeof( IntPtr ) || type == typeof( UIntPtr );
        }
    }
}