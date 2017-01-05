namespace Ana.Source.Utils.Extensions
{
    using System;

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
    }
    //// End class
}
//// End namespace