(Data)Rule: MatchDataSequence<T>
=============================

Matches a sequence of data. 

C#
--

Example of succesful input:

```csharp

// see MatchDataSequenceTests 
public void MatchSimpleData_SuccessExample()
{
    var rule = new MatchDataSequence<char>("matchFoo", [.."foo"]);

    IsTrue(rule.Parse([.."foo"], 0));
    IsTrue(rule.Parse([.."barfoo"], 3));
}
```

Example of unsuccesful input:

```csharp
// see MatchDataSequenceTests 
public void MatchSimpleData_FailureExample()
{
    var rule = new MatchDataSequence<char>("matchFoo", [.. "foo"]);

    // input is empty
    IsFalse(rule.Parse([], 0));

    // should only match foo, not bar
    IsFalse(rule.Parse([.."bar"], 0));

    // should capitals
    IsFalse(rule.Parse([.."Foo"], 0));
}
```

Script
------

The token to match a data sequence is a quoted string, eg 'foo' 

```csharp

is_foo = 'foo';

```

Note that data sequences are only available in tokenizers, not grammars.
