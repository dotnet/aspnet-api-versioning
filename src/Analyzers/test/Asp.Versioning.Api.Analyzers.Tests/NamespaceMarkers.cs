// Copyright (c) .NET Foundation and contributors. All rights reserved.

// NamespaceParser reads the namespace of a type rather than a string, so comparing the port against it
// requires real types declared in the namespaces under test. Naming them the way a version is spelled
// is the entire point, and they only exist within this test assembly
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable SA1300 // Element should begin with an uppercase letter
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1403 // File may only contain a single namespace
#pragma warning disable SA1649 // File name should match first type name

namespace Asp.Versioning.Analyzers.Versioned.V1 { public class VersionedV1Marker { } }

namespace Asp.Versioning.Analyzers.Versioned.v1_1 { public class Versionedv1_1Marker { } }

namespace Asp.Versioning.Analyzers.Versioned.V2_0_Beta { public class VersionedV2_0_BetaMarker { } }

namespace Asp.Versioning.Analyzers.Versioned._20180401 { public class Versioned20180401Marker { } }

namespace Asp.Versioning.Analyzers.Versioned._2018_04_01 { public class Versioned2018_04_01Marker { } }

namespace Asp.Versioning.Analyzers.Versioned._2018_04_01_1_0_Beta { public class Versioned2018_04_01_1_0_BetaMarker { } }

namespace Asp.Versioning.Analyzers.Versioned.v2018_04_01_1_1_Beta { public class Versionedv2018_04_01_1_1_BetaMarker { } }

namespace Asp.Versioning.Analyzers.Unversioned.Controllers { public class UnversionedControllersMarker { } }

namespace Asp.Versioning.Analyzers.Unversioned.Version1 { public class UnversionedVersion1Marker { } }

namespace Asp.Versioning.Analyzers.Unversioned.vNext { public class UnversionedvNextMarker { } }

namespace Asp.Versioning.Analyzers.Unversioned.v20181301 { public class Unversionedv20181301Marker { } }