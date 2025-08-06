TO DO
=====

(re)implement a EbnfTokenParser to use the compiler

implement a EbnfParserParser 

Do All of the following based on ebnf assets, not in the bootstrap
	implement alternatives for short hand
	see if sequence can go without ,
	see if range can go without {}

infinite overflow parsing guard

Make parser a bit more human friendly to read (see the json example)  
	- WONT happen it gets too complex, wait until ebnf is implemeted
	- May happen ? EbnfTokenizerParser looks a lot cleaner

create token style annotation parser / compiler


NOTES / DECISISONS
=================

shoudl the default production of an option in rule = *(a|b|c) be transitive and not none ?
	(note) in the compiler any sequence, option, !, *...* produces an annotation, lit, range ... create void


Figure out how to do error handling, goals
	- don't interfere with happy path
	- maybe exception like approach
	=> recovery


