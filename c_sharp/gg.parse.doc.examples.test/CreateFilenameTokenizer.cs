// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

using gg.parse.core;
using gg.parse.script;
using gg.parse.script.common;

using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace gg.parse.doc.examples.test
{
    [TestClass]
    public class CreateFilenameTokenizer
    {
        // programmatically create a tokenizer
        public class FilenameTokenizer : CommonTokenizer
        {
            public FilenameTokenizer()
            {
                // note this is a simplified filename tokenizer
                var letter = OneOf(UpperCaseLetter(), LowerCaseLetter());
                var number = InRange('0', '9');
                var specialCharacters = InSet("_-~()[]{}+=@!#$%&'`.".ToArray());
                var separator = InSet([.. "\\/"]);
                var drive = Sequence("drive", letter, Literal(":"), separator);
                var pathPart = OneOrMore("path_part", OneOf(letter, number, specialCharacters));
                var pathChain = ZeroOrMore("-r path_chain", Sequence("-r path_chain_part", separator, pathPart));
                var path = Sequence("path", pathPart, pathChain);
                var filename = Sequence("filename", drive, path);
                var findFilename = Skip(filename, failOnEoF: false);

                Root = OneOrMore("-r filenames", Sequence("-r find_filename", findFilename, filename));
            }
        }

        [TestMethod]
        public void FindSingleFilenameUsingFilenameTokenizer()
        {
            var filename = "c:\\users\\text.txt";
            var data = $"find the filename {filename} in this line.";           
            var tokens = new FilenameTokenizer().Tokenize(data);

            IsTrue(tokens && tokens.Count == 1);
            
            IsTrue(tokens[0].GetText(data) == filename);

            IsTrue(tokens[0] == "filename");
            IsTrue(tokens[0][0] == "drive");
            IsTrue(tokens[0][1] == "path");
            IsTrue(tokens[0][1][0] == "path_part");
            IsTrue(tokens[0][1][1] == "path_part");

            IsTrue(tokens[0][1][1].GetText(data) == "text.txt");
        }

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

        [TestMethod]
        public void FindSingleFilenameUsingScript()
        {
            // create a tokenizer from script
            var filename = "c:\\users\\text.txt";
            var data = $"find the filename {filename} in this line.";
            var tokens = new ParserBuilder().From(_filenameScript).Tokenize(data);

            IsTrue(tokens && tokens.Count == 1);

            IsTrue(tokens[0].GetText(data) == filename);

            IsTrue(tokens[0] == "filename");
            IsTrue(tokens[0][0] == "drive");
            IsTrue(tokens[0][1] == "path");
            IsTrue(tokens[0][1][0] == "path_part");
            IsTrue(tokens[0][1][1] == "path_part");

            IsTrue(tokens[0][1][1].GetText(data) == "text.txt");
        }
    }
}
