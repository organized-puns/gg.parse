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