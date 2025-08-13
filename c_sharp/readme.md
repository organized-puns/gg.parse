gg.parse v(0.0.1)
======================

Simple tokenizer, parser and optionally compiler library for c#.

Core concepts:

- Rule (RuleBase), implements a function to parse input (char[], int[]) and turn them into Annotation
- Annotation, a description of some input and the rule which applies to that input
- Rule Graph, a collection of rules and one root rule to parse input
- A set of common rules (literal, sequence, not...)
- A ebnf tokenizer which generates tokens and parser which generates an ast-tree
- A compiler which takes a list of tokens and an ast tree and builds a RuleGraph based on said input
- A facade-like class, `EbnfParser.cs` which combines all of the above in a convenient package

*Example:*

Read an ebnf(like) file defining json tokens and a json grammar and build an AST:

```csharp

var jsonParser = new EbnfParser(	
					File.ReadAllText("assets/json_tokens.ebnf"), 
					File.ReadAllText("assets/json_grammar_optimized.ebnf"));

if (jsonParser.TryBuildAstTree(File.ReadAllText("assets/example.json"), out tokens, out astTree))) 
{
	Console.Write(jsonParser.Dump(jsonFile, tokens, astTree));
}
```


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

- EbnfParser if error tokens or nodes are reported in the result, set match to fail
	- CreateTokenizerFromEbnfFile is done
	- CreateParserFromEbnfFile needs to be done
- Add a test to see the compiler fail if rules with the same name are registered

- Clean up:  
  - Clean up unit tests and build proper examples
  - Add some more documentation, extend readme.
  - address all xxx

- rename json_grammar_optimized to json_grammar and json_grammar to json_grammar_basic
- add full/short names versions for "not /!" "any /." "optional /?" "zero_or_more /*", "one_or_more /+", "ignore, drop? /~", "transitive /#"
- add full/short names versions for "or /|"
- add alternative for "= / :"
- replace any with _, disallow identifier to start with '_'
- keywords should end with whitespace | non_keyword_char
- (Bug) add guard against infinite loop with zero or more (and other cases)

- add optional namespaces to avoid grammar / token name clash 

- build c# from rule table output, so there can be a compiled version so we can start building more forgiving ebnf parsers

- implement a Ebnf based EbnfParser and Tokenizer



- Do All of the following based on ebnf assets, not in the bootstrap
	implement alternatives for short hand
	see if sequence can go without ,
	see if range can go without {}

- Implement a calculator

- Implement a function console