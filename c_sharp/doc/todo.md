Todo 
========

Overview of all open items in order of priority for the upcoming release (0.1). Often badly defined.

```mermaid
---
config:
  kanban:
    ticketBaseUrl: 'https://github.com/mermaid-js/mermaid/issues/#TICKET#'
---
kanban
  Backlog
    
    [Have parser names in ScriptParser/Tokenizer comply with compiler generated names]
  
  In progress
    
    [Add rule examples]
	[implement explicit root, add documentation]
	
  Done
	[Create Graph Documentation]
	[Add rule reference documentation]
	[Made rule, annotation slightly more immutable]
	[Document Pruning]
	[output passthrough unintuitive, possibly wrong]
	[Add to nuget]	
    [Add missing names to CompilerFunctionNameGenerator functions]   
	[Fix all warnings]
	[Validate github PR permssions]
	[Add tests to check in rules]
	[Change rule output in favor of 'prune']

```

Details
-------------

### 3. Fix ambiguous root
Should be able to set root based on name 'root', if there is no root specified the first rule will be chosen

Future backlog
--------------

### Add meta rule vs data rule vs rule composition

### Add replace rule to parser
Add ReplaceRule(oldRuleName, newRule) to parser, allowing for different type with same name, rename existing replace
to update.

### Improve callback support

### Adjust find/skip
include 'inclusive' property to skip rules eg inclusive find +-> -->, inclusive skip +->> -->>
	  or find, find_including, skip, skip_including  
		or >>, +>>, >>>, +>>>, ->|, |->, ->>|, |->> stop_at, stop_after, skip_to, skip_after	 

### Add repeat 
Add repeat count to script '[3]' (min 3, max 3) [..,3] max 3 min 0 [3,..] min 3 max 0 [3,3] 


### Add Properties File Example   
Implement properties file

### Add annotation example

### Implement (json) annotations in its main program

### Fix/extend literal

Having a literal in a grammar leads to very confusing errors. Should be handled better.
Add literalRule which allows for case senstive matching or not

### Namespaces
Add optional namespaces to avoid grammar / token name clash 

### Example, small function console
Implement a function console

### Add transpiling
Transpile / build c# from rule table output, so there can be a compiled version so we can start building more forgiving ebnf parsers

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



???
---
- Figure out a way to capture the annotation associated with a reference, so in case of errors we can report the correct line/column
  (probably by adding a property to ReferenceRule which is set during compilation)
