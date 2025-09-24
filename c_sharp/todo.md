Todo (for mvp)
---------------

- Finalize:  
  - Fix calculator interpreter not raising errors when it should
  - Remove "Function" notion in favor of Rule in names across the project
  - checkout the xxx in public void ResolveReferences()
  - continue improving the rulebody of the scriptparser (too much spagetti) 
  - Implement json annotation in its main program
  - document match evaluation
  - address all xxx
  - Clean up unit tests and build proper examples
  - Add some more documentation, extend readme.
     
alpha (featured complete, buggy, ugly mess)
-------------------------------------------

-  Figure out if we really need all sub rules in the rulegraph id/name. 
   Yes because a = 'foo'; and b = 'foo', 'bar'; should NOT generate two 'foo-rules. a ='foo'; and b = ~'foo'; are different though
   (I'm sure this doesn't work as of yet - it actually does, probably not so much for the compile stage though)   

- Add repeat count to script '[3]' (min 3, max 3) [..,3] max 3 min 0 [3,..] min 3 max 0 [3,3] 

- build c# from rule table output, so there can be a compiled version so we can start building more forgiving ebnf parsers

- add optional namespaces to avoid grammar / token name clash 
- Implement a function console

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
