// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using System.Reflection;

public partial class VersionByNamespaceConventionTest
{
    public static IEnumerable<object[]> NamespaceAsVersionData
    {
        get
        {
            yield return new object[] { "v1", "1.0" };
            yield return new object[] { "v1RC", "1.0-RC" };
            yield return new object[] { "v20180401", "2018-04-01" };
            yield return new object[] { "v20180401_Beta", "2018-04-01-Beta" };
            yield return new object[] { "v20180401Beta", "2018-04-01-Beta" };
            yield return new object[] { "Contoso.Api.v1.Controllers", "1.0" };
            yield return new object[] { "Contoso.Api.v10_0.Controllers", "10.0" };
            yield return new object[] { "Contoso.Api.v1_1.Controllers", "1.1" };
            yield return new object[] { "Contoso.Api.v0_9_Beta.Controllers", "0.9-Beta" };
            yield return new object[] { "Contoso.Api.v20180401.Controllers", "2018-04-01" };
            yield return new object[] { "Contoso.Api.v2018_04_01.Controllers", "2018-04-01" };
            yield return new object[] { "Contoso.Api.v20180401_Beta.Controllers", "2018-04-01-Beta" };
            yield return new object[] { "Contoso.Api.v2018_04_01_Beta.Controllers", "2018-04-01-Beta" };
            yield return new object[] { "Contoso.Api.v2018_04_01_1_0_Beta.Controllers", "2018-04-01.1.0-Beta" };
            yield return new object[] { "MyRestaurant.Vegetarian.Food.v1_1.Controllers", "1.1" };
            yield return new object[] { "VersioningSample.V5.Controllers", "5.0" };
        }
    }

    private sealed class TestType : TypeDelegator
    {
        internal TestType( string @namespace ) => Namespace = @namespace;
        public override string Namespace { get; }
    }
}