using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ModTools
{
    public static class ScriptCompiler
    {

        private static string workspacePath;
        private static string sourcesPath;
        private static string dllsPath;

        private static readonly string[] gameAssemblies =
        {
            "Assembly-CSharp.dll",
            "UnityEngine.dll"
        };

        static ScriptCompiler()
        {
            var tempPath = Application.temporaryCachePath;
            workspacePath = Path.Combine(tempPath, "ModTools");
            if (!Directory.Exists(workspacePath))
            {
                Directory.CreateDirectory(workspacePath);
            }

            ClearFolder(workspacePath);

            sourcesPath = Path.Combine(workspacePath, "src");
            Directory.CreateDirectory(sourcesPath);

            dllsPath = Path.Combine(workspacePath, "dll");
            Directory.CreateDirectory(dllsPath);
        }

        public static bool RunSource(List<ScriptEditorFile> sources, out string errorMessage, out IModEntryPoint modInstance)
        {
            modInstance = null;

            string dllPath;
            if (CompileSource(sources, out dllPath))
            {
                var assembly = Assembly.LoadFile(dllPath);

                if (assembly == null)
                {
                    errorMessage = "Failed to load assembly!";
                    return false;
                }

                Type entryPointType = null;
                foreach (Type type in assembly.GetTypes())
                {
                    if (typeof(IModEntryPoint).IsAssignableFrom(type))
                    {
                        entryPointType = type;
                        break;
                    }
                }

                if (entryPointType == null)
                {
                    errorMessage = "Failed to find any class that implements IModEntryPoint!";
                    return false;
                }

                modInstance = Activator.CreateInstance(entryPointType) as IModEntryPoint;
                if (modInstance == null)
                {
                    errorMessage = "Failed to create an instance of the IModEntryPoint class!";
                    return false;
                }
            }
            else
            {
                errorMessage = "Failed to compile the source!";
                return false;
            }

            errorMessage = "OK!";
            return true;
        }

        public static bool CompileSource(List<ScriptEditorFile> sources, out string dllPath)
        {
            var name = String.Format("tmp_{0}", Random.Range(0, int.MaxValue));

            var sourcePath = Path.Combine(sourcesPath, name);

            Directory.CreateDirectory(sourcePath);

            var outputPath = Path.Combine(dllsPath, name);
            Directory.CreateDirectory(outputPath);

            foreach (var file in sources)
            {
                var sourceFilePath = Path.Combine(sourcePath, Path.GetFileName(file.path));
                File.WriteAllText(sourceFilePath, file.source);
            }

            dllPath = Path.Combine(outputPath, Path.GetFileName(outputPath) + ".dll");

			// TODO: this was a feature from Cities Skylines - need to find replacement
            //var modToolsAssembly = FileUtil.FindPluginPath(typeof(Mod));
            //var additionalAssemblies = gameAssemblies.Concat(new[]{modToolsAssembly}).ToArray();

            //PluginManager.CompileSourceInFolder(sourcePath, outputPath, additionalAssemblies);
            return File.Exists(dllPath);
        }

        private static void ClearFolder(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);

            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                ClearFolder(dir.FullName);
                dir.Delete();
            }
        }

    }

}
