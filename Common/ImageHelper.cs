using System.Text.RegularExpressions;

namespace ThriftShop.Common
{
    public static class ImageHelper
    {
        private static readonly Regex Base64ImageRegex = new Regex(@"^data:image\/(?<type>[a-zA-Z0-9\+\-]+);base64,(?<data>.+)$", RegexOptions.Compiled);

        public static string SaveBase64OrUrl(string? input, string? webRootPath = null, string folder = "products")
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var trimmed = input.Trim();
            if (!trimmed.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
                return trimmed;

            try
            {
                var match = Base64ImageRegex.Match(trimmed);
                string ext = ".jpg";
                string base64Content;

                if (match.Success)
                {
                    var type = match.Groups["type"].Value.ToLower();
                    ext = type switch
                    {
                        "png" => ".png",
                        "webp" => ".webp",
                        "gif" => ".gif",
                        "jpeg" or "jpg" => ".jpg",
                        _ => ".jpg"
                    };
                    base64Content = match.Groups["data"].Value;
                }
                else
                {
                    var commaIndex = trimmed.IndexOf(',');
                    if (commaIndex >= 0)
                    {
                        base64Content = trimmed.Substring(commaIndex + 1);
                    }
                    else
                    {
                        return trimmed;
                    }
                }

                var bytes = Convert.FromBase64String(base64Content);
                var root = !string.IsNullOrWhiteSpace(webRootPath) ? webRootPath : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var targetDir = Path.Combine(root, "images", folder);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                var fileName = $"prod_{DateTime.Now.Ticks}_{Guid.NewGuid():N}{ext}";
                var physicalPath = Path.Combine(targetDir, fileName);
                File.WriteAllBytes(physicalPath, bytes);

                return $"/images/{folder}/{fileName}";
            }
            catch
            {
                return trimmed;
            }
        }

        public static async Task<string?> SaveFormFileAsync(IFormFile? file, string? webRootPath = null, string folder = "products")
        {
            if (file == null || file.Length == 0)
                return null;

            try
            {
                var ext = Path.GetExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(ext))
                    ext = ".jpg";

                var root = !string.IsNullOrWhiteSpace(webRootPath) ? webRootPath : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var targetDir = Path.Combine(root, "images", folder);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                var fileName = $"prod_{DateTime.Now.Ticks}_{Guid.NewGuid():N}{ext}";
                var physicalPath = Path.Combine(targetDir, fileName);

                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/images/{folder}/{fileName}";
            }
            catch
            {
                return null;
            }
        }

        public static List<string> ProcessImagesList(IEnumerable<string>? images, string? webRootPath = null, string folder = "products")
        {
            if (images == null)
                return new List<string>();

            var result = new List<string>();
            foreach (var img in images)
            {
                if (string.IsNullOrWhiteSpace(img))
                    continue;

                var saved = SaveBase64OrUrl(img, webRootPath, folder);
                if (!string.IsNullOrWhiteSpace(saved))
                {
                    result.Add(saved);
                }
            }
            return result;
        }
    }
}
