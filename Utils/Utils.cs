using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotEdgeForEpubWpf.Utils
{
    internal class MyUtils
    {
        public static string HashFromStream(Stream stream)
        {
            var xx128 = new XxHash128(); xx128.Append(stream);
            return BitConverter.ToString(xx128.GetCurrentHash()).Replace("-", "").ToLowerInvariant(); ;
        }
        public static Task<string> HashFromStreamAsync(Stream stream)
        {
            return Task.Run(() => HashFromStream(stream));
        }
        public static string EncodeBase64FromString(string str)
        {
            return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(str));
        }
        public static string DecodeBase64ToString(string str)
        {
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(str));
        }

        public static string PathInFileResovle(string filePath, string relativePath)
        {
            if (relativePath.StartsWith('/'))
            {
                return relativePath.Length > 1 ? relativePath.Substring(1) : String.Empty;
            }
            string? basePath = Path.GetDirectoryName(filePath);
            while (relativePath.StartsWith("../"))
            {
                relativePath = relativePath.Length > 3 ? relativePath.Substring(3) : String.Empty;
                basePath = Path.GetDirectoryName(basePath);
            }
            string fullPath = Path.Combine((basePath ?? ""), relativePath);
            return fullPath;
        }
        //public static bool FileInDictoryWithoutExtension(string directoryPath,string fileName)
        //{
        //    return Directory.GetFiles(directoryPath, fileName + ".*").Length > 0;
        //}
        //public static bool FileExistsWithoutExtension(string filePath)
        //{
        //    return FileInDictoryWithoutExtension(Path.GetDirectoryName(filePath), Path.GetFileName(filePath));
        //}


        public static readonly Dictionary<string, string> MimeMapping = new(StringComparer.OrdinalIgnoreCase)
        {
            {"application/fsharp-script", ".fsx"},
                {"application/msaccess", ".adp"},
                {"application/msword", ".doc"},
                {"application/octet-stream", ".bin"},
                {"application/onenote", ".one"},
                {"application/postscript", ".eps"},
                {"application/step", ".step"},
                {"application/vnd.apple.keynote", ".key"},
                {"application/vnd.apple.numbers", ".numbers"},
                {"application/vnd.apple.pages", ".pages"},
                {"application/vnd.ms-excel", ".xls"},
                {"application/vnd.ms-powerpoint", ".ppt"},
                {"application/vnd.ms-works", ".wks"},
                {"application/vnd.visio", ".vsd"},
                {"application/x-director", ".dir"},
                {"application/x-msdos-program", ".exe"},
                {"application/x-shockwave-flash", ".swf"},
                {"application/x-x509-ca-cert", ".cer"},
                {"application/x-zip-compressed", ".zip"},
                {"application/xhtml+xml", ".xhtml"},
                {"application/x-iwork-keynote-sffkey", ".key"},
                {"application/x-iwork-numbers-sffnumbers", ".numbers"},
                {"application/x-iwork-pages-sffpages", ".pages"},
                {"application/xml", ".xml"}, // anomaly, .xml -> text/xml, but application/xml -> many things, but all are xml, so safest is .xml
                {"application/yaml", ".yaml"},
                {"audio/aac", ".AAC"},
                {"audio/aiff", ".aiff"},
                {"audio/basic", ".snd"},
                {"audio/mid", ".midi"},
                {"audio/mp4", ".m4a"}, // one way mapping only, mime -> ext
                {"audio/ogg", ".ogg"},
                {"audio/ogg; codecs=opus", ".opus"},
                {"audio/wav", ".wav"},
                {"audio/x-m4a", ".m4a"},
                {"audio/x-mpegurl", ".m3u"},
                {"audio/x-pn-realaudio", ".ra"},
                {"audio/x-smd", ".smd"},
                {"image/bmp", ".bmp"},
                {"image/heic", ".heic"},
                {"image/heif", ".heif"},
                {"image/jpeg", ".jpg"},
                {"image/pict", ".pic"},
                {"image/png", ".png"}, // Defined in [RFC-2045], [RFC-2048]
                {"image/x-png", ".png"}, // See https://www.w3.org/TR/PNG/#A-Media-type :"It is recommended that implementations also recognize the media type "image/x-png"."
                {"image/tiff", ".tiff"},
                {"image/x-macpaint", ".mac"},
                {"image/x-quicktime", ".qti"},
                {"message/rfc822", ".eml"},
                {"text/calendar", ".ics"},
                {"text/html", ".html"},
                {"text/plain", ".txt"},
                {"text/scriptlet", ".wsc"},
                {"text/xml", ".xml"},
                {"video/3gpp", ".3gp"},
                {"video/3gpp2", ".3gp2"},
                {"video/mp4", ".mp4"},
                {"video/mpeg", ".mpg"},
                {"video/quicktime", ".mov"},
                {"video/vnd.dlna.mpeg-tts", ".m2t"},
                {"video/x-dv", ".dv"},
                {"video/x-la-asf", ".lsf"},
                {"video/x-ms-asf", ".asf"},
                {"x-world/x-vrml", ".xof"},
        };
    }
}
