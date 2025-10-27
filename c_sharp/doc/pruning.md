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

```c_sharp

// We are not interested in a "json_token" node, but we'd rather deal with just the
// actual type. Therefore prune the root. 
-r json_token = float | int | boolean | null | string | scope_start | scope_end | array_start | array_end | kv_separator | item_separator;

```

Adding pruning modifiers to anything other than rules and rule references in parse script will result in an error.

Furthermore rules in parse script have their own default modifier depending on whether or not the rule is a named/toplevel rule or an anonymous rule