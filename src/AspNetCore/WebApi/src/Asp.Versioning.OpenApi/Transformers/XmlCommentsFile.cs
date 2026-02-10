// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Transformers;

using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Reflection;
using FilePath = System.IO.Path;

internal sealed class XmlCommentsFile
{
    [UnconditionalSuppressMessage( "ILLink", "IL3000" )]
    public XmlCommentsFile( Assembly[] assemblies, IHostEnvironment environment )
    {
        var paths = new List<string>( capacity: 3 )
        {
            default!,
            environment.ContentRootPath,
            AppContext.BaseDirectory,
        };
        string? directory;
        int start;

        for ( var i = 0; i < assemblies.Length; i++ )
        {
            var assembly = assemblies[i];
            var fileName = FilePath.ChangeExtension( assembly.GetName().Name, ".xml" );

            if ( string.IsNullOrEmpty( fileName ) )
            {
                continue;
            }

            try
            {
                directory = FilePath.GetDirectoryName( assembly.Location );
            }
            catch ( NotSupportedException )
            {
                directory = default;
            }

            if ( string.IsNullOrEmpty( directory ) )
            {
                start = 1;
            }
            else
            {
                paths[0] = directory;
                start = 0;
            }

            for ( var j = start; j < paths.Count; j++ )
            {
                var path = FilePath.Combine( paths[j], fileName );

                if ( File.Exists( path ) )
                {
                    Path = path;
                    return;
                }
            }
        }

        Path = string.Empty;
    }

    public string Path { get; }
}