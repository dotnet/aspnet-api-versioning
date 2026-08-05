// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

/// <summary>
/// Represents what a policy is keyed by.
/// </summary>
/// <remarks>
/// A policy is keyed by an API name, an API version, or both. A policy resolves by name and version
/// first, then by name, and finally by version, so a policy that leaves one of them unstated is reached
/// by every API that agrees with the part it does state.
/// </remarks>
internal sealed class PolicyKey
{
    private const string Name = "name";
    private const string ApiVersion = "apiVersion";
    private const string MajorVersion = "majorVersion";
    private const string MinorVersion = "minorVersion";
    private const string Version = "version";
    private const string Status = "status";
    private const string Year = "year";
    private const string Month = "month";
    private const string Day = "day";

    private PolicyKey( string? name, string? version )
    {
        ApiName = name;
        ApiVersionForm = version;
    }

    /// <summary>
    /// Gets the name the policy is keyed by, if any.
    /// </summary>
    public string? ApiName { get; }

    /// <summary>
    /// Gets the form of the API version the policy is keyed by, if any.
    /// </summary>
    public string? ApiVersionForm { get; }

    /// <summary>
    /// Gets a value indicating whether no API reaches the policy.
    /// </summary>
    /// <remarks>A version reaches every API of that version whatever it is named, and a name reaches
    /// every version of that API. Stating neither reaches nothing at all rather than everything.</remarks>
    public bool Unreachable => ApiName is null && ApiVersionForm is null;

    /// <summary>
    /// Determines whether both policies can be reached by the same API.
    /// </summary>
    /// <param name="other">The policy key to compare against.</param>
    /// <returns>True if some API reaches both policies; otherwise, false.</returns>
    /// <remarks>A part that is unstated is agreed with by every API, so two keys are reached together
    /// unless a part they both state disagrees.</remarks>
    public bool Intersects( PolicyKey other ) =>
        Agrees( ApiName, other.ApiName ) && Agrees( ApiVersionForm, other.ApiVersionForm );

    /// <summary>
    /// Attempts to resolve what a policy is keyed by.
    /// </summary>
    /// <param name="context">The context the policy was declared in.</param>
    /// <param name="invocation">The expression declaring the policy.</param>
    /// <param name="method">The method the expression resolves to.</param>
    /// <param name="key">The resolved key, if any.</param>
    /// <returns>True if the key was resolved; otherwise, false.</returns>
    /// <remarks>A key written in a way that cannot be read leaves nothing to compare, which is not the
    /// same as a key that states nothing.</remarks>
    public static bool TryResolve(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation,
        IMethodSymbol method,
        out PolicyKey key )
    {
        key = default!;

        var arguments = invocation.ArgumentList.Arguments;
        var name = default( string );
        var version = default( string );
        var major = default( int? );
        var minor = default( int? );
        var number = default( double? );
        var year = default( int? );
        var month = default( int? );
        var day = default( int? );
        var status = default( string );

        for ( var i = 0; i < arguments.Count; i++ )
        {
            var argument = arguments[i];
            var parameter = ResolveParameter(
                method.Parameters,
                argument.NameColon?.Name.Identifier.ValueText,
                i );

            if ( parameter is null )
            {
                return false;
            }

            if ( parameter.Name == ApiVersion )
            {
                // the version can be an expression of its own, which reduces the same way it does elsewhere
                if ( OptionValue.Resolve( context.SemanticModel, argument.Expression, context.CancellationToken )
                     is not { } form )
                {
                    return false;
                }

                version = form;
                continue;
            }

            var constant = context.SemanticModel.GetConstantValue( argument.Expression, context.CancellationToken );

            if ( !constant.HasValue )
            {
                return false;
            }

            switch ( parameter.Name )
            {
                case Name when constant.Value is null:
                    break;
                case Name when constant.Value is string text:
                    name = string.IsNullOrEmpty( text ) ? default : text;
                    break;
                case MajorVersion when constant.Value is int value:
                    major = value;
                    break;
                case MinorVersion when constant.Value is null:
                    break;
                case MinorVersion when constant.Value is int value:
                    minor = value;
                    break;
                case Version when constant.Value is double value:
                    number = value;
                    break;
                case Version when constant.Value is int value:
                    number = value;
                    break;
                case Year when constant.Value is int value:
                    year = value;
                    break;
                case Month when constant.Value is int value:
                    month = value;
                    break;
                case Day when constant.Value is int value:
                    day = value;
                    break;
                case Status when constant.Value is null:
                    break;
                case Status when constant.Value is string text:
                    status = text;
                    break;

                // a group version stated as a date is not a form this can carry
                default:
                    return false;
            }
        }

        if ( version is null )
        {
            if ( major is { } value )
            {
                version = OptionValue.ApiVersion( value, minor ?? 0, status );
            }
            else if ( number is { } stated )
            {
                if ( OptionValue.ApiVersion( stated, status ) is not { } form )
                {
                    return false;
                }

                version = form;
            }
            else if ( year is { } y && month is { } m && day is { } d )
            {
                version = OptionValue.GroupVersion( y, m, d, status );
            }
        }

        key = new( name, version );
        return true;
    }

    private static bool Agrees( string? left, string? right ) =>
        left is null || right is null || left == right;

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