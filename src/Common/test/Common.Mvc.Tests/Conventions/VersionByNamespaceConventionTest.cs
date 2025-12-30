// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using System.Reflection;

public partial class VersionByNamespaceConventionTest
{
    public static TheoryData<string, string> NamespaceAsVersionData => new()
    {
        { "v1", "1.0" },
        { "v1RC", "1.0-RC" },
        { "v20180401", "2018-04-01" },
        { "v20180401_Beta", "2018-04-01-Beta" },
        { "v20180401Beta", "2018-04-01-Beta" },
        { "Contoso.Api.v1.Controllers", "1.0" },
        { "Contoso.Api.v10_0.Controllers", "10.0" },
        { "Contoso.Api.v1_1.Controllers", "1.1" },
        { "Contoso.Api.v0_9_Beta.Controllers", "0.9-Beta" },
        { "Contoso.Api.v20180401.Controllers", "2018-04-01" },
        { "Contoso.Api.v2018_04_01.Controllers", "2018-04-01" },
        { "Contoso.Api.v20180401_Beta.Controllers", "2018-04-01-Beta" },
        { "Contoso.Api.v2018_04_01_Beta.Controllers", "2018-04-01-Beta" },
        { "Contoso.Api.v2018_04_01_1_0_Beta.Controllers", "2018-04-01.1.0-Beta" },
        { "MyRestaurant.Vegetarian.Food.v1_1.Controllers", "1.1" },
        { "VersioningSample.V5.Controllers", "5.0" },
    };

    private sealed class TestType : TypeDelegator
    {
        internal TestType( string @namespace ) => Namespace = @namespace;

        public override string Namespace { get; }
    }
}