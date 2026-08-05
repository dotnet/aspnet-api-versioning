# Deprecating Versions

When a service supports multiple API versions, some versions will eventually be deprecated over time.  To advertise that
one or more API versions have been deprecated, simply decorate your controller with the deprecated API versions. A
deprecated API version does not mean the API version is not supported. A deprecated API version means that the version
will become unsupported after six months or more.

The following examples illustrate how to specify deprecated API versions depending on which service API versioning
approach you selected.