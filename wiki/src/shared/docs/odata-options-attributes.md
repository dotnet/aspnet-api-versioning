## Attribute Model

The attribute model relies on _Model Bound_ settings attributes and the `EnableQueryAttribute`. The
`EnableQueryAttribute` indicates API-specific options that might be too restrictive or not applicable to specific
models. Consider the following model and controller definitions.

```c#
using System;
using Microsoft.AspNet.OData.Query;
using static Microsoft.AspNet.OData.Query.SelectExpandType;

[Select]
[Select( "effectiveDate", SelectType = Disabled )]
public class Order
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime EffectiveDate { get; set; } = DateTime.Now;
    public string Customer { get; set; }
    public string Description { get; set; }
}
````