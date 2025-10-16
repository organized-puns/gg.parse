(Look-ahead) Rule: MatchCondition<T>
===================================

Look-ahead rule which tests if a condition holds, returns a match if succesful. 
This does NOT change the current position in the data.

C#
--

Example of succesful input:

```csharp

// see MatchConditionTests 
public void MatchSimpleCondition_SuccessExample()
{
    var fooRule = new MatchDataSequence<char>("match_foo", "foo".ToArray());
    var isFooRule = new MatchCondition<char>("match_foo_condition", fooRule);

    IsTrue(isFooRule.Parse("foo", 0));
    // look-ahead does not return a length
    IsTrue(isFooRule.Parse("foo", 0).MatchLength == 0);
}
```

Example of unsuccesful input:

```csharp
// see MatchConditionTests 
public void MatchSimpleData_FailureExample()
{
    var fooRule = new MatchDataSequence<char>("match_foo", "foo".ToArray());
    var isFooRule = new MatchCondition<char>("match_foo_condition", fooRule);

    // input is empty
    IsFalse(isFooRule.Parse([], 0));

    // start position is beyond the input
    IsFalse(isFooRule.Parse("foo", 1));

    // bar is not foo
    IsFalse(isFooRule.Parse("bar", 1));
}
```

Script
------

The token to create a condition is 'if' (see gg.parse.doc.examples.test.MatchCondition_ScriptExample)

```csharp
is_foo = (if 'foo', info 'foo found') | info 'foo was not found';
```

```csharp
public void MatchCondition_ScriptExample()
{
    var builder = new ParserBuilder();
    var logger = new ScriptLogger()
    {
        Out = (level, message) => Debug.WriteLine($"[{level}]: {message}")
    };

    var tokenizer = builder.FromFile("assets/condition_example.tokens", logger: logger);
            
    // will output "foo found" 
    IsTrue(tokenizer.Tokenize("foo", processLogsOnResult: true));

    // will output "foo not found" 
    IsTrue(tokenizer.Tokenize("bar", processLogsOnResult: true));
}
```