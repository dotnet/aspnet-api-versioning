# Error Responses

There are several built-in error responses. The body of each error response complies with [RFC 7807: Problem Details].

>[!NOTE]
>In earlier versions, the error responses bodies complied with the [Microsoft REST Guidelines error response format],
which is itself the error response format used by the OData protocol (see [OData JSON Format §21.1]). There wasn't a
broad standard at that time, which made any common error response format sensible.

Each problem detail also contains a `code` extension to retain a level of backward compatibility for clients that may
have relied on that value. If you need to retain the old functionality, refer to
[backward compatibility](#backward-compatibility) below.

### Unspecified

All versioned services require that an API version be specified. When a client makes a request without providing an API
version, then the server will respond with a bad request. This behavior is typically not exhibited when the API is
version-neutral or the `AssumeDefaultVersionWhenUnspecified` option is configured to true.

| | |
| - | - |
| **Title**  | Unspecified API version |
| **Type**   | https://docs.api-versioning.org/problems#unspecified |
| **Status** | 400 |
| **Detail** | An API version is required, but was not specified |
| **Code**   | ApiVersionUnspecified |

### Unsupported

When a client requested API version does not match any of the available controllers or their actions, then the server
will respond with a problem. If the `ReportApiVersions` option is true, then the supported versions will be returned
to the client in the `api-supported-versions` HTTP header.

| | |
| - | - |
| **Title**  | Unsupported API version |
| **Type**   | https://docs.api-versioning.org/problems#unsupported |
| **Status** | 400<sup>1</sup> <sup>2</sup> |
| **Detail** | The specified API version is not supported |
| **Code**   | UnsupportedApiVersion |

><sub>1: Defined by `ApiVersioningOptions.UnsupportedApiVersionStatusCode`</sub><br/>
<sub>2: The value is always `404` when versioning by URL segment</sub>

### Invalid

When a client makes a request with an API version, but the value is malformed or cannot be parsed, then the server will
respond with a bad request. This typically occurs where the value contains incomplete version components or the
date-only form is invalid (ex: 2016-02-30).

| | |
| - | - |
| **Title**  | Invalid API version |
| **Type**   | https://docs.api-versioning.org/problems#invalid |
| **Status** | 400 |
| **Detail** | An API version was specified, but it is invalid |
| **Code**   | InvalidApiVersion |

### Ambiguous

When a client requests a specific API version, the specified API version must be unambiguous to the server. A client is
allowed to specify an API version more than once, but if the values are not identical, then the server will respond
with a bad request.

| | |
| - | - |
| **Title**  | Ambiguous API version |
| **Type**   | https://docs.api-versioning.org/problems#ambiguous |
| **Status** | 400 |
| **Detail** | An API version was specified multiple times with different values |
| **Code**   | AmbiguousApiVersion |


#### Examples

```http
GET /resource?api-version=1.0 HTTP/1.1
host: localhost
api-version: 1.0
```
_Figure 1: Multiple, unambiguous API versions requested_

```http
GET /resource?api-version=1.0 HTTP/1.1
host: localhost
api-version: 2.0
```
_Figure 2: Ambiguous API versions requested between in query string and headers_

```http
GET /resource?api-version=1.0&api-version=2.0 HTTP/1.1
host: localhost
```
_Figure 3: Ambiguous API versions requested in the query string_

```http
GET /resource HTTP/1.1
host: localhost
api-version: 1.0
api-version: 2.0
```
_Figure 4: Ambiguous API versions requested in the headers_