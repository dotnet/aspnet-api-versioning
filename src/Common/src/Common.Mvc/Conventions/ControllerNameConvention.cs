// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if !NETFRAMEWORK
#pragma warning disable IDE0057
#endif

/// <summary>
/// Represents the base implementation for a <see cref="IControllerNameConvention">controller name convention</see>.
/// </summary>
public abstract class ControllerNameConvention : IControllerNameConvention
{
    private static IControllerNameConvention? @default;
    private static IControllerNameConvention? original;
    private static IControllerNameConvention? grouped;

    /// <summary>
    /// Initializes a new instance of the <see cref="ControllerNameConvention"/> class.
    /// </summary>
    protected ControllerNameConvention() { }

    /// <inheritdoc />
    public abstract string NormalizeName( string controllerName );

    /// <inheritdoc />
    public abstract string GroupName( string controllerName );

    /// <summary>
    /// Gets the default controller name convention.
    /// </summary>
    /// <value>The default <see cref="IControllerNameConvention">controller name convention</see>.</value>
    /// <remarks>This convention will strip the <b>Controller</b> suffix as well as any trailing numeric values.</remarks>
    public static IControllerNameConvention Default => @default ??= new DefaultControllerNameConvention();

    /// <summary>
    /// Gets the original controller name convention.
    /// </summary>
    /// <value>The original <see cref="IControllerNameConvention">controller name convention</see>.</value>
    /// <remarks>This convention will apply the original convention which only strips the <b>Controller</b> suffix.</remarks>
    public static IControllerNameConvention Original => original ??= new OriginalControllerNameConvention();

    /// <summary>
    /// Gets the grouped controller name convention.
    /// </summary>
    /// <value>The grouped <see cref="IControllerNameConvention">controller name convention</see>.</value>
    /// <remarks>This convention will apply the original convention which strips the <b>Controller</b> suffix from the
    /// controller name. Any trailing numbers will also be stripped from controller name, but only for the purposes
    /// of grouping.</remarks>
    public static IControllerNameConvention Grouped => grouped ??= new GroupedControllerNameConvention();

    /// <summary>
    /// Trims any trailing numeric characters from the specified name.
    /// </summary>
    /// <param name="name">The name to trim any trailing numbers from.</param>
    /// <returns>The <paramref name="name"/> with any trailing numbers from its suffix.</returns>
    public static string TrimTrailingNumbers( string name )
    {
        if ( string.IsNullOrEmpty( name ) )
        {
            return string.Empty;
        }

        var last = name.Length - 1;

        for ( var i = last; i >= 0; i-- )
        {
            if ( !char.IsNumber( name[i] ) )
            {
                if ( i < last )
                {
                    return name.Substring( 0, i + 1 );
                }

                return name;
            }
        }

        return name;
    }
}