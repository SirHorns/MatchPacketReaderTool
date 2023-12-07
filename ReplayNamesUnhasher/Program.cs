using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Program
{
    static class Program
    {
        static Dictionary<long, string> NameHashes = new Dictionary<long, string>();
        static JArray Replay;
        static int i, j, k;

        static void Main(string[] inputPath)
        {
            string path;
            if (inputPath.Length > 0)
            {
                path = inputPath[0];
            }
            else
            {
                Console.WriteLine("Invalid Path!\nPlease drag and drop your replay file into the executable\nPress Enter to close this window...");
                Console.Read();
                return;
            }
            
            try
            {
                Console.WriteLine("Loading replay file...\nThis might take a while.");
                Replay = JArray.Parse(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Loading Hash Map...");
            LoadHashes();

            while (NameHashes.Count == 0)
            {
                Console.WriteLine("Content folder not found!\nPlease, insert path to Content the folder manually:");
                path = Console.ReadLine();
                Regex rgx = new Regex("[\"]");
                path = rgx.Replace(path, "");
                LoadHashes(path);
            }

            Console.WriteLine("Hash Map Loaded!");
            Console.WriteLine("Unhashing...\nThis might take a while");

            ProcessInfo();

            string outputPath = $"{Path.GetFullPath(Path.GetDirectoryName(path))}\\{Path.GetFileNameWithoutExtension(path)}Unhashed.json";
            File.WriteAllText(outputPath, Replay.ToString());
            Replay.Clear();
            GC.Collect();
            Console.WriteLine($"Done!\nYour file is in: {outputPath}\nPress Enter to close this window...");
            Console.Read();
        }

        static void ProcessInfo()
        {
            for (i = 0; i < Replay.Count; i++)
            {
                var packetInfo = Replay[i].SelectToken("Packet").ToArray();

                for (k = 0; k < packetInfo.Count(); k++)
                {
                    ProcessProperty(packetInfo[k] as JProperty);
                }
            };
        }
        static void ProcessProperty(JProperty parent)
        {
            if (parent.Values().Children().Count() > 1)
            {
                foreach (var child in parent.Children().Children())
                {
                    if (child is JProperty pr)
                    {
                        Unhash(pr, parent.Name);
                    }
                    else if (child is JObject obj)
                    {
                        ProcessJObject(obj, parent);
                    }

                    j++;
                }
                j = 0;
            }
            else
            {
                Unhash(parent as JProperty);
            }
        }

        static void ProcessJObject(JObject obj, JProperty parent)
        {
            foreach (var child in obj.Children())
            {
                if (child is JObject)
                {
                    ProcessJObject(obj, parent);
                }
                else if (child is JProperty jpro)
                {
                    Unhash(jpro, parent.Name);
                }
            }
        }

        public static void LoadHashes(string manualPath = "")
        {
            string contentPath;
            if (string.IsNullOrEmpty(manualPath))
            {
                contentPath = GetContentPath();
            }
            else
            {
                contentPath = manualPath;
            }

            if (contentPath != null && Directory.Exists(contentPath))
            {
                foreach (var file in Directory.GetFiles(contentPath, "*.json", SearchOption.AllDirectories))
                {
                    var jsonFile = JArray.Parse(File.ReadAllText(file));
                    foreach (JObject hash in jsonFile)
                    {
                        if (!NameHashes.ContainsKey(hash.Value<long>("Hash")))
                        {
                            NameHashes.Add(hash.Value<long>("Hash"), hash.Value<string>("Name"));
                        }
                    }
                }
            }
        }

        private static string GetContentPath()
        {
            string result = null;

            var executionDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (executionDirectory != null)
            {
                var directories = Directory.GetDirectories(executionDirectory);
                if (directories != null && directories.Contains($"{executionDirectory}\\Content"))
                {
                    return $"{executionDirectory}\\Content";
                }
            }
            var path = new DirectoryInfo(executionDirectory ?? Directory.GetCurrentDirectory());

            while (result == null)
            {
                if (path == null)
                {
                    break;
                }

                var directory = path.GetDirectories().Where(c => c.Name.Equals("Content")).ToArray();

                if (directory.Length == 1)
                {
                    result = directory[0].FullName;
                }
                else
                {
                    path = path.Parent;
                }
            }

            return result;
        }

        static void Unhash(JProperty token, string parentName = "")
        {
            long key;
            try
            {
                key = token.First.Value<long>();
            }
            catch
            {
                return;
            }

            if (NameHashes.ContainsKey(key))
            {
                //I've never been so ashamed of myself, but it seems to work just fine
                try
                {
                    Replay[i]["Packet"][parentName].ToArray()[j][token.Name] = NameHashes[key];
                }
                catch
                {
                    try
                    {
                        Replay[i]["Packet"][parentName][token.Name] = NameHashes[key];
                    }
                    catch
                    {
                        try
                        {
                            Replay[i]["Packet"][token.Name] = NameHashes[key];
                        }
                        catch
                        {
                            return;
                        }
                    }
                }
                Console.WriteLine($"Unhashed {key} to {NameHashes[key]}!");
            }
        }
    }
}