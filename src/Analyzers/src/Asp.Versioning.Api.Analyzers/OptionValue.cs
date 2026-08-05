// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

using System.Globalization;
using System.Text;

/// <summary>
/// Represents the value assigned to an option, reduced to a form that can be compared.
/// </summary>
/// <remarks>
/// The same value can be written more than one way, so values are compared by what they mean rather than
/// by how they are spelled. A value that cannot be decided as it is written has no form at all, which is
/// never equal to anything.
/// </remarks>
internal static class OptionValue
{
    private const string Default = nameof( Default );
    private const string Empty = nameof( Empty );
    private const string ApiVersionType = "Asp.Versioning.ApiVersion";

    /// <summary>
    /// Gets the form of the API version that options default to.
    /// </summary>
    public static string DefaultApiVersion { get; } = Version( 1, 0, default );

    /// <summary>
    /// Returns the form of a value known at compile time.
    /// </summary>
    /// <param name="value">The value to reduce.</param>
    /// <returns>The form of the <paramref name="value"/>.</returns>
    public static string Constant( object? value ) =>
        value is null
        ? "c:"
        : "c:" + value.GetType().Name + ":" + Convert.ToString( value, CultureInfo.InvariantCulture );

    /// <summary>
    /// Returns the form of an API version stated as its parts.
    /// </summary>
    /// <param name="major">The major version.</param>
    /// <param name="minor">The minor version.</param>
    /// <param name="status">The version status, if any.</param>
    /// <returns>The form of the API version.</returns>
    public static string ApiVersion( int major, int minor, string? status ) => Version( major, minor, status );

    /// <summary>
    /// Returns the form of an API version stated as a number.
    /// </summary>
    /// <param name="version">The version number.</param>
    /// <param name="status">The version status, if any.</param>
    /// <returns>The form of the API version, or <c>null</c> if the number is not one.</returns>
    public static string? ApiVersion( double version, string? status ) =>
        TrySplit( version, out var major, out var minor ) ? Version( major, minor, status ) : default;

    /// <summary>
    /// Returns the form of an API version stated as a date.
    /// </summary>
    /// <param name="year">The year of the group version.</param>
    /// <param name="month">The month of the group version.</param>
    /// <param name="day">The day of the group version.</param>
    /// <param name="status">The version status, if any.</param>
    /// <returns>The form of the API version.</returns>
    public static string GroupVersion( int year, int month, int day, string? status ) =>
        "g:" + year.ToString( "D4", CultureInfo.InvariantCulture ) + "-" +
        month.ToString( "D2", CultureInfo.InvariantCulture ) + "-" +
        day.ToString( "D2", CultureInfo.InvariantCulture ) + ":" + status;

    /// <summary>
    /// Returns the form of a value reached through a static member.
    /// </summary>
    /// <param name="name">The fully qualified name of the member.</param>
    /// <returns>The form of the member.</returns>
    public static string Member( string name ) => "s:" + name;

    /// <summary>
    /// Reduces the expression assigned to an option to a form that can be compared.
    /// </summary>
    /// <param name="model">The semantic model the expression belongs to.</param>
    /// <param name="expression">The assigned expression.</param>
    /// <param name="cancellationToken">The token that can be used to cancel the operation.</param>
    /// <returns>The form of the expression, or <c>null</c> if it cannot be decided.</returns>
    public static string? Resolve(
        SemanticModel model,
        ExpressionSyntax expression,
        CancellationToken cancellationToken )
    {
        var constant = model.GetConstantValue( expression, cancellationToken );

        if ( constant.HasValue )
        {
            return Constant( constant.Value );
        }

        var symbol = model.GetSymbolInfo( expression, cancellationToken ).Symbol;

        switch ( symbol )
        {
            // string.Empty is a static field rather than a constant, but it is the same value
            case IFieldSymbol { Name: Empty, ContainingType.SpecialType: SpecialType.System_String }:
                return Constant( string.Empty );

            // the version the options already default to, spelled the way the library spells it
            case IPropertySymbol { IsStatic: true, Name: Default } version
                when version.ContainingType?.ToDisplayString() == ApiVersionType:
                return DefaultApiVersion;

            case IPropertySymbol { IsStatic: true } or IFieldSymbol { IsStatic: true }:
                return Member( symbol.ToDisplayString() );
        }

        if ( expression is not BaseObjectCreationExpressionSyntax creation ||
             symbol is not IMethodSymbol constructor ||
             constructor.ContainingType?.ToDisplayString() is not { } type )
        {
            return default;
        }

        return type == ApiVersionType
             ? ResolveApiVersion( model, constructor, creation.ArgumentList, cancellationToken )
             : ResolveCreation( model, type, creation.ArgumentList, cancellationToken );
    }

