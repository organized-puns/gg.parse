(Other) Rule: BreakpointRule
============================

Allows setting a breakpoint on a specific rule via code or in script. 

Even with a small tokenizer/grammar, debugging a specific can get a chore as you may have pass through many layers of rules before getting to the rule you want to debug. To make life a little easier you can set a breakpoint on a specific rule. This adds a meta rule where the subject is whatever rule you're trying to debug. When running in debug mode, the debugger will stop before the Parse method of the subject is called.

One can set a breakpoint calling AddBreakpoint with the rule name:

```csharp
var breakPoint = builder.GrammarGraph.AddBreakpoint("foo");
```
When foo is being parsed the debugger will stop at this point:

```csharp
public override ParseResult Parse(T[] input, int start)
{
    Assertions.RequiresNotNull(Subject);

    // debugger will stop here 
    Debugger.Break();

    var result = Subject.Parse(input, start);
    return BuildResult(new util.Range(start, result.MatchLength), result.Annotations);
}
```

In script one can achieve the same with the `break` keyword:

```csharp
root = foo, break bar, baz;
foo = 'foo';
bar = 'bar';
baz = 'baz';
```