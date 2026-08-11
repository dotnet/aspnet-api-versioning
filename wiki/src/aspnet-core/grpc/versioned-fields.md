<!-- description: Use annotations to control which API versions a Protocol Buffer field appears in. -->

# Versioned Message Fields

Protocol Buffer messages are designed to support backward compatibility. JSON schemas, on the other hand, can be strict
or not provide extension points. API-to-model affinity is one of the reasons to version a service in the first place.
gRPC messages do not have a built-in way to express this when they are transcoded.

API Versioning provides a set of custom annotations that enables decorating which message fields are visible in which
API versions. The `asp.api.version` annotation indicates an API version range which indicates which API versions the
field should be visible in. The specified value must follow the [interval notation].

While the examples illustrate numbers, any API version is valid; for example:

`[2026-07-01,2027-01-01) → 2026-07-01 ≤ x < 2027-01-01`

In rare cases, you might need a split range. The `asp.api.version` annotation supports multiple entries; for example:

`[(asp.api.version) = "[,2.0)", (asp.api.version) = "(2.0,]"]`

This would indicate a field that is included in every API version **except** `2.0`.

## Annotations

The following sample gRPC service demonstrates how fields can be annotated to indicate which API versions they appear
in. When no annotation is applied, the field appears in all API versions.

```protobuf
syntax = "proto3";

import "asp/api/annotations.proto";
import "google/api/annotations.proto";
import "google/protobuf/empty.proto";

package people;

message Person {
    int32 id = 1;
    string first_name = 2;
    string middle_name = 4 [(asp.api.version) = "2.0"];
    string last_name = 3;
    string email = 5 [(asp.api.version) = "2.0"];
    string phone = 6 [(asp.api.version) = "3.0"];
}

message PersonRequest {
  int32 id = 1;
  Person person = 2;
}

message PeopleReply {
    Person person = 1;
    repeated Person people = 2;
}

service People {
  // GET /people?api-version=[1.0,2.0,3.0]
  rpc GetPeople (google.protobuf.Empty) returns (PeopleReply) {
    option (google.api.http) = {
      get: "/people"
      response_body: "people"
    };
  }

  // GET /people/{id}?api-version=[1.0,2.0,3.0]
  rpc GetPerson (PeopleRequest) returns (PeopleReply) {
    option (google.api.http) = {
      get: "/people/{id}"
      response_body: "person"
    };
  }

  // POST /people?api-version=[1.0,2.0,3.0]
  rpc AddPerson (PersonRequest) returns (PeopleReply) {
    option (google.api.http) = {
      post: "/people"
      body: "person"
      response_body: "person"
    };
  }
}
```

## Validation

A transcoded message will appear to clients as though the field does not exist. This is pure obfuscation. The field
does exist, but is omitted. There is nothing to prevent a client from sending a field that exists, but does not apply
to an API version. A service author can choose to ignore the field according to the requested API version or the
service can return an error if the client sends fields that are unexpected.

[interval-notation]: ../how-to/versioned-models.md#notation