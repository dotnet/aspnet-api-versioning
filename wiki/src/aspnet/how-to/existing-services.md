{{#include ../../shared/how-to/existing-services-pre.md}}

```c#
config.AddApiVersioning( options => options.AssumeDefaultVersionWhenUnspecified = true );
```

{{#include ../../shared/how-to/existing-services-mid.md}}

```c#
configuration.AddApiVersioning(
    options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion( new DateTime( 2016, 7, 1 ) );
    } );
```

{{#include ../../shared/how-to/existing-services-post.md}}