using System;
using System.Linq;

namespace DotEntity.Extensions
{
    public static class StringExtensions
    {
        private static readonly string[] ExcludeFromEnclosure = { "INFORMATION_SCHEMA.TABLES", "1" };

        public static string ToEnclosed(this string str)
        {
            if (string.IsNullOrEmpty(str) || ExcludeFromEnclosure.Contains(str))
                return str;
            var alias = "";
            var actualStr = str;
            if (str.Contains(".")) //a case of aliasing
            {
                var parts = str.Split('.');
                alias = parts[0] + ".";
                actualStr = parts[1];
            }
            return alias + DotEntityDb.Provider.SafeEnclose(actualStr);
        }

        public static string ReplaceFirst(this string text, string search, string replace)
        {
            var pos = text.IndexOf(search, StringComparison.Ordinal);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}