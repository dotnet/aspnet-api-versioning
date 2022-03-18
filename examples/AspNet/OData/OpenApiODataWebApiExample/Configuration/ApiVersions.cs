namespace ApiVersioning.Examples.Configuration;

using Asp.Versioning;

internal static class ApiVersions
{
    internal static readonly ApiVersion V1 = new( 1, 0 );
    internal static readonly ApiVersion V2 = new( 2, 0 );
    internal static readonly ApiVersion V3 = new( 3, 0 );
}