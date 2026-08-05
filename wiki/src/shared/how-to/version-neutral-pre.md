# Version-Neutral

All services should be explicitly versioned. In rare cases, however, you may have a service that is _version-neutral_.
A common scenario is a health check service that behaves in the exact same way, regardless of API version. This might
also apply to a legacy service that doesn't support API versioning. To effectively _opt out_ individual services from
API versioning, a service must indicate that it is _version-neutral_.

Technically, it's not a supported scenario to completely _opt out_ of API versioning. A _version-neutral_ service has
the following characteristics:

* Accepts **any** valid API version
* Accepts no API version at all (e.g. unspecified)

This is an important distinction and why the term _version-neutral_ is used. A _version-neutral_ service accepts any and
all versions, including none. This behavior can be used to define a service that accepts all API versions or service
that simply does not care about specific API versions.

It is not possible to have some versions of a controller that are API version-neutral and other versions of the same
controller require an explicit API version.  If the route of an API version-neutral service matches any other service,
it will result in an ambiguous match (e.g. server error).