    private static string Version( int major, int minor, string? status ) =>
        "v:" + major.ToString( CultureInfo.InvariantCulture ) + "." +
        minor.ToString( CultureInfo.InvariantCulture ) + ":" + status;

    private static string? ResolveApiVersion(
        SemanticModel model,
        IMethodSymbol constructor,
        ArgumentListSyntax? list,
        CancellationToken cancellationToken )
    {
        var arguments = list is null ? default : list.Arguments;
        var major = default( int? );
        var minor = default( int? );
        var status = default( string );

        for ( var i = 0; i < arguments.Count; i++ )
        {
            var argument = arguments[i];
            var parameter = ResolveParameter(
                constructor.Parameters,
                argument.NameColon?.Name.Identifier.ValueText,
                i );

            if ( parameter is null )
            {
                return default;
            }

            var constant = model.GetConstantValue( argument.Expression, cancellationToken );

            if ( !constant.HasValue )
            {
                return default;
            }

            switch ( parameter.Name )
            {
                case "version" when constant.Value is double number:
                    if ( !TrySplit( number, out var whole, out var fraction ) )
                    {
                        return default;
                    }

                    major = whole;
                    minor = fraction;
                    break;
                case "version" when constant.Value is int number:
                    major = number;
                    minor = 0;
                    break;
                case "majorVersion" when constant.Value is int number:
                    major = number;
                    break;
                case "minorVersion" when constant.Value is null:
                    break;
                case "minorVersion" when constant.Value is int number:
                    minor = number;
                    break;
                case "status" when constant.Value is null:
                    break;
                case "status" when constant.Value is string text:
                    status = text;
                    break;

                // a group version is a date rather than a number, which this form cannot carry
                default:
                    return default;
            }
        }

        // a minor version that is not stated is implied to be zero
        return major is { } value ? Version( value, minor ?? 0, status ) : default;
    }

    /// <remarks>The version is split the same way the constructor that takes a number splits it, so that
    /// a version written as a number reduces to the same form as one written as its parts.</remarks>
    private static bool TrySplit( double version, out int major, out int minor )
    {
        major = 0;
        minor = 0;

        if ( version < 0d || double.IsNaN( version ) || double.IsInfinity( version ) )
        {
            return false;
        }

        var number = new decimal( version );
        var scale = ( decimal.GetBits( number )[3] >> 16 ) & 31;
        var whole = decimal.Truncate( number );

        if ( whole > int.MaxValue )
        {
            return false;
        }

        major = (int) whole;
        minor = (int) ( ( number - whole ) * new decimal( Math.Pow( 10, scale ) ) );
        return true;
    }

    /// <remarks>An argument stated by name can appear in any order, which the form cannot represent, so a
    /// value written that way is left undecided rather than reduced to the wrong thing.</remarks>
    private static string? ResolveCreation(
        SemanticModel model,
        string type,
        ArgumentListSyntax? list,
        CancellationToken cancellationToken )
    {
        var arguments = list is null ? default : list.Arguments;
        var form = new StringBuilder( "n:" ).Append( type );

        for ( var i = 0; i < arguments.Count; i++ )
        {
            var argument = arguments[i];

            if ( argument.NameColon is not null )
            {
                return default;
            }

            var constant = model.GetConstantValue( argument.Expression, cancellationToken );

            if ( !constant.HasValue )
            {
                return default;
            }

            form.Append( ',' ).Append( Constant( constant.Value ) );
        }

        return form.ToString();
    }

    private static IParameterSymbol? ResolveParameter(
        ImmutableArray<IParameterSymbol> parameters,
        string? name,
        int index )
    {
        if ( name is null )
        {
            return index < parameters.Length ? parameters[index] : default;
        }

        foreach ( var parameter in parameters )
        {
            if ( parameter.Name == name )
            {
                return parameter;
            }
        }

        return default;
    }
}