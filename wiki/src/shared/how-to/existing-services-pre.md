# Existing Services

It's a fairly common scenario that services are released to production and, at some point in the future, it becomes
evident that service versioning is needed. The question now becomes, _"How do I add API versioning without breaking
existing clients?"_

Before API versioning was applied to your service, clients were already bound to some version of the service; they just
don't know which version. A client in this situation doesn't have any flexibility to go backward. If the service
changes, hopefully that carries forward without breaking any clients. When you're ready to introduce formal API
versioning semantics into your service, then any previously unversioned services snap to a single, default API version.

## Enable Backward Compatibility

The default API versioning semantics require that all clients explicitly request an API version for a service. This
would break backward compatibility with existing clients, so we need a way to address this. The [API versioning options]
provide a way to change the default behaviors that will enable supporting services that don't explicitly declare API
versions.

The bare minimum requirement to enable backward compatibility is to assume the default API version when a client does
not explicitly request an API version. This will allow a client to continue making requests to existing services without
providing API version information. Your existing controller implementations that back these services do not require any
attribution or configuration to enable this behavior. Clients wishing to upgrade to new versions of a service must begin
explicitly specifying an API version.