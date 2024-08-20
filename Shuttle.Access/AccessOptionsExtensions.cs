using Shuttle.Core.Contract;

namespace Shuttle.Access
{
    public static class AccessOptionsExtensions
    {
        public static string GetUrl(this AccessOptions accessOptions, string relativePath)
        {
            var siteUrl = Guard.AgainstNull(accessOptions).SiteUrl;
            var path = Guard.AgainstNullOrEmptyString(relativePath, nameof(relativePath));

            if (!siteUrl.EndsWith("/"))
            {
                siteUrl += "/";
            }

            if (path.StartsWith("/"))
            {
                path = path[1..];
            }

            return $"{siteUrl}{path}";
        }
    }
}