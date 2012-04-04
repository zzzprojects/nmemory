using System.Text.RegularExpressions;

namespace NMemory
{
    public static class Functions
    {
        public static bool Like(string input, string pattern)
        {
            // http://stackoverflow.com/questions/5663655/like-operator-in-linq-to-objects

            // Turn "off" all regular expression related syntax in the pattern string.
            pattern = Regex.Escape(pattern);

            // Replace the SQL LIKE wildcard metacharacters with the equivalent regular expression metacharacters.
            pattern = pattern.Replace("%", ".*?").Replace("_", ".");

            // The previous call to Regex.Escape actually turned off too many metacharacters, i.e. those which 
            // are recognized by both the regular expression engine and the SQL LIKE statement ([...] and [^...]). 
            // Those metacharacters have to be manually unescaped here.
            pattern = pattern.Replace(@"\[", "[").Replace(@"\]", "]").Replace(@"\^", "^");

            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase); 
        }
    }
}
