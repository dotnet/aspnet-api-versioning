# Version Format

It is possible to extend or change the provided API version format, but that capability comes with several rules:

1. You must extend `ApiVersion`
2. You must override:
   - `GetHashCode`
   - `CompareTo`
   - `ToString(string,IFormatProvider)`
3. You must implement `IApiVersionParser`
   - It may be possible to extend `ApiVersionParser` depending on your requirements

You will likely need to extend `ApiVersionFormatProvider` or implement a custom `IFormatProvider`. Although not
strictly required, you may want to implement operator overloads for your custom type to retain functional parity with
`ApiVersion`. The custom parser will need to be passed to components that accept `IApiVersionParser` and/or replace the
default implementation registered for dependency injection.

You should consider the impact that a custom API version may have on clients. Your custom format and parsing logic may
need to be distributed to them for to use.