# API Versioning with OData

Service API versioning using OData is similar to the normal configuration with a few slight variations. Each implemented
OData controller has an associated entity set and each entity set is defined in an Entity Data Model (EDM). Once we
introduce API versioning, each versioned OData controller now needs an EDM per API version. To satisfy this requirement,
we'll use the new [VersionedODataModelBuilder], build a collection of EDMs for each API version, and then map a set of
routes for them.

[VersionedODataModelBuilder]: model-builder.md