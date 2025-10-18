(Data)Rule: MatchDataRange<T>
=============================

Matches any single input as long as as the input is within a specified range.

C#
--

Example of succesful input:

```csharp

// see MatchDataRangeTests 
public void MatchSimpleData_SuccessExample()
{
    var rule = new MatchDataRange<char>("matchLowercaseLetter", 'a', 'z');

    IsTrue(rule.Parse(['a'], 0));
    IsTrue(rule.Parse(['d'], 0));
    IsTrue(rule.Parse(['z'], 0));
}
```

Example of unsuccesful input:

```csharp
// see MatchDataRangeTests 
public void MatchSimpleData_FailureExample()
{
    var rule = new MatchDataRange<char>("matchLowercaseLetter", 'a', 'z');

    // input is empty
    IsFalse(rule.Parse([], 0));

    // should only match lower case letters
    IsFalse(rule.Parse(['A'], 0));

    // should not match numbers
    IsFalse(rule.Parse(['1'], 0));
}
```

Script
------

The token to match a data rage is '{start..end}', eg 

```csharp

lower_case_letter = {'a'..'z'};

```
