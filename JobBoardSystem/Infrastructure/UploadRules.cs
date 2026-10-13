using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace JobBoardSystem.Infrastructure
{
    public static class UploadRules
    {
        public static int MaxUploadBytes
        {
            get
            {
                int bytes;
                return int.TryParse(ConfigurationManager.AppSettings["MaxUploadBytes"], out bytes)
                    ? bytes
                    : 5 * 1024 * 1024;
            }
        }

        public static string Validate(HttpPostedFileBase file, string extensionsKey)
        {
            if (file == null || file.ContentLength == 0)
            {
                return "Please choose a file.";
            }

            if (file.ContentLength > MaxUploadBytes)
            {
                return "The file exceeds the configured upload size.";
            }

            var allowed = (ConfigurationManager.AppSettings[extensionsKey] ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim().ToLowerInvariant())
                .ToList();

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension) || !allowed.Contains(extension.ToLowerInvariant()))
            {
                return "This file type is not allowed.";
            }

            return null;
        }
    }
}
