using System.Reflection;
using LeaguePacketsSerializer;
using LeaguePacketsSerializer.GameServer.Enums;
using Newtonsoft.Json;

namespace LeagueReplayFileSerializer.Replications;

[AttributeUsage(AttributeTargets.Property)]
public class ReplicationAttribute : Attribute
{
    public int PID;
    public int SID;
}

public class Replicate
{
    public object? Value { get; set; }
    public Replicate()
    {
        Value = null;
    }
}

public class Replicate<T>: Replicate
{
    public void SetValue(T value) => Value = value;
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class Replicant
{
    protected readonly PropertyInfo[] Properties;
    public Replicant()
    {
        var found  = GetType().GetProperties().Where(p => p.GetCustomAttribute(typeof(ReplicationAttribute)) != null);
        Properties = found.ToArray();
    }
    
    public void SetValue(int pid, int sid, object value)
    {
        foreach (var property in Properties)
        {
            var attribute = property.GetCustomAttribute(typeof(ReplicationAttribute));
            if (attribute is not ReplicationAttribute replicationAttribute)
            {
                continue;
            }

            if (replicationAttribute.PID != pid)
            {
                continue;
            }

            if (replicationAttribute.SID != sid)
            {
                continue;
            }

            var replicant = (Replicate)property.GetValue(this, null);
            if (replicant is null)
            {
                switch (this)
                {
                    case HQ hq:
                        break;
                    case Minion minion:
                        replicant = (Replicate)Activator.CreateInstance(property.PropertyType);
                        property.SetValue(minion, replicant, null);
                        break;
                }
            }
            if (replicant is null)
            {
                break;
            }
            replicant.Value = value;
            break;
        }
    }
}

public class HQ: Replicant
{
    // PID: 1
    [ReplicationAttribute(PID = 1, SID = 0)]
    public Replicate<float> HP { get; set; }
    [ReplicationAttribute(PID = 1, SID = 1)]
    public Replicate<bool> IsInvulnerable { get; set; }
    // PID: 5
    [ReplicationAttribute(PID = 5, SID = 0)]
    public Replicate<bool> IsTargetable { get; set; }
    [ReplicationAttribute(PID = 5, SID = 1)]
    public Replicate<SpellDataFlags> IsTargetableToTeamFlags { get; set; }
}

public class Minion : Replicant
{
    // PID: 1
    [ReplicationAttribute(PID = 1, SID = 0)]
    public Replicate<float> HP { get; set; }
    [ReplicationAttribute(PID = 1, SID = 1)]
    public Replicate<float> MaxHP { get; set; }
    [ReplicationAttribute(PID = 1, SID = 2)]
    public Replicate<float> Lifetime { get; set; }
    [ReplicationAttribute(PID = 1, SID = 3)]
    public Replicate<float> MaxLifetime { get; set; }
    [ReplicationAttribute(PID = 1, SID = 4)]
    public Replicate<float> LifetimeTicks { get; set; }
    [ReplicationAttribute(PID = 1, SID = 5)]
    public Replicate<float> MaxMP { get; set; }
    [ReplicationAttribute(PID = 1, SID = 6)]
    public Replicate<float> MP { get; set; }
    [ReplicationAttribute(PID = 1, SID = 7)]
    public Replicate<ActionState> ActionState { get; set; }
    [ReplicationAttribute(PID = 1, SID = 8)]
    public Replicate<float> MagicImmune { get; set; }
    [ReplicationAttribute(PID = 1, SID = 9)]
    public Replicate<float> IsInvulnerable { get; set; }
    [ReplicationAttribute(PID = 1, SID = 10)]
    public Replicate<float> IsPhysicalImmune { get; set; }
    [ReplicationAttribute(PID = 1, SID = 11)]
    public Replicate<float> IsLifestealImmune { get; set; }
    [ReplicationAttribute(PID = 1, SID = 12)]
    public Replicate<float> BaseAttackDamage { get; set; }
    [ReplicationAttribute(PID = 1, SID = 13)]
    public Replicate<float> Armor { get; set; }
    [ReplicationAttribute(PID = 1, SID = 14)]
    public Replicate<float> SpellBlock { get; set; }
    [ReplicationAttribute(PID = 1, SID = 15)]
    public Replicate<float> AttackSpeedMod { get; set; }
    [ReplicationAttribute(PID = 1, SID = 16)]
    public Replicate<float> FlatPhysicalDamageMod { get; set; }
    [ReplicationAttribute(PID = 1, SID = 17)]
    public Replicate<float> PercentPhysicalDamageMod { get; set; }
    [ReplicationAttribute(PID = 1, SID = 18)]
    public Replicate<float> FlatMagicDamageMod { get; set; }
    [ReplicationAttribute(PID = 1, SID = 19)]
    public Replicate<float> HPRegenRate { get; set; }
    [ReplicationAttribute(PID = 1, SID = 20)]
    public Replicate<float> PARRegenRate { get; set; }
    [ReplicationAttribute(PID = 1, SID = 21)]
    public Replicate<float> FlatMagicReduction { get; set; }
    [ReplicationAttribute(PID = 3, SID = 22)]
    public Replicate<float> PercentMagicReduction { get; set; }
    // PID: 3
    [ReplicationAttribute(PID = 3, SID = 0)]
    public Replicate<float> FlatBubbleRadiusMod { get; set; }
    [ReplicationAttribute(PID = 3, SID = 1)]
    public Replicate<float> PercentBubbleRadiusMod { get; set; }
    [ReplicationAttribute(PID = 3, SID = 2)]
    public Replicate<float> MoveSpeed { get; set; }
    [ReplicationAttribute(PID = 3, SID = 3)]
    public Replicate<float> SkinScaleCoef { get; set; }
    [ReplicationAttribute(PID = 3, SID = 4)]
    public Replicate<bool> IsTargetable { get; set; }
    [ReplicationAttribute(PID = 3, SID = 5)]
    public Replicate<SpellDataFlags> IsTargetableToTeamFlags { get; set; }
}