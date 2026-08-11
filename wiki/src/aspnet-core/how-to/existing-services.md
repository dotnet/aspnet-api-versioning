<!-- description: Add API versioning to an ASP.NET Core service without breaking clients that never sent a version. -->

{{#include ../../shared/how-to/existing-services-pre.md}}

```c#
services.AddApiVersioning( options => options.AssumeDefaultVersionWhenUnspecified = true );
```

{{#include ../../shared/how-to/existing-services-mid.md}}

```c#
services.AddApiVersioning(
    options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion( new DateOnly( 2016, 7, 1 ) );
    } );
```

{{#include ../../shared/how-to/existing-services-post.md}}