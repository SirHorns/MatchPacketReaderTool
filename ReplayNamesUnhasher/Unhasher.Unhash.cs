
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
            case LRFType.HTTP:
                break;
            case LRFType.NFO:
            case LRFType.ENET:
                UnhashPackets(slrf.Packets);
                break;
            case LRFType.NAN:
            default:
                throw new ArgumentOutOfRangeException();
        }
        Reset();
    }

    private void UnhashPackets(List<SerializedPacket> packets)
    {
        foreach (var sp in packets)
        {
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
            var result = UnhashPacket(packet);
            if (result is null)
            {
                continue;
            }
            sp.Packet = result;
        }
    }
}