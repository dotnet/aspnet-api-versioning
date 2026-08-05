## Versioning Methods

Several API versioning methods are supported out-of-the-box:

- [By Query String](how-to/version-by-query-string.md) (default)
- [By Media Type](how-to/version-by-media-type.md)
- [By Header](how-to/version-by-header.md)
- [By URL Segment](how-to/version-by-url.md)

Multiple methods of API versioning can be supported simultaneously. Use the `ApiVersionReader.Combine` method to compose
two or more [IApiVersionReader] instances together. You can also implement your own method of extracting the requested
API version using a custom [IApiVersionReader].

[IApiVersionReader]: config/api-version-reader.md