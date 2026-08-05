# API Version Conventions

API version conventions allow you to specify API version information for your services without having to use .NET
attributes. There are a number of reasons why you might choose this option. The most common reasons are:

- Centralized management and application of all service API versions
- Apply API versions to services defined by controllers in external .NET assemblies
- Dynamically apply API versions from external sources; for example, from configuration

Instead of applying `[ApiVersion]` to the controller, we can instead choose to define a convention in the
API versioning options.