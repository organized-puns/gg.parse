(Meta)Rule: RuleReference<T>
==============================

Allows re-use and composition of defined rules.

Implementation Details
----------------------

The `RuleReference<T>` class contains a string-based reference to another rule and invokes this rule when parsing (provided the name is resolved at compile time). This allows for reuse and composition of rules. Eg

```csharp
foo               = 'foo';
one_or_more_foos  = +foo; // reference to foo
zero_or_one_foo   = ?foo; // re-using the same foo
```

Rule references allow for forward declaration. In parse script you can refer to rules which may be defined later in the input as rules are resolved at the end of compilation. See `gg.parse.script.compiler.RuleCompiler#ResolveReferences`.

Rule-reference allow for [pruning](./pruning.md) customized for its use case where the reference is used inline. Eg

```csharp
letter = {'a'..'z'} | {'A'..'Z'};
quote = '"';
string = -a quote, *letter, -a quote; // prune the quote
block_quote = quote, *letter, quote; // keep the quotes
```

By allowing inline prune modifiers we avoid having to redefine the same rule each time we want different pruning behavior. 

Rule references can be used inline to refer to- and reuse existing or as a toplevel rule to 'tag' rules such that the application of the rules may become clear when inspecting the syntax tree. Tagging may insert this new rule (depending on the prune modifiers) as a new node in the syntax tree. Eg

```csharp
// when parsing using this rule the resulting syntax tree will look like this in user_name -> string -> *letter
user_name = string; 

// when parsing using this rule the resulting syntax tree will look like this in real_name -> string -> *letter
real_name = string; 
```

Note that rule headers and rule references allow for their own pruning modifiers. In the case of a top level rule, the resulting tree will be the result of applying first the rule's pruning and then applying the rule reference's pruning. Computing the resulting outcome can be somewhat complex, see `gg.parse.rules.RuleReference#Parse` and `gg.parse.script.tests.parserbuilder.PruneModifierTests` for more information.

C#
---

Example (see gg.parse.doc.examples.test\RuleReferenceTests.cs):

```csharp
var fooRule = new MatchDataSequence<char>("match_foo", [.."foo"]);
var matchFooByReference = new RuleReference<char>("ref_match_foo", "match_foo")
{
    // note when using references in a script, the compiler will automatically
    // resolve this
    Rule = fooRule
};

IsTrue(matchFooByReference.Parse("foo"));
```