MetaRule: SkipRule<T>
=====================

Rule which tests if a condition applies to the current data pointer, if not skips a single data. 

The skip rule has two variables which determine its outcome, other than what the standard meta-rule provides:

* `FailOnEof`: if set to true, the rule will fail if it reaches the end of file (EoF) before finding a matching condition

* `StopBeforeCondition`: if set to true, the data pointer will stop at the point where the provided condition matches, ie an exclusive skip. Otherwise it will also consume the data making up the condition, ie an inclusive skip.

Note that the names in parse script do not mention 'skip' at all but instead use 
* 'find' with `FailOnEof` = true and `StopBeforeCondition` = true, 
* 'stop_at' with `FailOnEof` = false and `StopBeforeCondition` = true,  
* 'stop_after' with `FailOnEof` = false and `StopBeforeCondition` = false 

C#
--

Example find using matching input:

```csharp
public void FindStopAtDifferences_Examples()
{
    var fooRule = new MatchDataSequence<char>("match_foo", [.."foo"]);
    var findFoo = new SkipRule<char>("find_foo", AnnotationPruning.None, 0, 
                                        fooRule, failOnEof: true, stopBeforeCondition: true);
    var stopAtFoo = new SkipRule<char>("stopAt_foo", AnnotationPruning.None, 0,
                                        fooRule, failOnEof: false, stopBeforeCondition: true);


    var result = findFoo.Parse("barfoo", 0);

    IsTrue(result);

    // skipped until we find 'foo' at position 3
    IsTrue(result.MatchLength == 3);

   result = stopAtFoo.Parse("barfoo", 0);

    // same result as findFoo
    IsTrue(result);
    IsTrue(result.MatchLength == 3);

    result = findFoo.Parse("bar", 0);

    // no foo found in this bar
    IsFalse(result);

    result = stopAtFoo.Parse("bar", 0);

    // no foo found but we do not fail on eof
    IsTrue(result);
    IsTrue(result.MatchLength == 3);
}
```

Script
------

```csharp
// see gg.parse.doc.examples.test.SkipTests

// find a literal 'foo' and stop at its start, fail if there is no foo in the input
find_foo = find 'foo';

// stop at the start of a literal 'foo', succeeds whether foo is found or not
stop_at_foo = stop_at 'foo';

// stop after the start of a literal 'foo', succeeds whether foo is found or not. 
// This will also capture 'foo' as a child
stop_after_foo = stop_after 'foo';
```

Example showing the differences:

```csharp
// see gg.parse.doc.examples.test.SkipTests
var script = 
    "find_foo = find 'foo';"
    + "stop_at_foo = stop_at 'foo';"
    + "stop_after_foo = stop_after 'foo';";

var tokenizer = new ParserBuilder().From(script).TokenGraph;

var result = tokenizer["find_foo"].Parse("barfoo", 0);

IsTrue(result);
IsTrue(result.MatchLength == 3);
IsTrue(result.Annotations[0].Children == null);

result = tokenizer["stop_at_foo"].Parse("barfoo", 0);

IsTrue(result);
IsTrue(result.MatchLength == 3);
IsTrue(result.Annotations[0].Children == null);

result = tokenizer["stop_after_foo"].Parse("barfoo", 0);

IsTrue(result);
IsTrue(result.MatchLength == 6);
// stop after with will have captured the 'foo' match as well
// note this will ONLY happen if the rule is a top level rule
// inline rules will prune all
IsTrue(result.Annotations[0].Children != null);
IsTrue(result[0][0].Rule is MatchDataSequence<char>);
```