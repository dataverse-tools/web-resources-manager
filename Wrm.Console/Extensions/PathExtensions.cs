using System.Collections.Generic;
using System.IO;
using Wrm.Model.Enums;


namespace Wrm.ConsoleApp.Extensions
{
    public static class PathExtensions
    {
        private static readonly Dictionary<WebResourceType, string> _typeToExtensionMap = new Dictionary<WebResourceType, string>
        {
            [WebResourceType.Css] = ".css",
            [WebResourceType.Gif] = ".gif",
            [WebResourceType.Html] = ".html",
            [WebResourceType.Ico] = ".ico",
            [WebResourceType.Jpg] = ".jpg",
            [WebResourceType.Png] = ".png",
            [WebResourceType.Script] = ".js",
            [WebResourceType.Xap] = ".xap",
            [WebResourceType.Xml] = ".xml",
            [WebResourceType.Xsl] = ".xsl"
        };

        private static readonly Dictionary<string, WebResourceType> _extensionToTypeMap = new Dictionary<string, WebResourceType>
        {
            [".css"] = WebResourceType.Css,
            [".gif"] = WebResourceType.Gif,
            [".htm"] = WebResourceType.Html,
            [".html"] = WebResourceType.Html,
            [".ico"] = WebResourceType.Ico,
            [".svg"] = WebResourceType.Ico,
            [".jpg"] = WebResourceType.Jpg,
            [".jpeg"] = WebResourceType.Jpg,
            [".png"] = WebResourceType.Png,
            [".js"] = WebResourceType.Script,
            [".xap"] = WebResourceType.Xap,
            [".xml"] = WebResourceType.Xml,
            [".resx"] = WebResourceType.Xml,
            [".xsl"] = WebResourceType.Xsl,
            [".xslt"] = WebResourceType.Xsl
        };

        public static string NormalizePath(this string path, string root, WebResourceType? type)
        {
            var filePath = path
                .Replace('/', Path.DirectorySeparatorChar)
                .ToLower();

            var fullPath = Path.Combine(root, filePath);

            if (Path.HasExtension(fullPath) || type == null)
            {
                return fullPath;
            }

            return Path.ChangeExtension(fullPath, _typeToExtensionMap[type.Value]);
        }

        public static WebResourceType? GetWebResourceType(this string path)
        {
            var ext = Path.GetExtension(path);

            if (string.IsNullOrEmpty(ext))
            {
                return null;
            }

            ext = ext.ToLower();

            if (!_extensionToTypeMap.ContainsKey(ext))
            {
                return null;
            }

            return _extensionToTypeMap[ext];
        }

        public static string GetDisplayName(this string name)
        {
            var idx = name.IndexOf('/');
            if (idx >= 0)
            {
                return name.Substring(name.IndexOf('/') + 1);
            }

            idx = name.IndexOf('_');
            if (idx >= 0)
            {
                return name.Substring(name.IndexOf('_') + 1);
            }

            return name;
        }
    }
}
