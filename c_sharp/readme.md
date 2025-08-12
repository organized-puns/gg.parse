gg.parse v(0.0.1)
======================

Simple tokenizer, parser and optionally compiler library for c#.

Core concepts:

- Rule (RuleBase), implements a function to parse input (char[], int[]) and turn them into Annotation
- Annotation, a description of some input and the rule which applies to that input
- Rule Graph, a collection of rules and one root rule to parse input
- A set of common rules (literal, sequence, not...)
- A bootstrap ebnf tokenizer which generates tokens and parser which generates an ast-tree
- A compiler which takes a list of tokens and an ast tree and builds a RuleGraph based on said input

*Examples:*
	- Full ebnf parser see EbnfParserTest

Todo (for v1.0)
---------------

- include peek operator, ">func" which succeeds when func passes, but passes with length 0

- allow including other ebnf (ie include "some_ebnf.ebnf";)

- Clean up:
  - address all xxx
  - Add some more documentation, extend readme.

- (Bug) add guard against infinite loop with zero or more (and other cases)

- build c# from rule table output, so there can be a compiled version

- implement a Ebnf based EbnfParser and Tokenizer

- Clean up unit tests and build proper examples

- Do All of the following based on ebnf assets, not in the bootstrap
	implement alternatives for short hand
	see if sequence can go without ,
	see if range can go without {}

- Implement a calculator

- Implement a function console