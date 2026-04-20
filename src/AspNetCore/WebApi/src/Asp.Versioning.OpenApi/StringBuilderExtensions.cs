// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Transformers;

using System.Text;

internal static class StringBuilderExtensions
{
    extension( StringBuilder sb )
    {
        public StringBuilder AppendWith<T>( Func<StringBuilder, T, StringBuilder> append, T arg ) => append( sb, arg );
    }
}