namespace MyBlogs.Helpers
{
    public static class UrlHelper
    {
        public static string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title)) return "";
            string slug = title.ToLower().Trim();
            // Remove special characters
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            // Replace spaces with hyphens
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-").Trim('-');
            return slug;
        }
    }
}