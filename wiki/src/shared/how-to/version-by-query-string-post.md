The effect of this attribution is that the following requests match different controller implementations:

| Request URL                     | Matched Controller    |
|:--------------------------------|:----------------------|
| /api/helloworld?api-version=1.0 | HelloWorldController  |
| /api/helloworld?api-version=2.0 | HelloWorld2Controller |
| /api/People?api-version=1.0     | PeopleController      |
| /api/People?api-version=2.0     | People2Controller     |

It’s important to note that only an undecorated controller will be inferred as the configured, default API version. Once
a controller has any API version attribution, it will never be considered as the default API version again unless the
API version attribute includes the default API version. This allows you permanently remove API versions over time.
