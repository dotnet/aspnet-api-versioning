# Existing Services

While it's great to plan for an API versioning story for your services upfront, it's all too common to need API
versioning after your services are in production. The ASP.NET versioning libraries provide features to help you retrofit
existing services and integrate formal API versioning without breaking your existing clients.

Unless a service is API version-neutral, existing services have some logical, yet undefined, API version that is not
formally declared by the service or known to a client. In order to prevent existing clients from breaking, they must be
able to make requests to the original URL without specifying any API version information.

When API versioning is applied, all of the existing services now have an explicit API version on the service side. The
initial, default API version is `1.0`, but that can be configured to be a different API version. All existing controller
definitions that do not have explicit API version definitions will now be implicitly bound to the default API version.
Once a controller has any API version attribution or conventions, it will never be implicitly matched. This enables
service authors to permanently sunset API versions over time. Controllers that have an implicit API version can be
confusing to service authors; especially, in a team environment. It is recommended that you explicitly apply API
versions to all of your existing services when you introduce formal API versioning.