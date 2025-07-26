using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gg.parse.rules
{
    public class CharacterSetRule : RuleBase
    {
        public static readonly string DefaultName = "CharacterSetRule";

        public string Characters { get; init; } 

        public CharacterSetRule(string characters, string? name = null)
            : base(name ?? DefaultName)
        {
            if (string.IsNullOrEmpty(characters))
            {
                throw new ArgumentException("Characters cannot be null or empty", nameof(characters));
            }
            Characters = characters;
        }

        public override ParseResult Parse(string text, int offset)
        {
            if (offset < text.Length)
            {
                char currentChar = text[offset];

                return new ParseResult
                {
                    ResultCode = Characters.Contains(currentChar) ? ParseResult.Code.Success : ParseResult.Code.Failure,
                    Length = 1,
                    Rule = this,
                    Offset = offset,
                };
            }
            else
            {
                return new ParseResult
                {
                    ResultCode = ParseResult.Code.Failure,
                    Length = 0,
                    Rule = this,
                    Offset = offset,
                };
            }
        }
    }
}
