// ----------------------------------------------------------------------------------
// <copyright file="Functions.cs" company="NMemory Team">
//     Copyright (C) 2012-2014 NMemory Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// ----------------------------------------------------------------------------------

namespace NMemory
{
    using System.Text.RegularExpressions;

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
