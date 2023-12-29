using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Parser
{
    public class Helper
    {
        private readonly HashSet<string> _scripts = new ();

        public void DumpSceneHierarchy(string path, string outputPath)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var outputFile = new StreamWriter(Path.Combine(outputPath, Path.GetFileName(path) + ".dump"));

            var input = new StringReader(File.ReadAllText(path));
            var roots = new List<long>();

            var objectMap = _parseSceneYaml(input, roots);

            Dictionary<long, bool> used = new();
            foreach (var objId in objectMap.Keys)
            {
                used[objId] = false;
            }

            foreach (var root in roots.Where(root => !used[root]))
            {
                used[root] = true;
                objectMap[root].DumpHierarchy(0, used, objectMap, outputFile);
            }

            outputFile.Close();
        }

        private Dictionary<long, GameObject> _parseSceneYaml(StringReader input, List<long> roots)
        {
            var yaml = new YamlStream();
            yaml.Load(input);
            GameObject currentGameObject = null;
            var objects = new List<GameObject>();

            foreach (var document in yaml)
            {
                var currentDocumentId = long.Parse(document.RootNode.Anchor.ToString());
                var mapping = (YamlMappingNode)document.RootNode;
                var node = mapping.Children[0];
                var fields = (YamlMappingNode)mapping.Children[new YamlScalarNode(node.Key.ToString())];
                switch (node.Key.ToString())
                {
                    case "GameObject":
                    {
                        if (currentGameObject != null) objects.Add(currentGameObject);
                        currentGameObject = ParseGameObjectNode(fields);
                        break;
                    }
                    case "Transform":
                    {
                        if (currentGameObject == null) throw new FormatException("Invalid YAML format");
                        ParseAndAddFromSequence("m_Children", fields, currentGameObject.ChildrenIds);
                        currentGameObject.TransformId = currentDocumentId;
                        break;
                    }
                    case "MonoBehaviour": 
                        ParseMonoBehaviourNode(fields); 
                        break;
                    case "SceneRoots":
                    {
                        ParseAndAddFromSequence("m_Roots", fields, roots);
                        break;
                    }
                }
            }
            objects.Add(currentGameObject);
            return GetObjectMapFromList(objects);
        }
        
        private static Dictionary<long, GameObject> GetObjectMapFromList(List<GameObject> objects)
        {
            var result = new Dictionary<long, GameObject>();
            foreach (var obj in objects)
            {
                result[obj.TransformId] = obj;
            }

            return result;
        }
        private static GameObject ParseGameObjectNode(YamlMappingNode items)
        {
            var currentGameObject = new GameObject();
            var item = items[new YamlScalarNode("m_Name")];
            currentGameObject.Name = item.ToString();;
            return currentGameObject;
        }

        private void ParseMonoBehaviourNode(YamlMappingNode items)
        {
            var item = items[new YamlScalarNode("m_Script")];
            var mScript = (YamlMappingNode)item;
            _scripts.Add(mScript.Children[1].Value.ToString());
        }
        private static void ParseAndAddFromSequence(string key, YamlMappingNode items, List<long> roots)
        {
            var item = items[new YamlScalarNode(key)];
            var children = ((YamlSequenceNode)item).Children;
            foreach (YamlMappingNode child in children)
            {
                roots.Add(long.Parse(child.Children[0].Value.ToString()));
            }
        }

        public void DumpUnusedScripts(string root, List<string> paths, string outputPath)
        {
            var outputFile = new StreamWriter(Path.Combine(outputPath, "UnusedScripts.csv"));
            outputFile.WriteLine("Relative Path,GUID");
            var scriptNames = ParseScriptMetas(root, paths);
            foreach (var script in scriptNames)
            {
                if (_scripts.Contains(script.Key)) continue;
                outputFile.WriteLine("{0},{1}", script.Value, script.Key);
                
            }
            outputFile.Close();
        }
        
        private static Dictionary<string, string> ParseScriptMetas(string root, List<string> paths)
        {
            Dictionary<string, string> names = new Dictionary<string, string>();
            var yaml = new YamlStream();
            foreach (var file in paths)
            {
                if (file.EndsWith(".cs.meta"))
                {
                    yaml.Load(new StringReader(File.ReadAllText(file)));
                    var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
                    var node = mapping[new YamlScalarNode("guid")];
                    names[node.ToString()] = file.Substring(root.Length, file.Length - root.Length - 5);
                }
            }
            return names;
        }
        
        private class GameObject
        {
            public long TransformId; 
            public string Name { get; set; }
            public readonly List<long> ChildrenIds = new();


            public void DumpHierarchy(int depth, Dictionary<long, bool> used, Dictionary<long, GameObject> objectMap, StreamWriter outputFile)
            {
                outputFile.WriteLine(new string('-', depth * 2) + Name);
                foreach (long child in ChildrenIds)
                {
                    used[child] = true;
                    objectMap[child].DumpHierarchy(depth + 1, used, objectMap, outputFile);
                }
            }
        }
    }
}