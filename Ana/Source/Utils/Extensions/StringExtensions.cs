namespace Ana.Source.Utils.Extensions
{
    using System;
    using System.Text;
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

        public static String Replace(this String source, String oldValue, String newValue, StringComparison comparisonType)
        {
            if (source.Length == 0 || oldValue.Length == 0)
            {
                return source;
            }

            StringBuilder result = new StringBuilder();
            Int32 startingPos = 0;
            Int32 nextMatch;

            while ((nextMatch = source.IndexOf(oldValue, startingPos, comparisonType)) > -1)
            {
                result.Append(source, startingPos, nextMatch - startingPos);
                result.Append(newValue);
                startingPos = nextMatch + oldValue.Length;
            }
            result.Append(source, startingPos, source.Length - startingPos);

            return result.ToString();
        }
    }
    //// End class
}
//// End namespace