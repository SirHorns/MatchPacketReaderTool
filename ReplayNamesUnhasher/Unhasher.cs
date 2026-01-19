using Newtonsoft.Json.Linq;
using ReplayNamesUnhasher.Enums;
using ReplayNamesUnhasher.Replications;

namespace ReplayNamesUnhasher;

public partial class Unhasher
{
    private string _filePath;
    private Dictionary<long, string> NameHashes = new();
    private JArray? _replay;
    private int i, j, k;

    private Dictionary<uint, GameObjectTypes> _netIdToTypesMap;
    private Dictionary<uint, ReplicationTypes> _replicationTypes;

    private ReplicationDict ReplicationDict;
    private List<HashPair> Bones;
    private List<HashPair> Characters;
    private List<HashPair> Items;
    private List<HashPair> Scripts;
    private List<HashPair> Spells;
    private List<HashPair> Talents;
    private List<HashPair> Particles1;
    private List<HashPair> Particles2;

    public Unhasher()
    {
        _netIdToTypesMap = new Dictionary<uint, GameObjectTypes>();
        _replicationTypes = new Dictionary<uint, ReplicationTypes>();
        
        ReplicationDict = new ReplicationDict();
        Bones = [];
        Characters = [];
        Items = [];
        Scripts = [];
        Spells = [];
        Talents = [];
        Particles1 = [];
        Particles2 = [];
    }

    public bool Initialize(string manualPath = "")
    {
        Console.WriteLine("Loading Hash Map...");
        var contentPath = string.IsNullOrEmpty(manualPath) ? GetContentPath() : manualPath;

        if (string.IsNullOrEmpty(contentPath) || !Directory.Exists(contentPath))
        {
            return false;
        }

        foreach (var file in Directory.GetFiles(contentPath, "*.json", SearchOption.AllDirectories))
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var json = File.ReadAllText(file);
            var jArray = JArray.Parse(json);
            foreach (var jToken in jArray)
            {
                var pair = jToken.ToObject<HashPair>();
                switch (fileName)
                {
                    case "BonesHashed":
                        Bones.Add(pair);
                        break;
                    case "CharactersHashed":
                        Characters.Add(pair);
                        break;
                    case "ItemsHashed":
                        Items.Add(pair);
                        break;
                    case "ScriptsHashed":
                        Scripts.Add(pair);
                        break;
                    case "SpellsHashed":
                        Spells.Add(pair);
                        break;
                    case "TalentsHashed":
                        Talents.Add(pair);
                        break;
                    case "ParticlesHashedCabeca143":
                        Particles1.Add(pair);
                        break;
                    case "ParticlesHashedLizardy":
                        Particles2.Add(pair);
                        break;
                }
                
                var hasher = (JObject)jToken;
                var hash = hasher.Value<long>("Hash");
                if (NameHashes.ContainsKey(hash))
                {
                    continue;
                }

                var name = hasher.Value<string>("Name") ?? "unknown_name";
                NameHashes.Add(hash, name);
            }
        }

        ReplicationDict.Initialize();
        
        Console.WriteLine("Hash Map Loaded!");
        return true;
    }

    public void Reset()
    {
        _filePath = "";
        NameHashes.Clear();
        _replay?.Clear();
        i = 0;
        j = 0;
        k = 0;
        _netIdToTypesMap.Clear();
        _replicationTypes.Clear();
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
}
    