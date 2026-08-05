# Version Advertisement

Splitting implemented service API versions across hosted applications or endpoints is a fairly common scenario. There
are several reasons why you might choose to split hosted endpoints, such as different run-time versions or traffic load
balancing.

When service API versions are split across deployments, two issues arise:

1. The correct service API version cannot be selected across deployments.
2. The set of implemented service API versions cannot be aggregated across deployments.

## Service Gateway

The first issue can be remedied by a using a service gateway.  The gateway becomes responsible for obfuscating which
endpoints host which API versions. The exact method in which gateways implement this functionality is at the discretion
of service authors.

Future consideration is being investigated to support [YARP](https://github.com/microsoft/reverse-proxy).

## Service API Version Advertisement

Since there is no direct way to know or interrogate the available API version information at runtime in a performant
manner when services are deployed separately, an alternate approach is required. This concept is referred to as
*service API version advertisement*. Each service will advertise the supported and deprecated API versions it knows
about.

A service can advertise its supported and deprecated API versions using the `AdvertiseApiVersionsAttribute`. This
attribute functions almost identically to the `ApiVersionAttribute`, except that it is never considered for controller
resolution and cannot be applied to an action. The advertised and implemented API versions are always aggregated
together.

The following is an example of a service with API version `2.0` hosted at another endpoint that knows that API version
`1.0` is a supported version somewhere else: