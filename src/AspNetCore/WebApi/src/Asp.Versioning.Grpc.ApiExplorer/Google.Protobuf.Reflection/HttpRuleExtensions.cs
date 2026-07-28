// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Google.Protobuf.Reflection;

using Google.Api;
using Microsoft.AspNetCore.Http;

internal static class HttpRuleExtensions
{
    extension( HttpRule http )
    {
        public bool TryResolvePattern(
            [NotNullWhen( true )] out string? pattern,
            [NotNullWhen( true )] out string? method )
        {
            switch ( http.PatternCase )
            {
                case HttpRule.PatternOneofCase.Get:
                    pattern = http.Get;
                    method = HttpMethods.Get;
                    return true;
                case HttpRule.PatternOneofCase.Put:
                    pattern = http.Put;
                    method = HttpMethods.Put;
                    return true;
                case HttpRule.PatternOneofCase.Post:
                    pattern = http.Post;
                    method = HttpMethods.Post;
                    return true;
                case HttpRule.PatternOneofCase.Delete:
                    pattern = http.Delete;
                    method = HttpMethods.Delete;
                    return true;
                case HttpRule.PatternOneofCase.Patch:
                    pattern = http.Patch;
                    method = HttpMethods.Patch;
                    return true;
                case HttpRule.PatternOneofCase.Custom:
                    pattern = http.Custom.Path;
                    method = http.Custom.Kind;
                    return true;
                default:
                    pattern = default;
                    method = default;
                    return false;
            }
        }
    }
}