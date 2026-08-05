# Migration From Previous Versions

This topic serves as the guide for migrating from version `<= 5.x.x` to version `>= 6.0.0`. The majority of this
information has been outlined in previous [discussions].

>[!NOTE]
>If you'd like more information on the background context, you can read the [Hello Project "Asp"] announcement.

For the most part, you can expect the required changes to be a new package identifier and different namespaces. It is
entirely possible that you may update those and find the rest of the code to be identical. The mileage will vary
depending on your level of customization, but you can expect the changes to be trivial in most cases.

[discussions]: https://github.com/dotnet/aspnet-api-versioning/discussions
[Hello Project "Asp"]: https://github.com/dotnet/aspnet-api-versioning/discussions/807