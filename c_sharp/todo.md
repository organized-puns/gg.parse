Todo 0.1 first public release
-----------------------------

- Clean up unit tests and build proper examples
- Add some more documentation, extend readme.

0.2 Adjust and fix output in favor of 'prune':
----------------------------------------------
this is weird:
array_item_list   = value, *(~item_separator, value);
array			  = ~array_start, ?array_item_list, ~array_end;
?array_item_list 
	yields zero_or_one of the children of array_item_list
	it should yield the children of zero_or_one of array_item_list
	but not ?(a | b | c) -> should yield a, b, c not (a|b|c)

value = a | b | c; should yield value(a) not a BUT value = (a | b | c ) | 'foo' should ield  value(a) or value('foo')

- better compile name generation, have parser names comply with generated names

Adjustments:
- replace output with 'prune' 
	such that:  
	- prune(none, parent_node(child1, child2.. childN)) =  parent_node(child1, child2.. childN)
	- prune(parent, parent_node(child1, child2.. childN)) =  (child1, child2.. childN)
	- prune(children, parent_node(child1, child2.. childN)) =  parent_node
	- prune(all, parent_node(child1, child2.. childN)) =  null

- consider change '~' to '-' and '#' to '..' (?) or `~` to '--' and '#' to '-'...
	? -c, -p -a 

0.21 Add Properties File Example   
------------------------------
- Implement properties file


0.3
---------
- Implement (json) annotations in its main program

0.4
---------

- Having a literal in a grammar leads to very confusing errors. Should be handled better.
- Add literalRule which allows for case senstive matching or not
- Should be able to set root based on name 'root', if there is no root specified the first rule will be chosen

- include 'inclusive' property to skip rules eg inclusive find +-> -->, inclusive skip +->> -->>
	  or find, find_including, skip, skip_including  
		or >>, +>>, >>>, +>>>, ->|, |->, ->>|, |->> stop_at, stop_after, skip_to, skip_after	 

- Add repeat count to script '[3]' (min 3, max 3) [..,3] max 3 min 0 [3,..] min 3 max 0 [3,3] 

0.5
---------

- transpile / build c# from rule table output, so there can be a compiled version so we can start building more forgiving ebnf parsers

0.6
---------
- Add Extend() to existing parser 

- add BuildMatcher() class (add function to Graphbuilder?) which takes a tokenizer rule term and will match a string and has
     all common tokens defined
	eg var ip4AddressMatcher = BuildMatcher("byte, '.', byte, '.', byte, '.', byte, '.', optional(':', word)")
	   var ranges = ip4AddressMatcher.Find("#this are the ip addresses 127.9.21.12, 256.12.12.3:8080") => two ranges

0.7
--------
- add optional namespaces to avoid grammar / token name clash 

0.8
---------
- Implement a function console

0.9
----------
- Do All of the following based on ebnf assets, not in the bootstrap
	implement alternatives for short hand (see json_grammar_test.ebnf)
	see if sequence can go without ,
	see if range can go without {}

- implement a Ebnf based EbnfParser and Tokenizer
- add full/short names versions for "not /!" "any /." "optional /?" "zero_or_more /*", "one_or_more /+", "ignore, drop? /~", "transitive /#"
- add full/short names versions for "or /|"
- add alternative for "= / :"
- replace any with _, disallow identifier to start with '_' ?

0.10
---------
- Add interpolatable tokens to errors, eg {token}, {position}, {line}, {column}, {file} etc

alpha (featured complete, buggy, ugly mess)
-------------------------------------------

???
---
- Figure out a way to capture the annotation associated with a reference, so in case of errors we can report the correct line/column
  (probably by adding a property to ReferenceRule which is set during compilation)
