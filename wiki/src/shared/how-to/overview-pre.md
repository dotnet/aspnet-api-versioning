# How to Version Your Service

REST services are implemented in ASP.NET as an endpoint. To version your service, you simply need to decorate your
endpoints with the appropriate API version information. The method of decoration will vary depending on whether you are
using controllers or Minimal APIs as well as whether you want to use attributes or conventions.

## How It Works

The way that you create and define routes remains unchanged. The key difference is that routes may now overlap depending
on whether you are using convention-based routing, attribute-based routing, or both. In the case of attribute routing,
multiple controllers will define the same route. The default services in each flavor of ASP.NET assumes a one-to-one
mapping between routes and endpoints and, therefore, considers duplicate routes to be ambiguous. The API versioning
services replace the default implementations and allow endpoints to also be disambiguated by API version. Although
multiple routes may match a request, they are expected to be distinguishable by API version. If the routes cannot be
disambiguated, this is likely a developer mistake and the behavior is the same as the default implementation.

## Naming and Collation

While it might seem more intuitive that similar route templates are collated together, that is simply not the case.
Consider that `order/{id}` and `order/{id:int}` are different, but semantically identical. API Versioning makes no
attempt understand this difference. Although it is possible to have an API with a single endpoint, most APIs consist of
a collection of endpoints; for example the _Orders_ API. What if we saw the route template `order/{id}/items`? Is this
part of the _Orders_ API or some other API? For this reason, API Versioning collates on the logical name of an API and
not individual route templates. For more information see: [Controller Conventions].

[Controller Conventions]: naming-conventions.md

## Routing Methods

The following table outlines the various supported routing methods:

| Routing Method                                 | Supported |
|:-----------------------------------------------|:---------:|
| Attribute-based routing                        | Yes       |
| Convention-based routing                       | Yes       |
| Attribute and convention-based routing (mixed) | Yes       |