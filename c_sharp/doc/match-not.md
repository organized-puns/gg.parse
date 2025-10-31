(Look-ahead) Rule: MatchNot<T>
===================================

Look-ahead rule which tests if a condition does NOT holds, returns a match if NOT succesful. 
This does NOT change the current position in the data.

C#
--

Example of succesful input:

```csharp

// see MatchNotTests 
public void MatchNot_SuccessExample()
{
    var fooRule = new MatchDataSequence<char>("match_foo", [.."foo"]);
    var isNotFooRule = new MatchNot<char>("match_foo_condition", fooRule);
    
    IsTrue(isNotFooRule.Parse("bar", 0));
    // look-ahead does not return a length
    IsTrue(isNotFooRule.Parse("bar", 0).MatchLength == 0);
}
```

Example of unsuccesful input:

```csharp
// see MatchNotTests 
public void MatchNot_FailureExample()
{
    var fooRule = new MatchDataSequence<char>("match_foo", [.. "foo"]);
    var isNotFooRule = new MatchNot<char>("match_foo_condition", fooRule);

    IsFalse(isNotFooRule.Parse("foo", 0));
}
```

Script
------

The token to create a condition is '!' (see gg.parse.doc.examples.test.MatchNotTests#MatchNot_ScriptExample)

```csharp
not_foo = !'foo';
```

```csharp
public void MatchNot_ScriptExample()
{
    var logger = new ScriptLogger(output: (level, message) => Debug.WriteLine($"[{level}]: {message}"));
    var tokenizer = new ParserBuilder().From("root = if !'foo', info 'not foo';", logger: logger);
    
    // will output "not foo " 
    IsTrue(tokenizer.Tokenize("bar", processLogsOnResult: true));

    // will output nothing
    IsFalse(tokenizer.Tokenize("foo", processLogsOnResult: true));
}
```