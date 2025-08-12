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

Extending the EBNF Parser
-------------------------

- Create a new rule class
- Optionally create a tokenname for the rule in `CommonTokenNames.cs`
- Optionally create a shorthand for the rule in `CommonRules.cs`
- Add the rule to the `EbnfTokenizer.cs` constructor
- Add the new tokens to the `EbnfTokenParser.cs` and create a matching function property
- Don't forget to add the matching function to `ruleTerms.RuleOptions` in the `EbnfTokenParser.cs` constructor
- Create a corresponding Compile rule in `CompilerFunctions.cs`
- Register this compile rule to the `RegisterTokenizerCompilerFunctions` / `RegisterGrammarCompilerFunctions`
 
Adding tests:

- Add a new `MyNewFunctionTest.cs` to the testproject `/rules` and perform the appropriate tests
- Add a new test method to `EbnfTokenizerTests.cs` to test token parsing.
- Add a new test method to `EbnfTokenParserTests.cs` to test token parsing or add it to `ParseRule_ExpectSucess`.
- Add a compiler test in `RuleCompilerTests.cs`
- Add an integration test, add the rule in an .ebnf, parse/compile and test.

Todo (for v1.0)
---------------

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