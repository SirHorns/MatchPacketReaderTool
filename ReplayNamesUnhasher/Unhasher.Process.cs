using LeaguePackets;
using LeaguePackets.Game;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReplayNamesUnhasher.Enums;
using ReplayNamesUnhasher.Replications;

namespace ReplayNamesUnhasher;

public partial class Unhasher
{
    
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

    private void UnhashPacket(BasePacket packet)
    {
    }
    
    private object UnhashOnReplication(OnReplication replication)
    {
            var syncId = replication.SyncID;
            //Console.WriteLine($"[{syncId}]");
            foreach (var rd in replication.ReplicationData)
            {
                var unitNetID = rd.UnitNetID;
                var data = rd.Data;

                var objectType = _netIdToTypesMap.GetValueOrDefault(unitNetID, GameObjectTypes.Unknown);
                var replicationType = _replicationTypes.GetValueOrDefault(unitNetID, ReplicationTypes.Unknown );
                //Console.WriteLine($"[ID: {unitNetID} / Obj: {GameObjectTypes.Unknown} / Repl: {replicationType}]");

                Replicant? owner = null;
                
                switch (replicationType)
                {
                    case ReplicationTypes.Unknown:
                    case ReplicationTypes.Barracks:
                    case ReplicationTypes.BarracksDampener:
                        continue;
                    case ReplicationTypes.Prop:
                        break;
                    case ReplicationTypes.Hero:
                        break;
                    case ReplicationTypes.HQ:
                        owner = new HQ();
                        break;
                    case ReplicationTypes.Minion:
                        owner = new Minion();
                        break;
                    case ReplicationTypes.Turret:
                        break;
                    default:
                        break;
                }
                for (byte pid = 0; pid < 6; pid++)
                {
                    var unknown = data[pid].Item1;
                    if (unknown == 0)
                    {
                        continue;
                    }

                    var index = 0;
                    var bytes = data[pid].Item2;

                    for (byte sid = 0; sid < 32; sid++)
                    {
                        if (((unknown >> sid) & 1) == 0)
                        {
                            continue; // not sure what the large unknown Tuple uints mean
                        }
                        
                        var replicationDataType = ReplicationDict.GetReplicationValueType((int)replicationType, pid, sid);
                        object? val = null; 
                        try
                        {
                            val = ReplicationDict.GetValue(replicationDataType, bytes, ref index);
                        }
                        catch (Exception e)
                        {
                            //Console.WriteLine($"Failed to find replication {replicationDataType} for a {replicationType}");
                            DumpState(bytes, index, replicationType, pid, sid, e);
                            break;
                        }

                        if (owner is not null)
                        {
                            owner.SetValue(pid, sid, val);
                        }
                    }
                }

                if (owner is null)
                {
                    continue;
                }

                var json = JsonConvert.SerializeObject(owner, Formatting.Indented);
                Console.WriteLine(json);
            }

            return null;
    }
}