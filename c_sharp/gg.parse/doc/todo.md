TO DO
=====

implement Compiler
	test not function

implement a EbnfParserParser :)

shoudl the default production of an option in rule = *(a|b|c) be transitive and not none ?
	(note) in the compiler any sequence, option, !, *...* produces an annotation

Do All of the following based on ebnf assets, not in the bootstrap
	implement alternatives for short hand
	see if sequence can go without ,
	see if range can go without {}

Figure out how to do error handling, goals
	- don't interfere with happy path
	- maybe exception like approach
	=> recovery


infinite overflow parsing guard


Make parser a bit more human friendly to read (see the json example)  
	- WONT happen it gets too complex, wait until ebnf is implemeted
	- May happen ? EbnfTokenizerParser looks a lot cleaner



create token style annotation parser / compiler
