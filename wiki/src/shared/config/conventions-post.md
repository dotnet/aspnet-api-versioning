### Namespace

This built-in convention allows you to version your controllers by the .NET namespace they reside in when applied.

```c#
options.Conventions.Add( new VersionByNamespaceConvention() );
```

The defined namespace name must conform to the API version format so that it can be parsed. The language-neutral syntax
is:

```ebnf
letter = "A" | "B" | "C" | "D" | "E" | "F" | "G"
       | "H" | "I" | "J" | "K" | "L" | "M" | "N"
       | "O" | "P" | "Q" | "R" | "S" | "T" | "U"
       | "V" | "W" | "X" | "Y" | "Z" | "a" | "b"
       | "c" | "d" | "e" | "f" | "g" | "h" | "i"
       | "j" | "k" | "l" | "m" | "n" | "o" | "p"
       | "q" | "r" | "s" | "t" | "u" | "v" | "w"
       | "x" | "y" | "z" ;

prefix = "v" | "V" ;

positive = "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" ;

digit = "0" | positive ;

day = ( [ "0" ] positive ) | ( "1" | "2" ) digit | ( "3" ( "0" | "1" ) ) ;

month = ( [ "0" ] positive ) | ( "1" ( "0" | "1" | "2" ) ) ;

year = 4 * digit ;

api-version = prefix ( ( year "_" month "_" day ) | ( digit [ "_" digit ] ) ) [ "_" { letter } ] ;

```

The `.` character is considered a namespace delimiter in many programming languages. This character must be changed to
`_` so that newly added files have the correct format. In addition, most languages do not allow the name of a namespace
to start with a number. Since a leading character is required, the first character **must** be `v` or `V`. There is no
requirement as to where the API version must appear in the namespace.

By default, API versions derived from a namespace will be considered supported. If the controller is decorated with the
`ObsoleteAttribute`, then the API version inferred from the containing namespace will be considered deprecated.

**Examples**

- `Contoso.Api.v1.Controllers` → 1.0
- `Contoso.Api.v1_1.Controllers` → 1.1
- `Contoso.Api.v0_9_Beta.Controllers` → 0.9-Beta
- `Contoso.Api.v20180401.Controllers` → 2018-04-01
- `Contoso.Api.v2018_04_01.Controllers` → 2018-04-01
- `Contoso.Api.v2018_04_01_Beta.Controllers` → 2018-04-01-Beta
- `Contoso.Api.v2018_04_01_1_0_Beta.Controllers` → 2018-04-01.1.0-Beta

```
Contoso
└ Api
  ├─ v1
  │  └ Controllers
  ├─ v2
  │  └ Controllers
  └─ v2_5
     └ Controllers
```
> _Figure 1:_ Sample folder layout with numeric API versions

```
Contoso
└ Api
  ├─ v2018_07_01
  │  └ Controllers
  ├─ v2018_08_01
  │  └ Controllers
  └─ v2018_09_01
     └ Controllers
```
> _Figure 2:_ Sample folder layout with date API versions