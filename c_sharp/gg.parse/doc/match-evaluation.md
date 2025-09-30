(Meta)Rule: MatchEvaluation<T>
==============================

Builds a precedence-based tree of a set of rules.

Implementation Details
----------------------

The `MatchEvaluation<T>` class is a generic rule that constructs a precedence-based evaluation tree from a collection of rules. It inherits from `RuleBase<T>` and implements the `IRule` interface.

Now that AI generated line is out of the way, let's look at the code. The original intention was to build a Pratt parser (https://en.wikipedia.org/wiki/Operator-precedence_parser), however it turned into something else because of the various preceding implementation choices, ie: it had to work within the Rule / Annotation setup. In essence it still does the same but much less elegantly.

The code starts Parsing, like all rules with a list of tokens in the form of annotations, a starting position and a set of operators to choose from (see above). We'll outline process step by step, then give an example. 

1. Find the first operator which matches the input at the current position. 
2. If found the resulting annotation is now considered the root and current parent of the resulting tree.
3. If there is no more input left, the parsing is done. Build the result based on this rules' IRule.Output strategy.

So far so good, we've parsed out first operator from the annotation list. Now we're parsing the remainder for as far as we have valid input.

4. Get the next operation. To get the next operation we _first move the input pointer one step back so we can match complete binary operation_. We don't have to worry about unary operations because within this scripting language they occupy a single (nested) token, so from this operations perspective they are atomic.

5. Repeat the following as long as we have input and operation matches left:
	5.1 Find the next matching operator (note if we encounter an annotation containing an error we give up immediately).
	5.2 Get the precedence of the new operator and compare it to the current parent operator. As long as the new operator has a _lower precedence_ than the current parent we move up the tree, ie: set the current parent to its own parent.
	5.3 Eventually we end up with a parent that has a lower precedence OR with a null parent, which means we are at the root of the tree.
	5.4 If we're at the root, the root becomes the new operator and the old root becomes its left child. If we're at a node with a lower precedence, the new operator becomes the right child of that node and the old right child of that node becomes the left child of the new operator.
	5.5 The current operator becomes the current parent. We move the input pointer one step back and repeat from 5

6. Update the root's range so it capture the entire input range and return the result.

Here's an example of how this works. Consider the following:

* An input of 5 tokens, `1 + 2 * 3`, 
* An instruction point (ip) = 0, 
* An annotation node representing the `parent` operation 
* An annotation node representing the `current` operation
* The resulting tree

step 0, initial conditions:

```
input[5]: 1 + 2 * 3
ip: 0
parent: null
current: null
result: null
```

step 1, intialize:

```
input[5]: 1 + 2 * 3
ip: 3
parent: 1 + 2
current: null
result: 
	+
   / \
  1   2
```

step 2, get the next operation:

```
input[5]: 1 + 2 * 3
ip: 2
parent: 1 + 2
current: 2 * 3
result: 
	+
   / \
  1   2
```

step 3, compare precedence, move up the tree, in this case `+` has a lower precedence so we are done, replace the parent's right with the new node after which there's no more input:

```
input[5]: 1 + 2 * 3
ip: 2
parent: 1 + 2
current: 2 * 3
result: 
	+
   / \
  1   *
	 / \
	2   3
```

step 3 again, compare precedence, move up the tree, in this case let's _ASSUME_ `+` has a higher precedence so we have to move up.

```
input[5]: 1 + 2 * 3
ip: 2
parent: null
current: 2 * 3
result: 
	+
   / \
  1   2
```

since the parent is null, we're at the root, so we make the new node the root and the old root its left child:

```
input[5]: 1 + 2 * 3
ip: 2
parent: null
current: 2 * 3
result: 
	*
   / \
  +   3
 / \
1   2
```

C#
---

Example of succesful input:


Script
------
