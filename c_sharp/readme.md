gg.parse 0.1
=======================================================================================================================

```bash
dotnet add package gg.parse.script --version 0.1.0
```

!_Please Note_! the code base is under development and changes frequently. The documentation below may be out of date.

gg.parse is a c# project aiming to provide a library for a tokenization, parsing and offer an ebnf-like scripting 
tools to make parsing of simple and complex data easy to do.

## Table of Contents
- [License](#license)
- [Goals](#goals-use-cases-and-otherwise)
- [Quickstart](#quickstart)
- [Project structure](#project-structure)
- [Rule references](#rule-references)
  - [Data rules](#data-rules)
    - [Match any](#match-any)
    - [Match range](#match-data-range-az)
  - [Meta rules](#meta-rules)
    - [Match count](#match-count--)
    - [Match evaluation](#match-evaluation--abc)
  - [Look ahead rules](#look-ahead-rules)
    - [Match condition](#match-condition--if-)
- [More information](#more-information)

License
-------

[MIT](./license.md)

Goals, use cases and otherwise
------------------------------

The goal of the gg.parse project is to provide a library for a tokenization, parsing and offer an ebnf-like scripting 
tools to make parsing of simple and complex data easy to do, both programatically and via an interpreted scripting 
language. Furthermore this project aims to provide an _easy to understand and use, light-weight_ framework.

gg.parse is an LL(k) parser. [There are many parsers](./doc/on-parsers.md) out there but this one is, according to the license, ours as well. 

Quickstart
-----------------------------------------------------------------------------------------------------------------------

### Core concepts:

- A _`Rule`_ implements a function to parse data (text) and create one or more _`Annotations`_.
- An _`Annotation`_ describes what data is intended to mean, as expressed by a rule. An annotation describes a specific 
  part of the data by way of a _`Range`_. A range a position in the data and its length. Furthermore an annotation is a 
  tree-node where its children may give further insight in the details of data in question.
- A collection of `Rules` make up a _`Rule Graph`_. A `Rule Graph` can _parse_ data (commonly, but not necessarily 
  text)  and map the data to one or more `Annotations`. Depending on the use case, the collection of `Annotations` 
  can either be used as `Tokens` or an `Abstract Syntax Tree`.


### Extended concepts:

- A set of common rules (literal, sequence, not...) to quickly build tokenizers and parsers. 
- A tokenizer/parser/compiler which can build a tokenizer and or parser based on a high-level ebnf-like script.
- A facade-like class, `gg.parse.script.ParserBuilder`, which combines all of the above in a single convenient class.
- [Pruning](./doc/pruning.md) as first class citizen to generate lean syntax tree.

### Example

Programmatically create a tokenizer to tokenizer (simplified) filenames in a text (see 
`gg.parse.doc.examples.test\CreateFilenameTokenizer.cs`):

```csharp
        public class FilenameTokenizer : CommonTokenizer
        {
            public FilenameTokenizer()
            {
                var letter = OneOf(UpperCaseLetter(), LowerCaseLetter());
                var number = InRange('0', '9');
                var specialCharacters = InSet("_-~()[]{}+=@!#$%&'`.".ToArray());
                var separator = InSet("\\/".ToArray());
                var drive = Sequence("drive", letter, Literal(":"), separator);
                var pathPart = OneOrMore("path_part", OneOf(letter, number, specialCharacters));
                var pathChain = ZeroOrMore("#path_chain", Sequence("#path_chain_part", separator, pathPart));
                var path = Sequence("path", pathPart, pathChain);
                var filename = Sequence("filename", drive, path);
                var findFilename = Skip(filename, failOnEoF: false);

                Root = OneOrMore("#filenames", Sequence("#find_filename", findFilename, filename));
            }
        }

        ...

        var filename = "c:\\users\\text.txt";
        var data = $"find the filename {filename} in this line.";           
        var tokens = new FilenameTokenizer().Tokenize(data);
            
        IsTrue(tokens[0].GetText(data) == filename);

        IsTrue(tokens[0] == "filename");
        IsTrue(tokens[0][0] == "drive");
        IsTrue(tokens[0][1] == "path");
        IsTrue(tokens[0][1][0] == "path_part");
        IsTrue(tokens[0][1][1] == "path_part");
```

Doing the same using a script (see `gg.parse.doc.examples.test\CreateFilenameTokenizer.cs`):

```csharp

    // note this can also be read from a separate file
    public static readonly string _filenameScript =
      "-r filenames       = +(find_filename, filename);\n" +
      "-a find_filename   = >>> filename;\n" +
      "filename           = drive, path;\n" +
      "drive              = letter, ':', separator;\n" +
      "path               = path_part, *(-a separator, path_part);\n" +
      "path_part          = +(letter | number | special_character);\n" +
      "letter             = {'a'..'z'} | {'A'..'Z'};\n" +
      "number             = {'0'..'9'};\n" +
      "separator          = {'\\\\/'};\n" +
      "special_character  = {\"_-~()[]{}+=@!#$%&`.'\"};\n";

    ...

    var filename = "c:\\users\\text.txt";
    var data = $"find the filename {filename} in this line.";
    var tokens = new ParserBuilder().From(_filenameScript).Tokenize(data);

    IsTrue(tokens[0].GetText(data) == filename);

    IsTrue(tokens[0] == "filename");
    IsTrue(tokens[0][0] == "drive");
    IsTrue(tokens[0][1] == "path");
    IsTrue(tokens[0][1][0] == "path_part");
    IsTrue(tokens[0][1][1] == "path_part");
```

Project structure
-----------------

The project consists of 3 main topics:

1. Core: Core classes (eg IRule, RuleGraph and Annotation) as well as the basic rules (eg Sequence, Data matchers)
2. Script: Everything related to the scripting framework: parsers, tokenizers and the compiler.
3. Examples: Various examples to demonstrate (and test) the framework.

Each of these main topics has each own corresponding test project.
  

Rule References
---------------

### Data rules
#### [Match Any](./doc/match-any-data.md) (.)
#### [Match Data Range](./doc/match-data-range.md) ({'a'..'z'})
#### [Match Data Sequence](./doc/match-data-sequence.md) ('abc')

### Meta rules
#### [Match Evaluation](./doc/match-evaluation.md)  (a/b/c)
#### [Match Count](./doc/match-count.md) (*,+, ?)

### Look-ahead rules
#### [Match Condition](./doc/match-condition.md)  (if ...)


More information
----------------

[Extending parse script](./doc/extending_parse_script.md) steps required to add a new rule to the script.

[Pruning](./doc/pruning.md) details on how to keep your syntax tree lean.

[To do list](./doc/todo.md) a list of all planned or unplanned tasks.

