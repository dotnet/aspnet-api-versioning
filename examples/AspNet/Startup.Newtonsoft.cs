namespace ApiVersioning.Examples;

using Newtonsoft.Json;

public partial class Startup
{
    // REF: https://github.com/advisories/GHSA-5crp-9r3c-p9vr
    static Startup() => JsonConvert.DefaultSettings = () => new() { MaxDepth = 128 };
}