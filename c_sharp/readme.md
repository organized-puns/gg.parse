gg.parse v(0.0.1)
======================

Quickstart
----------------------

Simple tokenizer, parser and optionally compiler library for c# based on ebnf-like grammar definitions.

Core concepts:

- Rule (RuleBase), implements a function to parse input (char[], int[]) and turn them into Annotation
- Annotation, a description of some input and the rule which applies to that input
- Rule Graph, a collection of rules and one root rule to parse input
- A set of common rules (literal, sequence, not...)
- A ebnf tokenizer which generates tokens and parser which generates an ast-tree
- A compiler which takes a list of tokens and an ast tree and builds a RuleGraph based on said input
- A facade-like class, `EbnfParser.cs` which combines all of the above in a single convenient class

## Example

Read an ebnf(like) file defining json tokens and a json grammar and build an AST:

```csharp

var jsonParser = new EbnfParser(	
					File.ReadAllText("assets/json_tokens.ebnf"), 
					File.ReadAllText("assets/json_grammar.ebnf"));

if (jsonParser.TryBuildAstTree(File.ReadAllText("assets/example.json"), out tokens, out astTree))) 
{
	Console.Write(jsonParser.Dump(jsonFile, tokens, astTree));
}
```



## Error handling

Since the EbnfParser builds both a tokenizer and parser, there are two types of exceptions (in the current implementation) which are thrown as the inner-exception of an `EbnfException.cs`. 
The latter identifies where an exception took place. a `TokenizeException.cs` exception is used when tokenization fails. A `ParseException.cs` is used when parsing fails.

Note that the errors in this example are the "fall-back" error. This fall back error implies the EbnfParser either encounters a token for which it has no rules or it cannot find a mapping to a grammar rule. In both cases it throws its hands up in the air and reports "I don't know what to do with this". It's up to the ebnf specification to provide more detailed error (more on that later).

Eg: tokenization in the input text of the tokenizer is invalid:

```csharp

    try
    {
        // & and ^ are no valid tokens, so this should raise an exception
        var parser = new EbnfParser("& foo ^", null);
    }
    catch (EbnfException ebnfException)
    {
        // The message ebnfException will indicate where it went wrong 
        // (in building the tokenizer)
        var errors = (ebnfException.InnerException as TokenizeException).Errors;

        Console.WriteLine(ebnfException.Message);

        // write the two errors to Console, note that this is currently not very
        // informative as the errors are Annotations with minimal information.
        foreach (var error in errors)
        {
            Console.WriteLine(error);
        }
    }
```

Eg: forgetting a ; after a rule in the grammar can be handled with


```csharp
    try
    {
        var parser = new EbnfParser("foo='bar';", 
                                // first rule, is_bar, has no ;
                                "is_bar=foo is_not_bar = !bar;");
    }
    catch (EbnfException ebnfException)
    {
        // The message ebnfException will indicate where it went wrong 
        // (in building the parser)
        var errors = (ebnfException.InnerException as ParseException).Errors;

        Console.WriteLine(ebnfException.Message);

        // write the two errors to Console, note that this is currently not very
        // informative as the errors are Annotations with minimal information.
        foreach (var error in errors)
        {
            Console.WriteLine(error);
        }
    }
```

## Extending the EBNF Parser

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

Good to remember (FAQ?):
------------------------

- bug? Seems rule = #(a | b) or ~(...) is not parsing ? production operator issue with group?
    IS _AS INTENDED_... groups by definition are always transitive and any other.
-   Data functions, eg {'a'..'b'} ALWAYS have production `none` because have them annotated is just overhead (that is to say I can't think of a good use case at the moment).
    IT SHOULD have a clear error though, something like "found production rule without identifier".

Todo (for v1.0)
---------------

- Clean up:  
  - Clean up unit tests and build proper examples
  - Add some more documentation, extend readme.
  - address all xxx

- Error handling, example: var (_, _, _, table) = SetupTokenizeParseCompile("rule1 100= ,;"); <- ',' should be a '.', this should be immediately clear
  or var (_, _, _, table) = SetupTokenizeParseCompile("rule1 100= .;#rule2 200 = .; *rule_three -1 = .;"); <- '*' should be a '~'

- keywords should end with whitespace | non_keyword_char
- (Bug) add guard against infinite loop with zero or more (and other cases)

- add optional namespaces to avoid grammar / token name clash 

- build c# from rule table output, so there can be a compiled version so we can start building more forgiving ebnf parsers

- add BuildMatcher() class (add function to EbnfParser?) which takes a tokenizer rule term and will match a string and has
     all common tokens defined
	eg var ip4AddressMatcher = BuildMatcher("byte, '.', byte, '.', byte, '.', byte, '.', optional(':', word)")
	   var ranges = ip4AddressMatcher.Find("#this are the ip addresses 127.9.21.12, 256.12.12.3:8080") => two ranges


- Do All of the following based on ebnf assets, not in the bootstrap
	implement alternatives for short hand (see json_grammar_test.ebnf)
	see if sequence can go without ,
	see if range can go without {}

- implement a Ebnf based EbnfParser and Tokenizer
- add full/short names versions for "not /!" "any /." "optional /?" "zero_or_more /*", "one_or_more /+", "ignore, drop? /~", "transitive /#"
- add full/short names versions for "or /|"
- add alternative for "= / :"
- replace any with _, disallow identifier to start with '_' ?

- Implement a function console