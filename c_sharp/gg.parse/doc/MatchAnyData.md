(Data)Rule: MatchAnyData<T>
===========================

Matches any single input as long as there is input left.

C#
--

Example of succesful input:

```csharp

using gg.parse.ebnf;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

...

var function = new MatchAnyData<char>("valid_match_any");

IsTrue(function.Parse(['a'], 0).FoundMatch);
IsTrue(function.Parse(['1'], 0).FoundMatch);
IsTrue(function.Parse(['%'], 0).FoundMatch);
```

Example of unsuccesful input:

```csharp
var function = new MatchAnyData<char>("invalid_match_any");

// input is empty
IsFalse(function.Parse([], 0).FoundMatch);

// start position is beyond the input
IsFalse(function.Parse(['a'], 1).FoundMatch);
```

Script
------

The token to match any character is '.'

```csharp

match_any = .;

// note this can be used in combination with not (!) to verify for the end of file (EOF)

EOF = !.;
```

