// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Text;

internal static class Format
{
    internal static readonly CompositeFormat UnableToFindServices = CompositeFormat.Parse( SR.UnableToFindServices );
}