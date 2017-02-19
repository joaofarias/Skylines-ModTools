using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ModTools
{
    public static class FileUtil
    {

        public static List<string> ListFilesInDirectory(string path, List<string> _filesMustBeNull = null)
        {
            _filesMustBeNull = _filesMustBeNull ?? new List<string>();

            foreach (string file in Directory.GetFiles(path))
            {
                _filesMustBeNull.Add(file);
            }
            return _filesMustBeNull;
        }

		// TODO: do I need this?
        //public static string FindPluginPath(Type type)
        //{
        //    var pluginManager = PluginManager.instance;
        //    var plugins = pluginManager.GetPluginsInfo();

        //    foreach (var item in plugins)
        //    {
        //        var instances = item.GetInstances<IUserMod>();
        //        var instance = instances.FirstOrDefault();
        //        if (instance != null && instance.GetType() != type)
        //        {
        //            continue;
        //        }
        //        foreach (var file in Directory.GetFiles(item.modPath))
        //        {
        //            if (Path.GetExtension(file) == ".dll")
        //            {
        //                return file;
        //            }
        //        }
        //    }
        //    throw new Exception("Failed to find assembly!");
        //}

        public static string LegalizeFileName(this string illegal)
        {
            var regexSearch = new string(Path.GetInvalidFileNameChars()/* + new string(Path.GetInvalidPathChars()*/);
            var r = new Regex($"[{Regex.Escape(regexSearch)}]");
            return r.Replace(illegal, "_");
        }
    }

}
