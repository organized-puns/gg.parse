(Meta) Rule: MatchCount<T>
===================================

Meta rule which tests if a given rule can be matched between N and M times. If N or M are zero or negative, no limits are imposed. N and M are called 'min' and 'max' in the implementation. 

The MatchCount rules allows for derived rules like:

* 'Zero or one' implementing a MatchCount with min = 0, max = 1. In script is denoted as a '?'.
* 'Zero or more' implementing a MatchCount with min = 0, max = 0. In script is denoted as a '*'.
* 'One or more' implementing a MatchCount with min = 1, max = 0. In script is denoted as a '+'.

C#
--

```csharp

Example of match exactly 3 rules:

// see MatchCountTests 
public void MatchSimpleData_SuccessExample()
{
    var fooRule = new MatchDataSequence<char>("match_foo", [.."foo"]);
    var match3Foos = new MatchCount<char>("match_three_foos", fooRule, min:3, max:3);

    // must get 3 foos on the input
    IsTrue(match3Foos.Parse("foofoofoo"));

    // not less
    IsFalse(match3Foos.Parse("foofoo"));
}
```

Example of match exactly zero or more rules:

```csharp
public void MatchZeroOrMore_SuccessExample()
{
    var fooRule = new MatchDataSequence<char>("match_foo", [.. "foo"]);
    var matchZeroOrMoreFoos = new MatchCount<char>("match_*_foos", fooRule, min: 0, max: 0);

    // basically anything is fine for matchZeroOrMoreFoos, it will always succeed
    // the only thing that will differ is the match length
    IsTrue(matchZeroOrMoreFoos.Parse(""));
    IsTrue(matchZeroOrMoreFoos.Parse("").MatchLength == 0);

    IsTrue(matchZeroOrMoreFoos.Parse("bar"));
    IsTrue(matchZeroOrMoreFoos.Parse("bar").MatchLength == 0);

    IsTrue(matchZeroOrMoreFoos.Parse("foo"));
    IsTrue(matchZeroOrMoreFoos.Parse("foo").MatchLength == 3);

    IsTrue(matchZeroOrMoreFoos.Parse("foofoo"));
    IsTrue(matchZeroOrMoreFoos.Parse("foofoo").MatchLength == 6);
}
```

Script
------

Currently there is no implementation in the script to set a specific min or max on the match count. We do have symbols for 

* zero_or_one: '?'
* zero_or_more: '*'
* one_or_more: '+'

```csharp
// gg.parse.doc.examples.test\assets\match_count_example.tokens
match_zero_or_more_foos = *foo, info "zero or more foos found";
match_zero_or_one_foo = ?foo, info "zero or one foo found";
match_one_or_more_foos = +foo, info "one or more foos found";
```

```csharp
// see MatchCountTests 
public void MatchCount_ScriptExample()
{
    var logger = new ScriptLogger((level, message) => Debug.WriteLine($"[{level}]: {message}"));
    var tokenizer = new ParserBuilder()
                    .FromFile("assets/match_count_example.tokens", logger: logger);

    // will output "zero or more foos found"
    IsTrue(tokenizer.Tokenize("foofoo", usingRule: "match_zero_or_more_foos", processLogsOnResult: true));

    // will also output "zero or more foos found"
    IsTrue(tokenizer.Tokenize(
        "barbazqazquad", 
        usingRule: "match_zero_or_more_foos", 
        processLogsOnResult: true
    ));

    // will output "zero or one foo found"
    IsTrue(tokenizer.Tokenize("foofoo", usingRule: "match_zero_or_one_foo", processLogsOnResult: true));

    // will output "one or more foos found"
    IsTrue(tokenizer.Tokenize("foo", usingRule: "match_one_or_more_foos", processLogsOnResult: true));

    // will output "no foos found :("
    IsTrue(tokenizer.Tokenize("bar", usingRule: "match_one_or_more_foos", processLogsOnResult: true));
}
```