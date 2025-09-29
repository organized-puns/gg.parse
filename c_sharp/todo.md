Todo (for mvp)
---------------

- Finalize:  
  - Implement json annotation in its main program
  - document match evaluation
  - address all xxx
  - Clean up unit tests and build proper examples
  - Add some more documentation, extend readme.
     
alpha (featured complete, buggy, ugly mess)
-------------------------------------------

- Should be able to set root based on name 'root', if there is no root specified the first rule will be chosen

- Figure out a way to capture the annotation associated with a reference, so in case of errors we can report the correct line/column
  (probably by adding a property to ReferenceRule which is set during compilation)

- Add interpolatable tokens to errors, eg {token}, {position}, {line}, {column}, {file} etc

- include 'inclusive' property to skip rules eg inclusive find +-> -->, inclusive skip +->> -->>
	  or find, find_including, skip, skip_including  
		or >>, +>>, >>>, +>>>, ->|, |->, ->>|, |->> stop_at, stop_after, skip_to, skip_after

- better compile name generation

-  Figure out if we really need all sub rules in the rulegraph id/name. 
   Yes because a = 'foo'; and b = 'foo', 'bar'; should NOT generate two 'foo-rules. a ='foo'; and b = ~'foo'; are different though
   (I'm sure this doesn't work as of yet - it actually does, probably not so much for the compile stage though)   

- Add repeat count to script '[3]' (min 3, max 3) [..,3] max 3 min 0 [3,..] min 3 max 0 [3,3] 

- transpile / build c# from rule table output, so there can be a compiled version so we can start building more forgiving ebnf parsers

- Add FromFile to ParserBuilder. Add Extend() to existing parser 

- add BuildMatcher() class (add function to Graphbuilder?) which takes a tokenizer rule term and will match a string and has
     all common tokens defined
	eg var ip4AddressMatcher = BuildMatcher("byte, '.', byte, '.', byte, '.', byte, '.', optional(':', word)")
	   var ranges = ip4AddressMatcher.Find("#this are the ip addresses 127.9.21.12, 256.12.12.3:8080") => two ranges

- add optional namespaces to avoid grammar / token name clash 
- Implement a function console


- Do All of the following based on ebnf assets, not in the bootstrap
	implement alternatives for short hand (see json_grammar_test.ebnf)
	see if sequence can go without ,
	see if range can go without {}

- implement a Ebnf based EbnfParser and Tokenizer
- add full/short names versions for "not /!" "any /." "optional /?" "zero_or_more /*", "one_or_more /+", "ignore, drop? /~", "transitive /#"
- add full/short names versions for "or /|"
- add alternative for "= / :"
- replace any with _, disallow identifier to start with '_' ?
