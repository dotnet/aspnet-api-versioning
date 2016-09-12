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
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Contract validator." )]
        internal static void NotNull<T>( T value, string name ) where T : class
        {
            if ( value == null )
            {
                throw new ArgumentNullException( name );
            }
            Contract.EndContractBlock();
        }

        [DebuggerStepThrough]
        [ContractArgumentValidator]
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Contract validator." )]
        internal static void NotNull<T>( T? value, string name ) where T : struct
        {
            if ( value == null )
            {
                throw new ArgumentNullException( name );
            }
            Contract.EndContractBlock();
        }

        [DebuggerStepThrough]
        [ContractArgumentValidator]
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Contract validator." )]
        internal static void NotNullOrEmpty( string value, string name )
        {
            if ( string.IsNullOrEmpty( value ) )
            {
                throw new ArgumentNullException( name );
            }
            Contract.EndContractBlock();
        }

        [DebuggerStepThrough]
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Contract validator." )]
        internal static void InRange<T>( T value, T minValue, string name ) where T : IComparable<T>
        {
            if ( value.CompareTo( minValue ) < 0 )
            {
                throw new ArgumentOutOfRangeException( name );
            }
            Contract.EndContractBlock();
        }

        [DebuggerStepThrough]
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Contract validator." )]
        internal static void InRange<T>( T value, T minValue, T maxValue, string name ) where T : IComparable<T>
        {
            if ( value.CompareTo( minValue ) < 0 || value.CompareTo( maxValue ) > 0 )
            {
                throw new ArgumentOutOfRangeException( name );
            }
            Contract.EndContractBlock();
        }

        [DebuggerStepThrough]
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Contract validator." )]
        internal static void LessThan<T>( T param, T value, string paramName ) where T : struct, IComparable<T>
        {
            if ( param.CompareTo( value ) >= 0 )
            {
                throw new ArgumentOutOfRangeException( paramName );
            }
        }

        [DebuggerStepThrough]
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Contract validator." )]
        internal static void LessThanOrEqualTo<T>( T param, T value, string paramName ) where T : struct, IComparable<T>
        {
            if ( param.CompareTo( value ) > 0 )
            {
                throw new ArgumentOutOfRangeException( paramName );
            }
        }

        [DebuggerStepThrough]
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Contract validator." )]
        internal static void GreaterThan<T>( T param, T value, string paramName ) where T : struct, IComparable<T>
        {
            if ( param.CompareTo( value ) <= 0 )
            {
                throw new ArgumentOutOfRangeException( paramName );
            }
        }

        [DebuggerStepThrough]
        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Contract validator." )]
        internal static void GreaterThanOrEqualTo<T>( T param, T value, string paramName ) where T : struct, IComparable<T>
        {
            if ( param.CompareTo( value ) < 0 )
            {
                throw new ArgumentOutOfRangeException( paramName );
            }
        }
    }
}