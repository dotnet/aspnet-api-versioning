Although not illustrated in these examples, it’s important to note that different versions of a service action might
have different return values. The effect of the API versioning attribution is that the following requests match
different controller and action implementations:

| Request URL                     | Matched Controller    | Matched Action |
|:--------------------------------|-----------------------|----------------|
| /api/helloworld?api-version=1.0 | HelloWorldController  | Get            |
| /api/helloworld?api-version=2.0 | HelloWorld2Controller | Get            |
| /api/helloworld?api-version=3.0 | HelloWorld2Controller | GetV3          |
| /api/People?api-version=1.0     | PeopleController      | Get            |
| /api/People?api-version=2.0     | People2Controller     | Get            |
| /api/People?api-version=3.0     | People2Controller     | GetV3          |

It should be reiterated that the defined API version, even for an action, never directly influences routing.  When the
action matched for a route is ambiguous, the selection process will look for an explicit API version that matches the
requested API version.  If an explicit match is not found, then the action will be implicitly matched. If two actions are ambiguous by route and API version, then this is a developer mistake and the default behavior is unchanged.