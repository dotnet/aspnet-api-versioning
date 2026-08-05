{{#include ../../shared/docs/options-pre.md}}
- [FormatGroupName](#format-group-name)

### Format Group Name

This option allows you to define an optional `FormatGroupNameCallback`, which will provide the current group name and
formatted API version. By default, the formatted API version is used as the group name and is the most logical choice.
A developer, however, may specify their own group name in a variety of ways such as
`[ApiExplorerSettings(GroupName = "Custom")]`. When a developer explicitly sets a group name, that name is honored.
If, and **only** if, a developer sets both a custom group name and defines a `FormatGroupName` callback, the method
will be invoked to produce a combination of both.

Consider the following controller:

```c#
[ApiVersion( 1.0 )]
[ApiExplorerSettings( GroupName = "Sales" )]
[Route( "[controller]" )]
public class OrderController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok();
}
```

A callback can be defined to control how the combination of the API version and group name will be formatted.

```c#
builder.Services.AddApiVersioning()
                .AddMvc()
                .AddApiExplorer(
                    options =>
                    {
                        // the default is ToString(), but we want "'v'major[.minor][-status]"
                        options.GroupNameFormat = "'v'VVV";

                        // if we have both parts, decided how to format the group
                        // from the example: "Sales - v1"
                        options.FormatGroupName = (group, version) => $"{group} - {version}";
                    } );
```

{{#include ../../shared/docs/odata-options-post.md}}