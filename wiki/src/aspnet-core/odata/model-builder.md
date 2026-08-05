{{#include ../../shared/odata/model-builder-pre.md}}

>[!NOTE]
>`IModelConfiguration` instances are automatically discovered through _Dependency Injection_ when you declare
>`IEnumerable<IModelConfiguration>` or `VersionedODataModelBuilder` as a dependent parameter. The
>`ModelConfigurations` property can be modified after injection, if required.

{{#include ../../shared/odata/model-builder-post.md}}