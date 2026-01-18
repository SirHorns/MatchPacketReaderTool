using ReplayNamesUnhasher.Enums;

namespace ReplayNamesUnhasher.Replications;

public class Minion : Replicant
{
    // PID: 1
    [Replication(PID = 1, SID = 0)]
    public Replicate<float> HP { get; set; }
    [Replication(PID = 1, SID = 1)]
    public Replicate<float> MaxHP { get; set; }
    [Replication(PID = 1, SID = 2)]
    public Replicate<float> Lifetime { get; set; }
    [Replication(PID = 1, SID = 3)]
    public Replicate<float> MaxLifetime { get; set; }
    [Replication(PID = 1, SID = 4)]
    public Replicate<float> LifetimeTicks { get; set; }
    [Replication(PID = 1, SID = 5)]
    public Replicate<float> MaxMP { get; set; }
    [Replication(PID = 1, SID = 6)]
    public Replicate<float> MP { get; set; }
    [Replication(PID = 1, SID = 7)]
    public Replicate<ActionState> ActionState { get; set; }
    [Replication(PID = 1, SID = 8)]
    public Replicate<float> MagicImmune { get; set; }
    [Replication(PID = 1, SID = 9)]
    public Replicate<float> IsInvulnerable { get; set; }
    [Replication(PID = 1, SID = 10)]
    public Replicate<float> IsPhysicalImmune { get; set; }
    [Replication(PID = 1, SID = 11)]
    public Replicate<float> IsLifestealImmune { get; set; }
    [Replication(PID = 1, SID = 12)]
    public Replicate<float> BaseAttackDamage { get; set; }
    [Replication(PID = 1, SID = 13)]
    public Replicate<float> Armor { get; set; }
    [Replication(PID = 1, SID = 14)]
    public Replicate<float> SpellBlock { get; set; }
    [Replication(PID = 1, SID = 15)]
    public Replicate<float> AttackSpeedMod { get; set; }
    [Replication(PID = 1, SID = 16)]
    public Replicate<float> FlatPhysicalDamageMod { get; set; }
    [Replication(PID = 1, SID = 17)]
    public Replicate<float> PercentPhysicalDamageMod { get; set; }
    [Replication(PID = 1, SID = 18)]
    public Replicate<float> FlatMagicDamageMod { get; set; }
    [Replication(PID = 1, SID = 19)]
    public Replicate<float> HPRegenRate { get; set; }
    [Replication(PID = 1, SID = 20)]
    public Replicate<float> PARRegenRate { get; set; }
    [Replication(PID = 1, SID = 21)]
    public Replicate<float> FlatMagicReduction { get; set; }
    [Replication(PID = 3, SID = 22)]
    public Replicate<float> PercentMagicReduction { get; set; }
    // PID: 3
    [Replication(PID = 3, SID = 0)]
    public Replicate<float> FlatBubbleRadiusMod { get; set; }
    [Replication(PID = 3, SID = 1)]
    public Replicate<float> PercentBubbleRadiusMod { get; set; }
    [Replication(PID = 3, SID = 2)]
    public Replicate<float> MoveSpeed { get; set; }
    [Replication(PID = 3, SID = 3)]
    public Replicate<float> SkinScaleCoef { get; set; }
    [Replication(PID = 3, SID = 4)]
    public Replicate<bool> IsTargetable { get; set; }
    [Replication(PID = 3, SID = 5)]
    public Replicate<SpellDataFlags> IsTargetableToTeamFlags { get; set; }
}