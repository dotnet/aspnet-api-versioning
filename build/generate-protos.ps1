<#
.SYNOPSIS
 Compiles the API versioning Protocl Buffers annotations into C#.

.DESCRIPTION
 The generated code is checked in so that the library has no build-time dependency on protoc.
 Consuming projects only ever *import* these .proto files (see build\*.targets in the packages),
 which means the extension identifiers must be defined exactly once - here. The generated types must be
 public: code generated for a consumer's .proto references the imported file's reflection class by name
 (for example global::Asp.Versioning.AnnotationsReflection.Descriptor).

 Re-run this script whenever a .proto file under a src\**\protos folder changes.

.PARAMETER GrpcToolsVersion
 The version of the Grpc.Tools package that supplies protoc and the well-known imports.
#>
[CmdletBinding()]
param (
    [string] $GrpcToolsVersion = '2.80.0'
)

$ErrorActionPreference = 'Stop'

$rootDir = Split-Path -Parent $PSScriptRoot
$packageDir = Join-Path $env:USERPROFILE ".nuget\packages\grpc.tools\$GrpcToolsVersion"

if ( -not ( Test-Path $packageDir ) ) {
    throw "Grpc.Tools $GrpcToolsVersion is not in the local NuGet cache. Build a project that references it (for example Asp.Versioning.Grpc.ApiExplorer.Tests) and try again."
}

$protoc = Join-Path $packageDir 'tools\windows_x64\protoc.exe'
$wellKnownProtos = Join-Path $packageDir 'build\native\include'
$header = '// Copyright (c) .NET Foundation and contributors. All rights reserved.'

# projects that ship .proto files. the generated code is emitted next to the project file so that it
# lands in the Asp.Versioning namespace folder, matching the csharp_namespace of the .proto files
$projects = @(
    ( Join-Path $rootDir 'src\AspNetCore\WebApi\src\Asp.Versioning.Grpc.ApiExplorer' )
)

foreach ( $project in $projects ) {
    $protoRoot = Join-Path $project 'protos'
    $protos = Get-ChildItem -Path $protoRoot -Filter *.proto -Recurse

    if ( $protos.Count -eq 0 ) {
        continue
    }

    Write-Host "Generating $($protos.Count) proto file(s) for $(Split-Path -Leaf $project)..."

    & $protoc `
        --proto_path=$protoRoot `
        --proto_path=$wellKnownProtos `
        --csharp_out=$project `
        --csharp_opt=file_extension=.g.cs `
        $protos.FullName

    if ( $LASTEXITCODE -ne 0 ) {
        throw "protoc exited with code $LASTEXITCODE."
    }

    # protoc has no option to emit a license header, so prepend it after the fact
    foreach ( $proto in $protos ) {
        $generated = Join-Path $project "$( ( Get-Culture ).TextInfo.ToTitleCase( $proto.BaseName ) ).g.cs"

        if ( -not ( Test-Path $generated ) ) {
            continue
        }

        $content = Get-Content -Path $generated -Raw

        if ( -not $content.StartsWith( $header ) ) {
            Set-Content -Path $generated -Value "$header`r`n`r`n$content" -NoNewline
        }

        Write-Host "  -> $( Resolve-Path -Relative $generated )"
    }
}