using Newtonsoft.Json.Linq;

namespace ReplayUnhasher;

public class Unhasher
{
    private string _filePath;
    private Dictionary<long, string> NameHashes = new();
    private JArray _replay;
    private int i, j, k;

    public Unhasher()
    {
        LoadHashes();
    }

    public void OpenJsonFile(string jsonPath)
    {
        try
        {
            Console.WriteLine("Loading replay file...\nThis might take a while.");
            _replay = JArray.Parse(File.ReadAllText(jsonPath));
            _filePath = jsonPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public void OpenJson(string json)
    {
        Console.WriteLine("Loading replay file, this might take a while...");
        _replay = JArray.Parse(json);
    }
    
    public bool LoadHashes(string manualPath = "")
    {
        Console.WriteLine("Loading Hash Map...");
        string contentPath;
        if (string.IsNullOrEmpty(manualPath))
        {
            contentPath = GetContentPath();
        }
        else
        {
            contentPath = manualPath;
        }

        if (string.IsNullOrEmpty(contentPath) || !Directory.Exists(contentPath))
        {
            return false;
        }
        
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
        
        Console.WriteLine("Hash Map Loaded!");
        return true;
    }
    
    public void UnhashReplay()
    {
        Console.WriteLine("Unhashing Replay...");
        for (i = 0; i < _replay.Count; i++)
        {
            var packetInfo = _replay[i].SelectToken("Packet").ToArray();

            for (k = 0; k < packetInfo.Count(); k++)
            {
                Console.WriteLine(k);
                ProcessProperty(packetInfo[k] as JProperty);
            }
        };
        Console.WriteLine("Finished Unhasing Replay!");
    }

    public void WriteJsonToFile()
    {
        string outputPath = $"{Path.GetFullPath(Path.GetDirectoryName(_filePath))}\\{Path.GetFileNameWithoutExtension(_filePath)}_Unhashed.json";
        File.WriteAllText(outputPath, _replay.ToString());
        _replay.Clear();
        GC.Collect();
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
    
    private void ProcessProperty(JProperty parent)
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

    private void ProcessJObject(JObject obj, JProperty parent)
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
    
    private void Unhash(JProperty token, string parentName = "")
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
                _replay[i]["Packet"][parentName].ToArray()[j][token.Name] = NameHashes[key];
            }
            catch
            {
                try
                {
                    _replay[i]["Packet"][parentName][token.Name] = NameHashes[key];
                }
                catch
                {
                    try
                    {
                        _replay[i]["Packet"][token.Name] = NameHashes[key];
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