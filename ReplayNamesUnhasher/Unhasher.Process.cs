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

        if (TryGetUnhashedValue(key, out var unhashedValue))
        {
            return;
        }

        //I've never been so ashamed of myself, but it seems to work just fine
        try
        {
            _replay[i]["Packet"][parentName].ToArray()[j][token.Name] = unhashedValue;
        }
        catch
        {
            try
            {
                _replay[i]["Packet"][parentName][token.Name] = unhashedValue;
            }
            catch
            {
                try
                {
                    _replay[i]["Packet"][token.Name] = unhashedValue;
                }
                catch
                {
                    return;
                }
            }
        }
        Console.WriteLine($"Unhashed {key} to {unhashedValue}!");
    }

    private object? UnhashPacket(BasePacket packet)
    {
        object? result = null;
        JObject job;
        //var json = JsonConvert.SerializeObject(owner, Formatting.Indented);
        switch (packet)
        {
            case SynchVersionS2C:
                break;
            case NPC_BuffRemoveGroup:
                break;
            case C2S_PlayVOCommand:
                break;
            case NPC_BuffAddGroup:
                break;
            case S2C_SetSpellData:
                break;
            case NPC_BuffRemove2:
                break;
            case NPC_BuffAdd2:
                break;
            case S2C_PlayContextualEmote:
                break;
            case S2C_NeutralMinionTimerUpdate:
                break;
            case S2C_NotifyContextualSituation:
                break;
            case FX_Create_Group:
                break;
            case NPC_CastSpellAns:
                break;
            case MissileReplication:
                break;
            case AvatarInfo_Server avatarInfoServer:
                job = JObject.FromObject(avatarInfoServer);
                
                /*var summoners = job["SummonerIDs"].ToArray();
                for (int l = 0; l < 2; l++)
                {
                    var hash = (uint)summoners[l];
                    var unhashed = NameHashes[hash];
                    summoners[l] = unhashed;
                }
     
                summoners = job["SummonerIDs2"].ToArray();
                for (int l = 0; l < 2; l++)
                {
                    var hash = (uint)summoners[l];
                    var unhashed = NameHashes[hash];
                    summoners[l] = unhashed;
                }
                */
                
                var talents = job["Talents"].ToArray();
                foreach (var talent in talents)
                {
                    var hash = (uint)talent["Hash"];
                    if(TryGetUnhashedValue(hash, out var unhashed))
                    {
                        talent["Hash"] = unhashed;
                    }
                }
                break;
        }

        return result;
    }
    
    [Flags]
    public enum ReplicationType
    {
        CLIENT_ONLY_REP_DATA = 0x1,
        LOCAL_REP_DATA1 = 0x2,
        LOCAL_REP_DATA2 = 0x4,
        MAP_REP_DATA = 0x8,
        ONVISIBLE_REP_DATA = 0x10,
        GLOBAL_REP_DATA = 0x20,
    }
    
    private object UnhashOnReplication(OnReplication replication)
    {
            var syncId = replication.SyncID;
            //Console.WriteLine($"[{syncId}]");
            foreach (var rd in replication.ReplicationData)
            {
                var unitNetID = rd.UnitNetID;
                var data = rd.Data;

                if (data[0].Item1 == 0)
                {
                    continue;
                }
                if (data[0].Item1 > (uint)ReplicationType.GLOBAL_REP_DATA)
                {
                    Console.WriteLine($"Unknown replication type: {data[0].Item1}");
                }
                
                var type = (ReplicationType)data[0].Item1;
                //Console.WriteLine($"[ID: {unitNetID} / Type: {type}]");
                //continue;
                
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