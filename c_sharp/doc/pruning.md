Pruning
=======

Syntax trees have a tendency to become rather sizeable. In the case of gg.parse, large syntax trees become harder to work with as the user will need to do extra work to traverse the tree to find the interesting parts. Moreover it builds an implicit dependency between the handling code and the parser, which in turn makes it harder to make changes as they may break existing assumptions the code dealing with the syntax tree must make.

Having this dependency between the tokens/grammar generating a syntax tree and the code using it, is unavoidable. What we can do it make it a little easier. One way is to reduce the amount of dependencies is to avoid generating unnecessary nodes in the abstract syntax tree (or tokens). In gg.parse this is implemented via pruning.

Each rule has a pruning strategy which determines - when a match is found - what nodes are kept and which ones are disgarded. This strategy is captured in the `Prune` (enum) property of `IRule`. There are four strategies:

* Prune nothing and keep all nodes via `AnnotationPruning.None`. 

* Prune everything via `AnnotationPruning.All`. 

* Prune the root but keep any children, `AnnotationPruning.Root`. 

* Keep only the root and prune the children via `AnnotationPruning.Children`. 

To illustrate this via some top of the line ascii art

```
   a                                   a
  /|\   => AnnotationPruning.None =>  /|\
 b c d                               a b c

   a                                   
  /|\   => AnnotationPruning.All => ∅
 b c d                               

   a                                   
  /|\   => AnnotationPruning.Root =>  b c d
 b c d                               

   a                                   
  /|\   => AnnotationPruning.Children => a
 b c d                               
```

In parse script these pruning modifiers can be added to _rules_ and _rule references_, via the modifiers

* -a: `AnnotationPruning.All`
* -r: `AnnotationPruning.Root` 
* -c: `AnnotationPruning.Children`

If none of these modifiers is present, `AnnotationPruning.None` is assumed, ie no pruning will be applied.

For example:

```js

// We are not interested in a "json_token" node, but we'd rather deal with just the
// actual type. Therefore prune the root. 
-r json_token = float | int | boolean | null | string | scope_start | scope_end | array_start | array_end | kv_separator | item_separator;

```

Adding pruning modifiers to anything other than rules and rule references in parse script will result in an error. For example:

```js

rule_1    = -a rule_2;           // ✅ modifier in front of rule_2 reference

rule_2    = "foo" | "bar"; 

-c rule_3 = {'abc'};             // ✅ modifier in front of rule_3 declaration

rule_4    = -a (rule_1, rule_2); // ❌ modifier in front of a group

rule_5    = -r "foo";            // ❌ modifier in front of a literal

```

Default pruning rules
---------------------

Furthermore rules in parse script have their own _**default pruning**_ depending on whether or not the rule is a _named/toplevel_ rule or an _anonymous/inline_ rule. 

For instance the "one of rule" in this example is a (trivial) top-level rule:

```js
top_level_example = "a" | "b" | "c";
```

Which is compiled into a rule with the `AnnotationPruning.None` pruning strategy and yields a root node being a `MatchOneOf<char>` and a `MatchDataSequence<char>` child (a literal):

```c_sharp
var text = "a";
var tokens = new ParserBuilder()
                .From("top_level_example = 'a' | 'b' | 'c';")
                .Tokenize(text);

IsTrue(tokens && tokens.Count == 1);
IsTrue(tokens[0] == "top_level_example");
IsTrue(tokens[0].Rule is MatchOneOf<char>);
IsTrue(tokens[0][0].Rule is MatchDataSequence<char>);

// will print:
// [0,1]top_level_example(0): a
//   [0,1].lit('a')(1): a

Debug.WriteLine(ScriptUtils.PrettyPrintTokens(text, tokens.Annotations));
```

An example of an inline / anonymous rule would be

```js
// ('a', 'b') and ('b', 'c') are inline rules
inline_example = ('a', 'b') | ('b', 'c');
```

Because most of the time the handling code is not interested in processing the inline rules, in these case `MatchRuleSequence`, only the results of these inline rules will be passed (ie, `AnnotationPruning.Root` ). For example, the following demonstrates how these inline rules are pruned from the result.

```c_sharp
   var text = "ab";
   var tokens = new ParserBuilder()
                   .From("inline_example = ('a', 'b') | ('b', 'c');")
                   .Tokenize(text);

   IsTrue(tokens && tokens.Count == 1);
   IsTrue(tokens[0] == "inline_example");
   IsTrue(tokens[0].Rule is MatchOneOf<char>);
   // line / anonymous rules
   IsTrue(tokens[0][0].Rule is MatchDataSequence<char>);
   IsTrue(tokens[0][1].Rule is MatchDataSequence<char>);

   // will print:
   // [0,2]inline_example(0): ab
   //   [0,1].lit('a')(2): a
   //   [1,2].lit('b')(3): b

   Debug.WriteLine(ScriptUtils.PrettyPrintTokens(text, tokens.Annotations));
```

To recap:

* top-level rules: `AnnotationPruning.None`, pass everything
* rule references: Depends on the pruning modifier, if none is provided `AnnotationPruning.None`.
* inline composition-rules: `AnnotationPruning.Root` for the rule itself, `AnnotationPruning.None` for its children. This includes `MatchRuleSequence<T>` (ie `a,b,c`), MatchOneOf<T> (ie `a | b | c`) and `MatchEvaluation' (ie `a / b / c`)
* inline count-rules: `AnnotationPruning.Root` for the rule itself, `AnnotationPruning.None` for its children. This includes all variations of MatchCount<T>, ie `zero-or-one +`, `zero-or-more *` and `one-or-more +`
* look-ahead rules: `AnnotationPruning.All` for both itself and its children. This includes `MatchNot<T>`, `MatchCondition<T> (if ...)`, `SkipRule<T> (>> and >>>)`.

For more details have a look at how these are implemented in `gg.parse.script.compiler.CompilerFunctions`.