## Removing a Service

To permanently sunset a service, simply remove that controller or API version from your implementation. The route will
no longer be matched. When one or more specific API versions cannot be matched, clients will receive HTTP status code
`400` (Bad Request). If no candidate routes match at all, clients will receive HTTP status code `404` (Not Found).