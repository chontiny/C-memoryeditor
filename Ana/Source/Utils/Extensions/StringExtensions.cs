namespace Ana.Source.Utils.Extensions
{
    using System;
    using System.Linq;
    internal static class StringExtensions
    {
        public static String RemoveSuffixes(this String str, params String[] suffixes)
        {
            if (suffixes == null)
            {
                return str;
            }

            String suffix = String.Empty;
            foreach (String nextSuffix in suffixes)
            {
                if (str.EndsWith(nextSuffix))
                {
                    suffix = nextSuffix;
                    break;
                }
            }

            if (String.IsNullOrEmpty(suffix))
            {
                return str;
            }

            return str.Substring(0, str.Length - suffix.Length);
        }

        public static String TrimWhiteSpace(this String input)
        {
            return new String(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }
    }
    //// End class
}
//// End namespace