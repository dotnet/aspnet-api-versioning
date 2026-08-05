# OpenAPI Options

The OpenAPI options allows you to configure, customize, and extend the default behaviors when you add OpenAPI support.
The configuration options are specified by providing a callback to the appropriate extension method:

The `VersionedOpenApiOptions` has the following configuration settings:

- [Description](#description)
- [Document](#document)
- [DocumentDescription](#document-description)

## Description

The description provides the `ApiVersionDescription` for the current OpenAPI document being generated. This information
includes the API version, its group name, and whether it is deprecated.

## Document

This provides access to the current `OpenApiOptions`. These are the same options you would configure when you document
an unversioned API. Your configuration can be the same for all versions or vary version by version.

## Document Description

The document description provides the configuration settings for the current OpenAPI document being generated. The
`description` for an OpenAPI document is defined as text, but most user interfaces allow Markdown to be specified.
These additional settings control the text and format to be included in the `description` attribute.

The `OpenApiDocumentDescriptionOptions` provide the following configuration settings:

- [HidePolicyLinks](#hide-policy-links)
- [DeprecationNotice](#deprecation-notice)
- [SunsetNotice](#sunset-notice)

### Hide Policy Links

This setting controls whether deprecation or sunset policy links are displayed. The default value is `false`, which
means any defined policies links are displayed as bulleted list of hyperlinks. Policy links rendered for user interface
purposes must have the link type `text/html`.

Policies may define other types of links, but these are not shown in the user interface. These links are rendered in
OpenAPI documents in the `x-api-versioning` document extension.

### Deprecation

If an API has an applicable deprecation policy, this setting defines the callback function used to generate the message
text based on the provided policy. The default setting returns a generic message in English indicating that the API is
deprecated if no date is specified or a date-specific message in English when the API became, or will become,
deprecated.

### Sunset

If an API has an applicable sunset policy, this setting defines the callback function used to generate the message text
based on the provided policy. If the policy does not have a date, then no message is generated; otherwise, a
date-specific message in English is generated indicating when the API became, or will become, sunset. A sunset policy is
for an API that is sunset and will no longer exist. The effective sunset policy date should always be in the future as
there should be no information about the API after it is sunset.