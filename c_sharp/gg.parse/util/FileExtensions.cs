// SPDX-License-Identifier: MIT
// Copyright (c) Pointless pun

namespace gg.parse.util
{
    public static class FileExtensions
    {
        public static HashSet<string> AddFilePath(this HashSet<string> paths, string fullFileName)
        {
            var path = Path.GetDirectoryName(fullFileName);

            return string.IsNullOrEmpty(path) || paths.Contains(path)
                ? paths 
                : [..paths, path];
        }

        public static string ResolveFile(this string fileName, HashSet<string>? paths = null)
        {
            if (fileName[0] == '"' || fileName[0] == '\'')
            {
                fileName = fileName.Substring(1, fileName.Length - 2);
            }

            if (File.Exists(fileName))
            {
                return Path.GetFullPath(fileName);
            }
            else if (paths != null && paths.Count > 0)
            {
                foreach (var path in paths)
                {
                    var separator = path[^1] == '/' || path[^1] == '\\'
                        ? ""
                        : Path.DirectorySeparatorChar.ToString();

                    if (File.Exists(path + separator + fileName))
                    {
                        return Path.GetFullPath(path + separator + fileName);
                    }
                }
            }

            throw new FileNotFoundException($"Trying to include {fileName} but doesn't seem to exist.");
        }
    }
}
