(Data)Rule: MatchAnyData<T>
===========================

Matches any single input as long as there is input left.

C#
--

Example of succesful input:

```csharp

// see MatchAnyDataTests 
public void MatchSimpleData_SuccessExample()
{
    var rule = new MatchAnyData<char>("match_any_example");

    IsTrue(rule.Parse(['a'], 0));
    IsTrue(rule.Parse(['1'], 0));
    IsTrue(rule.Parse(['%'], 0));
}
```

Example of unsuccesful input:

```csharp
// see MatchAnyDataTests 
public void MatchSimpleData_FailureExample()
{
    var rule = new MatchAnyData<char>("match_any_example");

    // input is empty
    IsFalse(rule.Parse([], 0));

    // start position is beyond the input
    IsFalse(rule.Parse(['1'], 1));
}
```

Script
------

The token to match any character is '.'

```csharp

match_any = .;

// note this can be used in combination with not (!) to verify for the end of file (EOF)

EOF = !.;
```

