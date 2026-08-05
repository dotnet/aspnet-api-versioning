The effect of the API version attribution is that the following requests match different controller implementations:

| Request URL        | Matched Controller    | Matched Action |
|:-------------------|:----------------------|----------------|
| /api/v1/helloworld | HelloWorldController  | Get            |
| /api/v2/helloworld | HelloWorld2Controller | Get            |
| /api/v3/helloworld | HelloWorld2Controller | GetV3          |
| /api/v1/People     | PeopleController      | Get            |
| /api/v2/People     | People2Controller     | Get            |
| /api/v3/People     | People2Controller     | GetV3          |
