using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Serilog;

namespace ClassifyBot
{
    public static class FileSystemInfoExtensions
    {
        public static bool CheckExistsAndReportError(this FileSystemInfo fsi, ILogger L)
        {
            string o = fsi is FileInfo ? "file" : "object";
            if (fsi.Exists)
            {
                return true;
            }
            else
            {
                L.Error("The {0} {1} does not exist.", o, fsi.FullName);
                return false;
            }
        }

    }
}
