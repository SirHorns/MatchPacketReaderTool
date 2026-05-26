
using LeaguePackets;
using LeaguePackets.Game;
using LeagueReplayFile.Enums;
using LeagueReplayFileSerializer;

namespace ReplayNamesUnhasher;

public partial class Unhasher
{
    public void Unhashie(SLRF slrf)
    {
        if (slrf.Packets.Count == 0)
        {
            return;
        }
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
            // incase random object somehow replaces the packet object
            if (sp.Packet is not BasePacket packet)
            {
                continue;
            }
            var result = UnhashPacket(packet);
            if (result is null)
            {
                continue;
            }
            sp.Packet = result;
        }
    }
}