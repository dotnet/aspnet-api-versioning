To address name collisions and provide control over how collation happens, API Versioning provides the following
service:

```c#
public interface IControllerNameConvention
{
    string NormalizeName( string controllerName );
    string GroupName( string controllerName );
}
```

`NormalizeName` controls how or whether a controller name is _normalized_. `GroupName` provides the name used to group
and collate on, which may not necessarily be the same as the _normalized_ name. `ControllerNameConvention` provides
three implementations out-of-the-box.

### Default

`ControllerNameConvention.Default` provides the default configuration which extends the original convention to have the
form: `<Name>[#]Controller`. This means that if you already have a `HelloWorldController`, you can now have a
`HelloWorld2Controller` and `HelloWorld3Controller`. Each type name removes the `Controller` suffix as well as any
trailing numbers. All of these controllers would end up named and grouped `HelloWorld`.

### Original

`ControllerNameConvention.Original` provides an alternate configuration that retains the original naming convention.
Consider that you have a type named `S3Controller`. In this scenario, you do **not** want the `3` to be stripped away.
If you have multiple versions of a such a controller, you would need your own implementation that understands this
behavior or separate the types into different .NET namespaces.

### Grouped

`ControllerNameConvention.Grouped` is a hybrid configuration the combines the **Default** and **Original** conventions.
For the purposes of the name, the original convention is used. For the purposes of grouping, the default convention is
used. A controller type of `S3Controller` would have the name `S3`, but the group name `S`. The group name is only used
for collation and is never displayed anywhere, so this behavior is acceptable.