using System;
using System.IO;

namespace Parser
{
    public static class UnityInfoDump
    {
        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("2 arguments expected: path to the Unity project and output folder");
                Environment.Exit(0);
            }
            var path = args[0];
            var outputPath = args[1];
            try
            {
                var scripts = UnityLoader.GetScriptPaths(path);
                var scenes = UnityLoader.GetScenePaths(path);
                var parser = new Helper();
                foreach (var scene in scenes)
                {
                    parser.DumpSceneHierarchy(scene, outputPath);
                }
                
                parser.DumpUnusedScripts(path, scripts, outputPath);

            }
            catch (DirectoryNotFoundException exception)
            {
                Console.WriteLine(exception.Message);
            }
            catch (FormatException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
}
