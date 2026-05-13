using LeaguePackets;
using LeaguePackets.Game;
using LeaguePackets.Game.Common;
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

    private object? UnhashPacket(BasePacket packet)
    {
        object? result = null;
        JObject job;
        //var json = JsonConvert.SerializeObject(owner, Formatting.Indented);
        switch (packet)
        {
            case SynchVersionS2C synchVersionS2C:
                job = JObject.FromObject(synchVersionS2C);
                foreach (var playerInfo in job["PlayerInfo"])
                {
                    var summoner1 = (uint)playerInfo["SummonorSpell1"];
                    if (NameHashes.TryGetValue(summoner1, out var summoner1Name))
                    {
                        job["SummonorSpell1"] = summoner1Name;
                    }
                    var summoner2 = (uint)playerInfo["SummonorSpell2"];
                    if (NameHashes.TryGetValue(summoner2, out var summoner2Name))
                    {
                        job["SummonorSpell2"] = summoner1Name;
                    }
                }
                result = job;
                break;
            case NPC_BuffRemoveGroup removeGroup:
                job = JObject.FromObject(removeGroup);
                var BuffNameHashGR = (uint)job["BuffNameHash"];
                if (NameHashes.TryGetValue(BuffNameHashGR, out var BuffNameGR))
                {
                    job["BuffNameHash"] = BuffNameGR;
                }
                result = job;
                break;
            case C2S_PlayVOCommand playVOCommand:
                job = JObject.FromObject(playVOCommand);
                var EventHash = (uint)job["EventHash"];
                if (NameHashes.TryGetValue(EventHash, out var EventHashName))
                {
                    job["EventHash"] = EventHashName;
                }
                result = job;
                break;
            case NPC_BuffAddGroup buffAddGroup:
                job = JObject.FromObject(buffAddGroup);
                var BuffNameHashG = (uint)job["BuffNameHash"];
                if (NameHashes.TryGetValue(BuffNameHashG, out var BuffNameHashNameG))
                {
                    job["BuffNameHash"] = BuffNameHashNameG;
                }
                var BuffPackageHashG = (uint)job["PackageHash"];
                if (NameHashes.TryGetValue(BuffPackageHashG, out var PackageHashNameG))
                {
                    job["PackageHash"] = PackageHashNameG;
                }
                result = job;
                break;
            case S2C_SetSpellData setSpellData:
                job = JObject.FromObject(setSpellData);
                var HashedSpellName = (uint)job["HashedSpellName"];
                if (NameHashes.TryGetValue(HashedSpellName, out var SpellName))
                {
                    job["HashedSpellName"] = SpellName;
                }
                result = job;
                break;
            case NPC_BuffRemove2 buffRemove2:
                job = JObject.FromObject(buffRemove2);
                var BuffNameHash2 = (uint)job["BuffNameHash"];
                if (NameHashes.TryGetValue(BuffNameHash2, out var BuffNameHashName2))
                {
                    job["BuffNameHash"] = BuffNameHashName2;
                }
                result = job;
                break;
            case NPC_BuffAdd2 buff:
                job = JObject.FromObject(buff);
                var BuffNameHash = (uint)job["BuffNameHash"];
                if (NameHashes.TryGetValue(BuffNameHash, out var BuffNameHashName))
                {
                    job["BuffNameHash"] = BuffNameHashName;
                }
                var BuffPackageHash = (uint)job["PackageHash"];
                if (NameHashes.TryGetValue(BuffPackageHash, out var PackageHashName))
                {
                    job["PackageHash"] = PackageHashName;
                }
                result = job;
                break;
            case S2C_PlayContextualEmote contextualEmote:
                job = JObject.FromObject(contextualEmote);
                var HashedParam = (uint)job["HashedParam"];
                if (NameHashes.TryGetValue(HashedParam, out var HashedParamName))
                {
                    job["HashedParam"] = HashedParamName;
                }
                result = job;
                break;
            case S2C_NeutralMinionTimerUpdate neutralMinionTimerUpdate:
                job = JObject.FromObject(neutralMinionTimerUpdate);
                var TypeHash = (uint)job["TypeHash"];
                if (NameHashes.TryGetValue(TypeHash, out var TypeName))
                {
                    job["TypeHash"] = TypeName;
                }
                result = job;
                break;
            case S2C_NotifyContextualSituation contextualSituation:
                job = JObject.FromObject(contextualSituation);
                var SituationNameHash = (uint)job["SituationNameHash"];
                if (NameHashes.TryGetValue(SituationNameHash, out var SituationName))
                {
                    job["SituationNameHash"] = SituationName;
                }
                result = job;
                break;
            case FX_Create_Group group:
                job = JObject.FromObject(group);
                foreach (var data in job["FXCreateGroup"])
                {
                    var PackageHash = (uint)data["PackageHash"];
                    if (NameHashes.TryGetValue(PackageHash, out var packageHashName))
                    {
                        data["PackageHash"] = packageHashName;
                    }
                    var EffectNameHash = (uint)data["EffectNameHash"];
                    if (NameHashes.TryGetValue(EffectNameHash, out var effectNameHashName))
                    {
                        data["EffectNameHash"] = effectNameHashName;
                    }
                    var TargetBoneNameHash = (uint)data["TargetBoneNameHash"];
                    if (NameHashes.TryGetValue(TargetBoneNameHash, out var targetBoneNameHashName))
                    {
                        data["TargetBoneNameHash"] = targetBoneNameHashName;
                    }
                    var BoneNameHash = (uint)data["BoneNameHash"];
                    if (NameHashes.TryGetValue(BoneNameHash, out var boneNameHashName))
                    {
                        data["BoneNameHash"] = boneNameHashName;
                    }
                }
                result = job;
                break;
            case NPC_CastSpellAns:
            case MissileReplication:
                job = JObject.FromObject(packet);
                var castInfo = job["CastInfo"];
                
                var missileSpellHash = (uint)castInfo["SpellHash"];
                if (NameHashes.TryGetValue(missileSpellHash, out var missileSpellName))
                {
                    castInfo["SpellHash"] = missileSpellName;
                }
                
                missileSpellHash = (uint)castInfo["PackageHash"];
                if (NameHashes.TryGetValue(missileSpellHash, out missileSpellName))
                {
                    castInfo["PackageHash"] = missileSpellName;
                }
                result = job;
                break;
            case AvatarInfo_Server avatarInfoServer:
                job = JObject.FromObject(avatarInfoServer);
                
                var summoners = job["SummonerIDs"].ToArray();
                for (int l = 0; l < 2; l++)
                {
                    var summonerHash = (uint)summoners[l];
                    if (NameHashes.TryGetValue(summonerHash, out var summonerName))
                    {
                        summoners[l] = summonerName;
                    }
                }
     
                summoners = job["SummonerIDs2"].ToArray();
                for (int l = 0; l < 2; l++)
                {
                    var summonerHash = (uint)summoners[l];
                    if (NameHashes.TryGetValue(summonerHash, out var summonerName))
                    {
                        summoners[l] = summonerName;
                    }
                }
                
                var talents = job["Talents"].ToArray();
                foreach (var talent in talents)
                {
                    var talentHash = (uint)talent["Hash"];
                    if (NameHashes.TryGetValue(talentHash, out var talentName))
                    {
                        talent["Hash"] = talentName;
                    }
                }
                result = job;
                break;
            case OnReplication onReplication:
                job = JObject.FromObject(onReplication);
                //result = UnhashOnReplication(onReplication);
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