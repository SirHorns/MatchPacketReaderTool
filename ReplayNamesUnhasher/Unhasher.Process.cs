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
        switch (packet)
        {
            case SynchVersionS2C:
            case NPC_BuffRemoveGroup:
            case C2S_PlayVOCommand:
            case NPC_BuffAddGroup:
            case S2C_SetSpellData:
            case NPC_BuffRemove2:
            case NPC_BuffAdd2:
            case S2C_PlayContextualEmote:
            case S2C_NeutralMinionTimerUpdate:
            case S2C_NotifyContextualSituation:
            case FX_Create_Group:
            case NPC_CastSpellAns:
            case MissileReplication:
            case AvatarInfo_Server:
            case OnReplication:
                break;
            default:
                return null;
        }
        
        object? result = null;
        var job = JObject.FromObject(packet);
        long hash;
        string value;
        //var json = JsonConvert.SerializeObject(owner, Formatting.Indented);
        switch (packet)
        {
            case SynchVersionS2C:
                foreach (var playInfo in (JArray)job["PlayerInfo"])
                {
                    hash = long.Parse(playInfo["SummonorSpell1"].ToString());
                    if (TryGetUnhashedValue(hash, out value))
                    {
                        playInfo["Summonor1"] = value;
                    }
                    hash = long.Parse(playInfo["SummonorSpell2"].ToString());
                    if (TryGetUnhashedValue(hash, out value))
                    {
                        playInfo["Summonor2"] = value;
                    }
                }
                break;
            case NPC_BuffAdd2:
                hash = long.Parse(job["BuffNameHash"].ToString());
                if (TryGetUnhashedValue(hash, out value))
                {
                    job["BuffName"] = value;
                }
                hash = long.Parse(job["PackageHash"].ToString());
                if (TryGetUnhashedValue(hash, out value))
                {
                    job["Package"] = value;
                }
                break;
            case NPC_BuffRemove2:
                hash = long.Parse(job["BuffNameHash"].ToString());
                if (TryGetUnhashedValue(hash, out value))
                {
                    job["BuffName"] = value;
                }
                break;
            case NPC_BuffAddGroup:
                hash = long.Parse(job["BuffNameHash"].ToString());
                if (TryGetUnhashedValue(hash, out value))
                {
                    job["BuffName"] = value;
                }
                hash = long.Parse(job["PackageHash"].ToString());
                if (TryGetUnhashedValue(hash, out value))
                {
                    job["Package"] = value;
                }
                break;
            case NPC_BuffRemoveGroup:
                hash = long.Parse(job["BuffNameHash"].ToString());
                if (TryGetUnhashedValue(hash, out value))
                {
                    job["BuffName"] = value;
                }
                break;
            case C2S_PlayVOCommand:
                hash = long.Parse(job["EventHash"].ToString());
                if (TryGetUnhashedValue(hash, out value))
                {
                    job["Event"] = value;
                }
                break;
            case S2C_SetSpellData:
                hash = long.Parse(job["HashedSpellName"].ToString());
                if (TryGetUnhashedValue(hash, out value))
                {
                    job["SpellName"] = value;
                }
                break;
            case S2C_PlayContextualEmote:
                hash = long.Parse(job["HashedParam"].ToString());
                if (TryGetUnhashedValue(hash, out value))
                {
                    job["Param"] = value;
                }
                break;
            case S2C_NeutralMinionTimerUpdate:
                hash = long.Parse(job["TypeHash"].ToString());
                if (TryGetUnhashedValue(hash, out value))
                {
                    job["Type"] = value;
                }
                break;
            case S2C_NotifyContextualSituation:
                hash = long.Parse(job["SituationNameHash"].ToString());
                if (TryGetUnhashedValue(hash, out value))
                {
                    job["SituationName"] = value;
                }
                break;
            case FX_Create_Group:
                foreach (var group in (JArray)job["FXCreateGroup"])
                {
                    hash = long.Parse(group["PackageHash"].ToString());
                    if (TryGetUnhashedValue(hash, out value))
                    {
                        group["Package"] = value;
                    }
                    hash = long.Parse(group["EffectNameHash"].ToString());
                    if (TryGetUnhashedValue(hash, out value))
                    {
                        group["EffectName"] = value;
                    }
                    hash = long.Parse(group["TargetBoneNameHash"].ToString());
                    if (TryGetUnhashedValue(hash, out value))
                    {
                        group["TargetBoneName"] = value;
                    }
                    hash = long.Parse(group["BoneNameHash"].ToString());
                    if (TryGetUnhashedValue(hash, out value))
                    {
                        group["BoneName"] = value;
                    }
                }
                break;
            case NPC_CastSpellAns:
            case MissileReplication:
                var castInfo = job["CastInfo"];
                hash = long.Parse(castInfo["SpellHash"].ToString());
                if (TryGetUnhashedValue(hash, out value))
                {
                    castInfo["Spell"] = value;
                }
                hash = long.Parse(castInfo["PackageHash"].ToString());
                if (TryGetUnhashedValue(hash, out value))
                {
                    castInfo["Package"] = value;
                }
                break;
            case AvatarInfo_Server:
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

                var summonerIDs1 = (JArray)job["SummonerIDs"];
                var summonerIDs2 = (JArray)job["SummonerIDs2"];

                var summs1 = new string[summonerIDs1.Count];
                var summs2 = new string[summonerIDs2.Count];


                for (int l = 0; l < summonerIDs1.Count; l++)
                {
                    hash = long.Parse(summonerIDs1[i].ToString());
                    if (TryGetUnhashedValue(hash, out value))
                    {
                        summs1[i] = value;
                    }
                }
                
                for (int l = 0; l < summonerIDs2.Count; l++)
                {
                    hash = long.Parse(summonerIDs2[i].ToString());
                    if (TryGetUnhashedValue(hash, out value))
                    {
                        summs2[i] = value;
                    }
                }

                job["Summoner"] = JArray.FromObject(summs1);
                job["Summoners2"] = JArray.FromObject(summs2);
                
                foreach (var talent in (JArray)job["Talents"])
                {
                    hash = long.Parse(talent["Hash"].ToString());
                    if (TryGetUnhashedValue(hash, out value))
                    {
                        talent["ID"] = value;
                    }
                }
                break;
            case OnReplication onReplication:
                result = UnhashOnReplication(onReplication);
                job = null;
                break;
        }


        switch (result)
        {
            case null when job is null:
                result = packet;
                break;
            case null:
                result = job;
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