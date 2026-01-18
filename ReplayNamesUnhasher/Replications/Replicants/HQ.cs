using ReplayNamesUnhasher.Enums;

namespace ReplayNamesUnhasher.Replications;

public class HQ: Replicant
{
    // PID: 1
    [Replication(PID = 1, SID = 0)]
    public Replicate<float> HP { get; set; }
    [Replication(PID = 1, SID = 1)]
    public Replicate<bool> IsInvulnerable { get; set; }
    // PID: 5
    [Replication(PID = 5, SID = 0)]
    public Replicate<bool> IsTargetable { get; set; }
    [Replication(PID = 5, SID = 1)]
    public Replicate<SpellDataFlags> IsTargetableToTeamFlags { get; set; }
}