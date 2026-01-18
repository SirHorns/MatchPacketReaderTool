using LeaguePackets;
using LeaguePackets.Game;
using ReplayNamesUnhasher.Enums;
using ReplayNamesUnhasher.Replications;

namespace ReplayNamesUnhasher;

public partial class Unhasher
{
    private void RegisterGameObjectType(BasePacket packet)
    {
        uint netID = 0;
        GameObjectTypes type = GameObjectTypes.Unknown;
        switch (packet)
        {
            case S2C_CreateTurret pkt:
                netID = pkt.NetID;
                type = GameObjectTypes.ObjAIBase_Turret;
                break;
            case S2C_SpawnTurret pkt:
                netID = pkt.NetID;
                type = GameObjectTypes.ObjAIBase_Turret;
                break;
            case S2C_CreateHero pkt:
                netID = pkt.NetID;
                type = GameObjectTypes.ObjAIBase_Hero;
                break;
            case S2C_CreateNeutral pkt:
                netID = pkt.NetID;
                type = GameObjectTypes.NeutralMinionCamp;
                break;
            case CHAR_SpawnPet pkt:
                netID = pkt.SenderNetID;
                type = GameObjectTypes.AttackableUnit;
                break;
            case SpawnMinionS2C pkt:
                netID = pkt.NetID;
                type = GameObjectTypes.AttackableUnit;
                break;
            case Barrack_SpawnUnit pkt:
                netID = pkt.SenderNetID;
                type = GameObjectTypes.AttackableUnit;
                break;
            case SpawnBotS2C pkt:
                netID = pkt.NetID;
                type = GameObjectTypes.Unknown; // GameObjectTypes.Bot;
                break;
            case SpawnLevelPropS2C pkt:
                netID = pkt.NetID;
                type = GameObjectTypes.LevelProp;
                break;
            case SpawnMarkerS2C pkt:
                netID = pkt.NetID;
                type = GameObjectTypes.ObjAIBase_Marker;
                break;
            case S2C_ForceCreateMissile pkt:
                return;
                netID = pkt.MissileNetID;
                type = GameObjectTypes.Missile;
                break;
            case IGamePacketsList parent:
                foreach (var subPacket in parent.Packets)
                {
                    RegisterGameObjectType(subPacket);
                }
                break;
            default:
                return;
        }

        if (netID == 0)
        {
            return;
        }
        
        if (!_netIdToTypesMap.TryAdd(netID, type))
        {
            //var saved = _netIdToTypesMap[netID];
            //Console.WriteLine($"Attempt Map Override :: {netID} : {saved} -> {type}");
        }
    }
    
    private void RegisterUnitReplicationType(BasePacket packet)
    {
        switch (packet)
        {
            case S2C_CreateTurret ct:
                _replicationTypes[ct.NetID] = ReplicationTypes.Turret;
                break;
            case S2C_SpawnTurret st:
                _replicationTypes[st.NetID] = ReplicationTypes.Turret;
                break;
            case S2C_CreateHero ch:
                _replicationTypes[ch.NetID] = ReplicationTypes.Hero;
                break;
            case S2C_CreateNeutral cn:
                _replicationTypes[cn.NetID] = ReplicationTypes.Minion;
                break;
            case CHAR_SpawnPet sp:
                _replicationTypes[sp.SenderNetID] = ReplicationTypes.Minion;
                break;
            case SpawnMinionS2C sm:
                _replicationTypes[sm.NetID] = ReplicationTypes.Minion;
                break;
            case Barrack_SpawnUnit su:
                _replicationTypes[su.SenderNetID] = ReplicationTypes.Minion;
                break;
            case SpawnLevelPropS2C slp:
                _replicationTypes[slp.SenderNetID] = ReplicationTypes.Prop;
                break;
            case SpawnMarkerS2C sp:
                //_replicationTypes[sp.SenderNetID] = ReplicationTypes.Marker;
                break;
            case IGamePacketsList parent:
                foreach (var subPacket in parent.Packets)
                {
                    RegisterUnitReplicationType(subPacket);
                }
                break;
        }
    }
}