# Swashbuckle Integration

Although the API explorers for API versioning provide all of the necessary information, there is select information
that OpenAPI (formerly Swagger) and Swashbuckle will not wire up for you. This includes iterating through all the
available API versions so that they don't have to be imperatively declared and changed one at a time. Fortunately,
bridging this gap is really easy to achieve using Swashbuckle's extensibility model. The following are simple
`IOperationFilter` implementations that leverage the metadata provided by the corresponding API explorer to fill in
these gaps.