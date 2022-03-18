// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

[AttributeUsage( AttributeTargets.Property )]
public sealed class AllowedRolesAttribute : Attribute
{
    public AllowedRolesAttribute( params string[] allowedRoles )
    {
        AllowedRoles = allowedRoles.ToList();
    }

    public IReadOnlyList<string> AllowedRoles { get; }
}