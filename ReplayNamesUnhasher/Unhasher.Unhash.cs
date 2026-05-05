
using LeaguePackets;
using LeaguePackets.Game;
using LeagueReplayFile.Enums;
using LeagueReplayFileSerializer;

namespace ReplayNamesUnhasher;

public partial class Unhasher
{
    public void Unhashie(SLRF slrf)
    {
        Console.WriteLine("Unhashing is still a WIP");
        switch (slrf.Type)
        {
            case LRFTypes.HTTP:
                break;
            case LRFTypes.NFO:
            case LRFTypes.ENET:
                UnhashPackets(slrf.Packets);
                break;
            case LRFTypes.NAN:
            default:
                throw new ArgumentOutOfRangeException();
        }
        Reset();
    }

    private void UnhashPackets(List<SerializedPacket> packets)
    {
        foreach (var sp in packets)
        {
            object result = null;
            // incase random object somehow replaces the packet object
            if (sp.Packet is not BasePacket packet)
            {
                continue;
            }
            
            // castinfo
            // talent
            // color
            // argsbuff, argsheal, argsDamage, argsforclient, argsminionkill
            //TODO: FINDING ALL PACKETS THAT NEED TO BE UNHASHED
            switch (packet)
            {
                case SynchVersionS2C:
                    UnhashPacket(packet);
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
                case OnReplication onReplication:
                    result = UnhashOnReplication(onReplication);
                    break;
                default:
                    continue;
            }

            if (result is null)
            {
                continue;
            }
            sp.Packet = result;
        }
    }
}