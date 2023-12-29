using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Parser
{
    public static class UnityLoader
    {
        public static List<string> GetScenePaths(string projectPath)
        {
            projectPath = Path.Combine(projectPath, "Assets/Scenes");
            if (!Directory.Exists(projectPath))
            {
                throw new DirectoryNotFoundException("Invalid project structure: " + projectPath +
                                                     " should contain /Assets/Scenes directory");
            }

            return GetNestedPaths(projectPath, ".unity");
        }
        public static List<string> GetScriptPaths(string projectPath)
        {
            projectPath = Path.Combine(projectPath, "Assets/Scripts");
            if (!Directory.Exists(projectPath))
            {
                throw new DirectoryNotFoundException("Invalid project structure: " + projectPath +
                                                     " should contain /Assets/Scripts directory");
            }

            return GetNestedPaths(projectPath, ".cs.meta");
        }
        
        private static List<String> GetNestedPaths(string path, string extension)
        {
            List<string> paths = Directory.GetFiles(path).Where(file => file.EndsWith(extension)).ToList();
            
            var subdirectoryEntries = Directory.GetDirectories(path);
            foreach (var subdirectory in subdirectoryEntries)
            {
                var subDirResults = GetNestedPaths(subdirectory, extension);
                paths.AddRange(subDirResults);
            }

            return paths;
        }
    }
}