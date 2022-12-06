// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Globalization;
using System.Reflection;
using Xunit.Sdk;
using static System.AttributeTargets;
using static System.Threading.Thread;

/// <summary>
/// Allows a test method to assume that it is running in a specific locale.
/// </summary>
[AttributeUsage( Class | Method, AllowMultiple = false, Inherited = true )]
public sealed class AssumeCultureAttribute : BeforeAfterTestAttribute
{
    private CultureInfo originalCulture;
    private CultureInfo originalUICulture;

    public AssumeCultureAttribute( string name ) => Name = name;

    public string Name { get; }

    public override void Before( MethodInfo methodUnderTest )
    {
        originalCulture = CurrentThread.CurrentCulture;
        originalUICulture = CurrentThread.CurrentUICulture;

        var culture = CultureInfo.CreateSpecificCulture( Name );

        CurrentThread.CurrentCulture = culture;
        CurrentThread.CurrentUICulture = culture;
    }

    public override void After( MethodInfo methodUnderTest )
    {
        CurrentThread.CurrentCulture = originalCulture;
        CurrentThread.CurrentUICulture = originalUICulture;
    }
}