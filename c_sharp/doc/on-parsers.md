On Parsers
===========

The goal of the gg.parse project is to provide a library for a tokenization, parsing and offer an ebnf-like scripting 
tools to make parsing of simple and complex data easy to do, both programatically and via an interpreted scripting 
language. Furthermore this project aims to provide an _easy to understand and use, light-weight_ framework.

gg.parse has no lofty ambitions, it's a c# side project lacking formal rigor and only indirectly draws from the wealth of knowledge of the deep knowledge available in the computer science literature.

If you're looking for a parser which may  qualify as 
"production ready", you might want to consider one of the following:

- [**LLLPG**](https://ecsharp.net/lllpg/) A C# parser generator with EBNF-style syntax.
- [**ANTLR**](https://www.antlr.org/) (ANother Tool for Language Recognition) is a powerful parser generator for reading, processing, executing, or translating structured text or binary files. 
- [**PEG.js / Peggy**](https://github.com/pegjs/pegjs) a JavaScript parser generator with EBNF-like syntax, very popular for web applications
- [**Pest**](https://pest.rs/) A rust parser generator using PEG syntax
- [**Yacc/Bison**](https://en.wikipedia.org/wiki/GNU_Bison)  Classic parser generators using BNF-like syntax (though more verbose than EBNF)
- [**Nearly.js**](https://nearley.js.org/docs/index#) _"nearley is an npm Staff Pick"_.