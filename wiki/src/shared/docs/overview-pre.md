# API Documentation

Adding documentation is often the final, pivotal step in making your versioned services available to clients and fosters
their utilization. While there are many approaches to documenting your services, OpenAPI (formerly Swagger) has quickly
become the de facto method for describing REST services.

The ASP.NET API versioning project provides several new API explorer implementations that make it easy to add versioning
into your OpenAPI configurations. Each of these API explorers do all of the heavy lifting to discover and collate your
REST services by API version. They do not directly rely on nor use any external OpenAPI libraries so that you can use
them for other scenarios as well.