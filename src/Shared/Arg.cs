namespace Microsoft
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    internal static class Arg
    {
        [DebuggerStepThrough]
        [ContractArgumentValidator]
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Shared source, but not libraries. May not be used in call links." )]
        internal static void NotNull<T>( T value, string name ) where T : class
        {
            if ( value == null )
                throw new ArgumentNullException( name );
            Contract.EndContractBlock();
        }

        [DebuggerStepThrough]
        [ContractArgumentValidator]
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Shared source, but not libraries. May not be used in call links." )]
        internal static void NotNull<T>( T? value, string name ) where T : struct
        {
            if ( value == null )
                throw new ArgumentNullException( name );
            Contract.EndContractBlock();
        }

        [DebuggerStepThrough]
        [ContractArgumentValidator]
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Shared source, but not libraries. May not be used in call links." )]
        internal static void NotNullOrEmpty( string value, string name )
        {
            if ( string.IsNullOrEmpty( value ) )
                throw new ArgumentNullException( name );
            Contract.EndContractBlock();
        }

        [DebuggerStepThrough]
        [ContractArgumentValidator]
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Shared source, but not libraries. May not be used in call links." )]
        internal static void InRange<T>( T value, T minValue, string name ) where T : IComparable<T>
        {
            if ( value.CompareTo( minValue ) < 0 )
                throw new ArgumentOutOfRangeException( name );
            Contract.EndContractBlock();
        }

        [DebuggerStepThrough]
        [ContractArgumentValidator]
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Shared source, but not libraries. May not be used in call links." )]
        internal static void InRange<T>( T value, T minValue, T maxValue, string name ) where T : IComparable<T>
        {
            if ( value.CompareTo( minValue ) < 0 || value.CompareTo( maxValue ) > 0 )
                throw new ArgumentOutOfRangeException( name );
            Contract.EndContractBlock();
        }
    }
}
