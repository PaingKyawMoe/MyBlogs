using System.Text.RegularExpressions;

namespace MyBlogs.Helpers
{
    public static class RemoveHtmlTagHelper
    {
        public static string RemoveHtmlTags(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // 1. Remove HTML tags
            // 2. Decode HTML entities (like &nbsp; or &amp;) to plain text
            string decoded = System.Net.WebUtility.HtmlDecode(input);
            return Regex.Replace(decoded, "<.*?>", string.Empty);
        }
    }
}