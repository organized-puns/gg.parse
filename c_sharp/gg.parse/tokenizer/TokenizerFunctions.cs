namespace gg.parse.tokenizer
{
    public static class TokenizerFunctions
    {       
        public static Range? AnyCharacter(string text, int start, int minLength, int maxLength)
        {
            var charactersLeft = text.Length - start;

            if (charactersLeft >= minLength)
            {
                var charactersRead = maxLength <= 0
                        ? charactersLeft
                        : Math.Min(charactersLeft, maxLength);

                return new(start, charactersRead);
            }

            return null;
        }

        public static Range? InCharacterRange(string text, int start, char rangeMin, char rangeMax)
        {
            if (start < text.Length
                && (text[start] >= rangeMin && text[start] <= rangeMax))
            {
                return new(start, 1);
            }

            return null;
        }

        public static Range? InCharacterSet(string text, int start, string set)
        {
            if (start < text.Length && set.Contains(text[start]))
            {
                return new(start, 1);
            }
            
            return null;
        }

        public static Range? MatchCount(string text, int start, TokenFunction function, int min = 0, int max = 0)
        {
            int count = 0;
            int index = start;

            while (index < text.Length && (max <= 0 || count < max))
            {
                var result = function.Parse(text, index);
                if (result == null || result.Category == AnnotationCategory.Error)
                {
                    break;
                }
                count++;
                index += result.Range.Length;
            }

            if (min <= 0 || count >= min)
            {
                return new Range(start, index - start);
            }

            return null;
        }

        public static Range? MatchNot(string text, int start, TokenFunction function)
        {
            var result = function.Parse(text, start);

            if (result == null)
            {
                return new Range(start, 0);
            }

            return null;
        }

        public static Range? Literal(string text, int start, string literal)
        {
            if (text.Length >= start + literal.Length)
            {
                for (var i = 0; i < literal.Length; i++)
                {
                    if (text[start+ i] != literal[i])
                    {
                        return null;
                    }
                }

                return new(start, literal.Length);
            }

            return null;
        }

        public static Range? MatchOneOf(string text, int start, params TokenFunction[] options)
        {
            foreach (var option in options)
            {
                var result = option.Parse(text, start);
                if (result != null)
                {
                    return result.Range;
                }
            }
            return null;
        }

        public static Range? MatchSequence(string text, int start, params TokenFunction[] sequence)
        {
            var index = start;
            
            foreach (var function in sequence)
            {
                var result = function.Parse(text, index);
                if (result == null)
                {
                    return null;
                }

                index += result.Range.Length;
            }

            return new(start, index - start);
        }
    }
}
