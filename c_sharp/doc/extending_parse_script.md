Extending Parse Script
----------------------

This document provides an overview of the steps required to add a new Rule to the ScriptParser / Tokenizer.

If you're adding a new token:

- Create a new rule class.
- Optionally create a tokenname for the rule in `gg.parse.script.common.CommonTokenNames`.
- Optionally create a shorthand for the rule in `gg.parse.script.common.CommonTokenizer`
- Add the rule to the `ScriptTokenizer` constructor (in the correct place).
- Create a corresponding Compile rule in `CompilerFunctions`.
- Register this compile rule to the `RegisterTokenizerCompilerFunctions` in `gg.parse.script.pipeline.ScriptPipeline`.

If you are adding new grammar.

- Create a new rule class.
- Optionally a new name for the rule in `gg.parse.script.parser.ScriptParser.Names`.
- Optionally create a shorthand for the rule in `gg.parse.script.common.CommonParser`
- Add the rule to the `ScriptParser` constructor (in the correct place).
- Create a corresponding Compile rule in `CompilerFunctions`.
- Register this compile rule to the `RegisterGrammarCompilerFunctions` in `gg.parse.script.pipeline.ScriptPipeline`.

Adding tests:

- Add a new `MyNewRuleTest.cs` to the testproject `gg.parse.tests/rules` and perform the appropriate unit tests
- Add a new test method to `gg.parse.script.tests.unit.ScriptTokenizerTests` to test token parsing.
- Add a new test method to `gg.parse.script.tests.unit.ScriptParserTests.cs` to test grammar parser.
- Add a compiler test in `gg.parse.script.tests.integration.CompilerFunctionsTests.cs`

- Make sure no other tests are broken :)
