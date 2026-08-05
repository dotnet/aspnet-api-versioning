# Version Format

Services are versioned using a version group (e.g. date) or major and minor version scheme with an optional status. The
version format has the following syntax:

```ebnf
letter = "A" | "B" | "C" | "D" | "E" | "F" | "G"
       | "H" | "I" | "J" | "K" | "L" | "M" | "N"
       | "O" | "P" | "Q" | "R" | "S" | "T" | "U"
       | "V" | "W" | "X" | "Y" | "Z" | "a" | "b"
       | "c" | "d" | "e" | "f" | "g" | "h" | "i"
       | "j" | "k" | "l" | "m" | "n" | "o" | "p"
       | "q" | "r" | "s" | "t" | "u" | "v" | "w"
       | "x" | "y" | "z" ;

positive = "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" ;

digit = "0" | positive ;

day = ( [ "0" ] positive ) | ( "1" | "2" ) digit | ( "3" ( "0" | "1" ) ) ;

month = ( [ "0" ] positive ) | ( "1" ( "0" | "1" | "2" ) ) ;

year = 4 * digit ;

group = year "-" month "-" day ;

version = { digit } [ "." { digit } ] ;

status = letter [ { letter | digit | "." } { letter | digit } ] ;

api-version = ( group | version ) [ "-" status ] ;

```

The version status allows you to provide a condition to a version such as **alpha**, **beta**, **rc**, and
so on.  While the status is optional, either the version group or the major and minor versions must be specified.

## Versioned Request

By default, clients must explicitly request the version of a service via the **api-version** query string parameter or
URL path segment per the [Microsoft REST Guidelines for versioning]. It is possible to customize this behavior for
legacy and other non-compliant services, which will be covered in the **Advanced Versioning** topic.

>[!NOTE]
>When versioning by URL segment, the `v` prefix is neither required nor part of the API version.

[Microsoft REST Guidelines for versioning]: https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#12-versioning

## Versioned Request Examples

The following outlines examples of various service version formats:

- /api/foo?api-version=1.0
- /api/foo?api-version=2.0-alpha
- /api/foo?api-version=2015-05-01.3.0
- /api/v1/foo
- /api/v2.0-alpha/foo
- /api/v2015-05-01.3.0/foo

## Custom

The `ApiVersion` class implements `IFormattable` and uses the `ApiVersionFormatProvider` for formatting by default. The
following table outlines the supported format specifiers.

| Format<br>Specifier | Description | Examples |
| ---------------- | ----------- | -------- |
| F      | Full API version as<br>_[group version][.major[.minor]][-status]_ | 2017-05-01.1-RC -><br> 2017-05-01.1-RC |
| FF     | Full API version with optional minor version as<br>_[group version][.major[.minor,0]][-status]_ | 2017-05-01.1-RC -><br> 2017-05-01.1.0-RC |
| G      | Group version as _yyyy-MM-dd_ | 2017-05-01.1-RC -><br> 2017-05-01 |
| GG     | Group version as _yyyy-MM-dd_ with status | 2017-05-01.1-RC -><br> 2017-05-01-RC |
| y      | Group version year from 0 to 99 | 2001-05-01.1-RC -> 1 |
| yy     | Group version year from 00 to 99 | 2001-05-01.1-RC -> 01 |
| yyy    | Group version year with a minimum of three digits | 2017-05-01.1-RC -> 017 |
| yyyy   | Group version year as a four-digit number | 2017-05-01.1-RC -> 2017 |
| M      | Group version month from 1 through 12 | 2001-05-01.1-RC -> 5 |
| MM     | Group version month from 01 through 12 | 2001-05-01.1-RC -> 05 |
| MMM    | Group version abbreviated name of the month | 2001-06-01.1-RC -> Jun |
| MMMM   | Group version full name of the month | 2001-06-01.1-RC -> June |
| d      | Group version day of the month, from 1 through 31 | 2001-05-01.1-RC -> 1 |
| dd     | Group version day of the month, from 01 through 31 | 2001-05-01.1-RC -> 01 |
| ddd    | Group version abbreviated name of the day of the week | 2001-05-01.1-RC -> Mon |
| dddd   | Group version full name of the day of the week | 2001-05-01.1-RC -> Monday |
| v      | Minor version | 2001-05-01.1-RC -> 1<br>1.1 -> 1 |
| V      | Major version | 1.0-RC -> 1<br>2.0 -> 2 |
| VV     | Major and minor version | 1-RC -> 1<br>1.1-RC -> 1.1<br>1.1 -> 1.1 |
| VVV    | Major, optional minor version, and status | 1-RC -> 1-RC<br>1.1 -> 1.1 |
| VVVV   | Major, minor version, and status | 1-RC -> 1.0-RC<br>1.1 -> 1.1<br>1 -> 1.0 |
| S      | Status | 1.0-Beta -> Beta |
| p      | Padded minor version with default of two digits | 1.1 -> 01<br>1 -> 00 |
| p[_n_] | Padded minor version with _N_ digits | **p2**: 1.1 -> 01<br>**p3**: 1.1 -> 001 |
| P      | Padded major version with default of two digits | 2.1 -> 02<br>2 -> 02 |
| P[_n_] | Padded major version with _N_ digits | **P2**: 2.1 -> 02<br>**P3**: 2.1 -> 002 |
| PP     | Padded major and minor version with a default of two digits | 2.1 -> 02.01<br>2 -> 02.00 |
| PPP    | Padded major, optional minor version, and status with a default of two digits | 1-RC -> 01-RC<br>1.1-RC -> 01.01-RC |
| PPPP   | Padded major, minor version, and status with a default of two digits | 1-RC -> 01.00-RC<br>1.1-RC -> 01.01-RC |

### Custom Examples 

```c#
var apiVersion = new ApiVersion( 1, 0 );
Console.WriteLine( "Welcome to version " + apiVersion.ToString( "V" ) );

apiVersion = new ApiVersion( 1, 1, "Beta" );
var message = string.Format( "Welcome to version {0:VV}{0:' ('S')'}", apiVersion );
Console.WriteLine( message );

apiVersion = new ApiVersion( 2, 0 );
message = string.Format( "Welcome to version {0:VV}{0:' ('S')'}", apiVersion );
Console.WriteLine( message );

// Output: Welcome to version 1
// Output: Welcome to version 1.1 (Beta)
// Output: Welcome to version 2.0
```
