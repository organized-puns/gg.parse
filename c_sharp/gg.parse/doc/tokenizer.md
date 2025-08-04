TO DO
=====

Make parser a bit more human friendly to read (see the json example)  
	WONT happen it gets too complex, wait until ebnf is implemeted
Figure out how to do error handling, goals
	- don't interfere with happy path
	- maybe exception like approach
	=> recovery

Figure out to do forward references in ebnf

Start implementing ebnf, tokenizer and parser

digit = '0'..'9'
sign = '+' | '-'
int = ?sign +digit !'.'
float = ?sign +digit '.' +digit ?('e'|'E' ?sign +digit)
string = '"' *any '"'
scope_start = '{'
scope_end = '}'



key_value_pair {
	rule  = key key_value_separator value
	error = key !(key_value_separator value) mark_error("failed to ...")
	skip  = '}' | key_value_pair
}

=>
key_value_pair  = (key key_value_separator value) 
				| (key !(key_value_separator value)) mark_error("failed to ...") 
				  skip_until('}' | '{' | key_value_pair) 

key_value_pair  = (key key_value_separator value) 
				// error handling
				| (key !(key_value_separator value)) 
				  error("failed to ...") 
				  skip_until('}' | '{' | key_value_pair) 

sequence = a b c
	if a succeeds but b fails, skip until c, return error
	if a & b succeed but c fails, return error


```
// simple json tokenizer grammar

// tokens:
	float			= sign?, digit+, (dot, digit+)?;
	integer			= sign?, digit+;
	string			= '"', ( err_eof_before_closing_string | err_eoln_before_closing_string | '\"' | !'"')*, '"';
	boolean			= 'true' | 'false';
	null			= 'null';
	object_start	= '{';
	object_end		= '}';
	kv_separator	= ':';
	array_start		= '[';
	array_end		= ']';
	array_separator = ',';

// ignore:
	~whitespace		= ' ' | '\t' | '\n' | '\r';


// helper rules:
	sign  = '+' | '-';						
	digit = {0..9};

// errors:
	err_eof_before_closing_string	= fatal(end_of_file, "end of file encountered while reading a string");
	err_eoln_before_closing_string  = error('\n', "end of line encountered while reading a string");
	
```

This implies we need an implementation of a tokenizer that can handle these rules. 
The tokenizer will need to recognize and extract tokens based on the defined grammar.

rules needed:
 * Literal, eg: 'value' or "value"
 * Optional / Or / OneOf, eg: a | b | c
 * Sequence: eg: a, b, c
 * Group eg: (a, "b", c)
 * MinMax : 
	* between N and M, eg: digit[2,5]
	* zero or one, eg minus?  == minus[0, 1]
	* zero or more, eg minus* == minus[0, *]
	* one or more, eg digit+ == minus[1, *]
    * exactly N, eg digit[3] == digit[3, 3]
 * Range, eg: or {a..z}	
 * Not, eg !digit
 * Eof rule, matches when index >= text.length
 * Fatal rule, throws an exception when matched
 * Error rule, reports an error, the lexer should capture this error and continue processing


parser:

```
	character_range_start = "<";
	character_range_end = ">";
	elipsis = "..";
	character_range = character_range_start, character, elipsis, character, character_range_start;

	// note, replace "<" with anonymous token _character_range_1 = "<" eventually in a pre-processing step
	// of the parser, so it's easy to write
	character_range = "<", character, "..", character, ">";
	character_set = "{", character+, "}";
	any_character = "any" | "_";

	basic_rules = literal | character_range | character_set | any_character | identifier;  
	group = "(", expression, ")";
	
	match_not = ("!" | "not "), unary;

	unary = basic_rules | group | match_not

	match_sequence = unary, "," unary, ("," unary)*;
	match_one_of = unary, "|", unary, ("|", unary)*;

	match_count = unary, "[", integer, ",", integer, "]";
	match_zero_or_more = unary, "*";
	match_zero_or_one = unary, "?";
	match_one_or_more= unary, "+";
		
	match_rules = match_sequence | match_one_of | match_count | match_zero_or_more | match_zero_or_one | match_one_or_more | match_not

	expression = basic_rules | group | match_rules

	action = "~";
	rule_declaration = action*, identifier;
	rule = rule_declaration, "=", expression, ";"
```

recovery points

example tokenizer

...
kv_separator = ":"
tokens = ... | kv_separator 
unknown_token = error("unknown token", tokens)
#root = *(whitespace | tokens | unknown_token)

	

example parser

err_missing_value		  = skip_until comma | scope_end
err_missing_kv_separator  = skip_until value | comma | scope_end

kv = key kv_separator value
   | key kv_separator err_missing_value 
   | key error "expecting kv_sepator" value | comma | scope_end

json_object = scope_start ?kv_list scope_end
			| scope_start error("failed to parse json object") skip_until(scope_end)
