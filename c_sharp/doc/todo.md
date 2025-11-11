Todo 
========

Overview of all open items in order of priority for the upcoming release (0.3). Often badly defined.

```mermaid
---
config:
  kanban:
    ticketBaseUrl: 'https://github.com/mermaid-js/mermaid/issues/#TICKET#'
---
kanban
  Backlog
       
    [ add loop guard ]
	[ Add linter to workflows ]
  
  In progress
    
    [ Add rule examples ]
    [ Implement properties file ]                      
        [ properties files - improving token/grammar recovery, both fail after just one error and don't recover ]
        [ properties files - replace parserbuilder with parser where possible, eg examples ]
        [ properties files - allow for ini files/java properties ]
        [ properties files - redo calculator based on compiler template ]
        [ properties files - redo rule compiler based on compiler template ]
        [ properties files - redo arg parser ]
        [ clean up, write doc ]
        	
  Done
    [ properties files - move parser graphs to read only, thread safe implementations ]
    [ properties files - test for deliberate errors, adding basic error reporting. ]
    [ properties files - adding precision: double, float to compile options ]
    [ properties files - simplify meta information to '_type': 'str' ]
    [ properties files - test read<obj> behavior (should default to annotation based)]
    [ properties files - allow for reading of array<obj> list<obj> and set<obj> ]
    [ properties files - enums ]
    [ properties files - safe instantiation, allow for simpler meta information types ]
    [ properties files - move properties to its own project ]
    [ properties files - moving examples to subdirectory ]
    [ properties files - split interpreter from reader ]    
    [ properties files - compiler base ]
    [ Add repeat count to script ]
    [ Replace skip tokens and find tokens with 'stop_at' 'stop_after' and 'find' ]
	[ Add meta rule vs data rule vs rule composition ]
    [ Remove callback scripting, add BreakPoint ]

```

Details
-------------

Future backlog
--------------


### Fix/extend literal

Having a literal in a grammar leads to very confusing errors. Should be handled better.
Add literalRule which allows for case senstive matching or not

### Add transpiling
Transpile / build c# from rule table output, so there can be a compiled version so we can start building more forgiving ebnf parsers

### Deal with endless loop, ie loop = loop;
? can set the out come to fail or throw exception ?

### Add extend to parser
Add Extend() to existing parser, similar to merge

### Matcher example
add BuildMatcher() class (add function to Graphbuilder?) which takes a tokenizer rule term and will match a string and has
     all common tokens defined
	eg var ip4AddressMatcher = BuildMatcher("byte, '.', byte, '.', byte, '.', byte, '.', optional(':', word)")
	   var ranges = ip4AddressMatcher.Find("#this are the ip addresses 127.9.21.12, 256.12.12.3:8080") => two ranges

### Compare performance against regex

### Recreate bootstrap in script
- Do All of the following based on ebnf assets, not in the bootstrap
	implement alternatives for short hand (see json_grammar_test.ebnf)
	see if sequence can go without ,
	see if range can go without {}

- implement a Ebnf based EbnfParser and Tokenizer
- add full/short names versions for "not /!" "any /." "optional /?" "zero_or_more /*", "one_or_more /+", "ignore, drop? /~", "transitive /#"
- add full/short names versions for "or /|"
- add alternative for "= / :"
- replace any with _, disallow identifier to start with '_' ?

### Add symbols to log

Add interpolatable tokens to errors, eg {token}, {position}, {line}, {column}, {file} etc to log

### Linter

dotnet format --verify-no-changes

name: Lint

on: [push, pull_request]

jobs:
  lint:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Check formatting
        run: dotnet format --verify-no-changes

### Add annotation example

### Implement (json) annotations in its main program


### Namespaces
Add optional namespaces to avoid grammar / token name clash 

### Example, small function console
Implement a function console


???
---
- Figure out a way to capture the annotation associated with a reference, so in case of errors we can report the correct line/column
  (probably by adding a property to ReferenceRule which is set during compilation)

