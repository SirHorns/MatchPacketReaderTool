using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace MatchPacketReaderTool.Models;

public class MatchFile
{
    private string _path = "";
    private List<string> _matchJson;
    private readonly JsonSerializer _serializer;
    private Dictionary<String, int> _packtNumbers = new Dictionary<string, int>();

    public string Path
    {
        get => _path;
        private set => _path = value;
    }

    public MatchFile(string path)
    {
        Path = path;
        _serializer = new JsonSerializer();
        LoadJson(Path);
    }
    
    public List<string> MatchJson
    {
        get => _matchJson;
        set => _matchJson = value;
    }
    
    private async void LoadJson(string path)
    {
        using (StreamReader sr = new StreamReader(path))
        using (JsonReader reader = new JsonTextReader(sr))
        {
            try
            {
                MatchJson = await Task.Run(() =>
                {
                    List<string> jsonList = new List<string>();
                        
                    while (reader.Read())
                    { 
                        if (reader.TokenType == JsonToken.StartObject) 
                        {
                            var _obj = _serializer.Deserialize(reader);
                            JObject? _jObject = _obj as JObject;
                            
                            if (_jObject != null)
                            {
                                jsonList.Add(_jObject.ToString());
                                
                                var pName = _jObject["Packet"]["$type"].ToString();

                                if (_packtNumbers.ContainsKey(pName))
                                {
                                    var i = _packtNumbers[pName];
                                    i++;
                                    _packtNumbers[pName] = i;
                                }
                                else
                                {
                                    _packtNumbers.Add(pName,1);
                                }
                            }
                        }
                    }
                        
                    return jsonList;
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}