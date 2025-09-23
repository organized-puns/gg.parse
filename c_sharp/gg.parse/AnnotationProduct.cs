namespace gg.parse
{
    public enum AnnotationProduct
    {
        /// <summary>
        /// Returns an annotation for the matched item.
        /// </summary>
        Annotation,

        /// <summary>
        /// Returns the annotation produced by any child rules.
        /// </summary>
        Transitive,

        /// <summary>
        /// Does not produce an annotation (eg whitespace).
        /// </summary>
        None
    }

    public static class AnnotationProductExtensions
    {
        public static readonly string TransitiveProductPrefix = "#";

        public static readonly string NoProductPrefix = "~";

        public static string GetPrefix(this AnnotationProduct production)
        {
            return production switch
            {
                AnnotationProduct.Annotation => string.Empty,
                AnnotationProduct.Transitive => TransitiveProductPrefix,
                AnnotationProduct.None => NoProductPrefix,
                _ => throw new NotImplementedException(),
            };
        }


        public static (string outputName, AnnotationProduct product) SplitNameAndProduct(this string name)
        {
            var product = AnnotationProduct.Annotation;
            var start = name.IndexOf(TransitiveProductPrefix);
            var length = 0;

            if (start == 0)
            {
                product = AnnotationProduct.Transitive;
                length = TransitiveProductPrefix.Length;
            }
            else if ((start = name.IndexOf(NoProductPrefix)) == 0)
            {
                product = AnnotationProduct.None;
                length = NoProductPrefix.Length;
            }

            // take the substring of the name minus the annotation product prefix
            return (name.Substring(Math.Max(0, start) + length).Trim(), product);
        }


        /// Note: this will only return true because of the current assumption that the product character
        /// will always start at 0 and defaults to AnnotationProduct.Annotation. Should this change in the future
        /// we can more easily revert.
        public static bool TryGetProduct(this string name, out AnnotationProduct product, out int start, out int length)
        {
            product = AnnotationProduct.Annotation;
            length = 0;

            start = name.IndexOf(TransitiveProductPrefix);

            if (start == 0)
            {
                product = AnnotationProduct.Transitive;
                length = TransitiveProductPrefix.Length;
                return true;
            }

            start = name.IndexOf(NoProductPrefix);

            if (start == 0)
            {
                product = AnnotationProduct.None;
                length = NoProductPrefix.Length;

                return true;
            }

            start = 0;

            return true;
        }

    }
}
