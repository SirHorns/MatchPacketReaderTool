using System;
using System.Collections.Generic;
using LeaguePacketsSerializer.GameServer.Enums;
using LeaguePacketsSerializer.Replication;

namespace LeaguePacketsSerializer;

public static class DataDict
{
    private static bool _recording;
    private static uint _u { get; set; }
    private static float _f { get; set; }
    private static bool _b { get; set; }
    private static Replicate[,] _currentValues { get; set; }
    private static ReplicationType _currentReplicationType { get; set; }
    private static ReplicationDataType?[][,] _replicationMaps { get; set; }

    static DataDict()
    {
        _recording = true;
        var types = (ReplicationType[])Enum.GetValues(typeof(ReplicationType));
        _replicationMaps = new ReplicationDataType?[types.Length][,];
        foreach (var type in types)
        {
            _replicationMaps[(int)type] = new ReplicationDataType?[6, 32];
            Gen(type, null);
        }

        _recording = false;
    }
    

    internal static Dictionary<string, object> Gen(ReplicationType replicationType, Replicate[,]? values)
    {
        var data = new Dictionary<string, object>();
        _currentReplicationType = replicationType;
        _currentValues = values;

        switch (replicationType)
        {
            // UpdateFloat\((.*), (\d+), (\d+)\) -> if(TryGetFloat($2, $3)) data["$1"] = f
            // UpdateUint\((.*), (\d+), (\d+)\) -> if(TryGetUint($2, $3)) data["$1"] = u
            // UpdateBool\((.*), (\d+), (\d+)\) -> if(TryGetUint($2, $3)) data["$1"] = u == 1u

            case ReplicationType.Turret:

                /**/
                if (TryGetFloat(1, 0))
                { 
                    data["Stats.ManaPoints.Total"] = _f; //mMaxMP
                }
                /**/
                if (TryGetFloat(1, 1)) data["Stats.CurrentMana"] = _f; //mMP
                if (TryGetUint(1, 2)) data["Stats.ActionState"] = ((ActionState)_u).ToString(); //ActionState
                if (TryGetBool(1, 3)) data["Stats.IsMagicImmune"] = _b; //MagicImmune
                if (TryGetBool(1, 4)) data["Stats.IsInvulnerable"] = _b; //IsInvulnerable
                if (TryGetBool(1, 5)) data["Stats.IsPhysicalImmune"] = _b; //IsPhysicalImmune
                if (TryGetBool(1, 6)) data["Stats.IsLifestealImmune"] = _b; //IsLifestealImmune
                if (TryGetFloat(1, 7)) data["Stats.AttackDamage.BaseValue"] = _f; //mBaseAttackDamage
                if (TryGetFloat(1, 8)) data["Stats.Armor.Total"] = _f; //mArmor
                if (TryGetFloat(1, 9)) data["Stats.MagicResist.Total"] = _f; //mSpellBlock
                if (TryGetFloat(1, 10)) data["Stats.AttackSpeedMultiplier.Total"] = _f; //mAttackSpeedMod
                if (TryGetFloat(1, 11)) data["Stats.AttackDamage.FlatBonus"] = _f; //mFlatPhysicalDamageMod
                if (TryGetFloat(1, 12)) data["Stats.AttackDamage.PercentBonus"] = _f; //mPercentPhysicalDamageMod
                if (TryGetFloat(1, 13)) data["Stats.AbilityPower.Total"] = _f; //mFlatMagicDamageMod
                if (TryGetFloat(1, 14)) data["Stats.HealthRegeneration.Total"] = _f; //mHPRegenRate
                if (TryGetFloat(3, 0)) data["Stats.CurrentHealth"] = _f; //mHP
                if (TryGetFloat(3, 1)) data["Stats.HealthPoints.Total"] = _f; //mMaxHP
                /**/
                if (TryGetFloat(3, 2)) data["Stats.PerceptionRange.FlatBonus"] = _f; //mFlatBubbleRadiusMod
                /**/
                if (TryGetFloat(3, 3)) data["Stats.PerceptionRange.PercentBonus"] = _f; //mPercentBubbleRadiusMod
                if (TryGetFloat(3, 4)) data["Stats.GetTrueMoveSpeed()"] = _f; //mMoveSpeed
                if (TryGetFloat(3, 5)) data["Stats.Size.Total"] = _f; //mSkinScaleCoef(mistyped as mCrit)
                if (TryGetBool(5, 0)) data["Stats.IsTargetable"] = _b; //mIsTargetable
                if (TryGetUint(5, 1))
                    data["Stats.IsTargetableToTeam"] = ((SpellDataFlags)_u).ToString(); //mIsTargetableToTeamFlags

                break;

            case ReplicationType.HQ:

                if (TryGetFloat(1, 0)) data["Stats.CurrentHealth"] = _f; //mHP
                if (TryGetBool(1, 1)) data["Stats.IsInvulnerable"] = _b; //IsInvulnerable
                if (TryGetBool(5, 0)) data["Stats.IsTargetable"] = _b; //mIsTargetable
                if (TryGetUint(5, 1)) data["Stats.IsTargetableToTeam"] = ((SpellDataFlags)_u).ToString(); //mIsTargetableToTeamFlags

                break;

            case ReplicationType.Hero:

                if (TryGetFloat(0, 0)) data["Stats.Gold"] = _f; //mGold
                /**/
                if (TryGetFloat(0, 1)) data["Stats.TotalGold"] = _f; //mGoldTotal
                if (TryGetUint(0, 2)) data["(uint)Stats.SpellsEnabled"] = _u; //mReplicatedSpellCanCastBitsLower1
                if (TryGetUint(0, 3)) data["(uint)(Stats.SpellsEnabled >> 32)"] = _u; //mReplicatedSpellCanCastBitsUpper1
                if (TryGetUint(0, 4)) data["(uint)Stats.SummonerSpellsEnabled"] = _u; //mReplicatedSpellCanCastBitsLower2
                if (TryGetUint(0, 5))
                    data["(uint)(Stats.SummonerSpellsEnabled >> 32)"] = _u; //mReplicatedSpellCanCastBitsUpper2
                /**/
                if (TryGetUint(0, 6)) data["Stats.EvolvePoints"] = _u; //mEvolvePoints
                /**/
                if (TryGetUint(0, 7)) data["Stats.EvolveFlags"] = _u; //mEvolveFlag
                for (var i = 0; i < 4; i++)
                {
                    if (TryGetFloat(0, 8 + i)) data[$"Stats.ManaCost[{i}]"] = _f; //ManaCost_{i}
                }

                for (var i = 0; i < 16; i++)
                {
                    if (TryGetFloat(0, 12 + i)) data[$"Stats.ManaCost[{45 + i}]"] = _f; //ManaCost_Ex{i}
                }

                if (TryGetUint(1, 0)) data["Stats.ActionState"] = ((ActionState)_u).ToString();
                if (TryGetBool(1, 1)) data["Stats.IsMagicImmune"] = _b; //MagicImmune
                if (TryGetBool(1, 2)) data["Stats.IsInvulnerable"] = _b; //IsInvulnerable
                if (TryGetBool(1, 3)) data["Stats.IsPhysicalImmune"] = _b; //IsPhysicalImmune
                if (TryGetBool(1, 4)) data["Stats.IsLifestealImmune"] = _b; //IsLifestealImmune
                if (TryGetFloat(1, 5)) data["Stats.AttackDamage.BaseValue"] = _f; //mBaseAttackDamage
                if (TryGetFloat(1, 6)) data["Stats.AbilityPower.BaseValue"] = _f; //mBaseAbilityDamage
                /**/
                if (TryGetFloat(1, 7)) data["Stats.DodgeChance"] = _f; //mDodge
                if (TryGetFloat(1, 8)) data["Stats.CriticalChance.Total"] = _f; //mCrit
                if (TryGetFloat(1, 9)) data["Stats.Armor.Total"] = _f; //mArmor
                if (TryGetFloat(1, 10)) data["Stats.MagicResist.Total"] = _f; //mSpellBlock
                if (TryGetFloat(1, 11)) data["Stats.HealthRegeneration.Total"] = _f; //mHPRegenRate
                if (TryGetFloat(1, 12)) data["Stats.ManaRegeneration.Total"] = _f; //mPARRegenRate
                if (TryGetFloat(1, 13)) data["Stats.Range.Total"] = _f; //mAttackRange
                if (TryGetFloat(1, 14)) data["Stats.AttackDamage.FlatBonus"] = _f; //mFlatPhysicalDamageMod
                if (TryGetFloat(1, 15)) data["Stats.AttackDamage.PercentBonus"] = _f; //mPercentPhysicalDamageMod
                if (TryGetFloat(1, 16)) data["Stats.AbilityPower.FlatBonus"] = _f; //mFlatMagicDamageMod
                /**/
                if (TryGetFloat(1, 17)) data["Stats.MagicResist.FlatBonus"] = _f; //mFlatMagicReduction
                /**/
                if (TryGetFloat(1, 18)) data["Stats.MagicResist.PercentBonus"] = _f; //mPercentMagicReduction
                if (TryGetFloat(1, 19)) data["Stats.AttackSpeedMultiplier.Total"] = _f; //mAttackSpeedMod
                if (TryGetFloat(1, 20)) data["Stats.Range.FlatBonus"] = _f; //mFlatCastRangeMod
                // TODO: Find out why a negative value is required for ability cooldowns to display properly.
                if (TryGetFloat(1, 21)) data["Stats.CooldownReduction.Total"] = -_f; //mPercentCooldownMod
                /**/
                if (TryGetFloat(1, 22)) data["Stats.PassiveCooldownEndTime"] = _f; //mPassiveCooldownEndTime
                /**/
                if (TryGetFloat(1, 23)) data["Stats.PassiveCooldownTotalTime"] = _f; //mPassiveCooldownTotalTime
                if (TryGetFloat(1, 24)) data["Stats.ArmorPenetration.FlatBonus"] = _f; //mFlatArmorPenetration
                if (TryGetFloat(1, 25)) data["Stats.ArmorPenetration.PercentBonus"] = _f; //mPercentArmorPenetration
                if (TryGetFloat(1, 26)) data["Stats.MagicPenetration.FlatBonus"] = _f; //mFlatMagicPenetration
                if (TryGetFloat(1, 27)) data["Stats.MagicPenetration.PercentBonus"] = _f; //mPercentMagicPenetration
                if (TryGetFloat(1, 28)) data["Stats.LifeSteal.Total"] = _f; //mPercentLifeStealMod
                if (TryGetFloat(1, 29)) data["Stats.SpellVamp.Total"] = _f; //mPercentSpellVampMod
                if (TryGetFloat(1, 30)) data["Stats.Tenacity.Total"] = _f; //mPercentCCReduction
                if (TryGetFloat(2, 0)) data["Stats.Armor.PercentBonus"] = _f; //mPercentBonusArmorPenetration
                if (TryGetFloat(2, 1)) data["Stats.MagicPenetration.PercentBonus"] = _f; //mPercentBonusMagicPenetration
                /**/
                if (TryGetFloat(2, 2)) data["Stats.HealthRegeneration.BaseValue"] = _f; //mBaseHPRegenRate
                /**/
                if (TryGetFloat(2, 3)) data["Stats.ManaRegeneration.BaseValue"] = _f; //mBasePARRegenRate
                if (TryGetFloat(3, 0)) data["Stats.CurrentHealth"] = _f; //mHP
                if (TryGetFloat(3, 1)) data["Stats.CurrentMana"] = _f; //mMP
                if (TryGetFloat(3, 2)) data["Stats.HealthPoints.Total"] = _f; //mMaxHP
                if (TryGetFloat(3, 3)) data["Stats.ManaPoints.Total"] = _f; //mMaxMP
                if (TryGetFloat(3, 4)) data["Stats.Experience"] = _f; //mExp
                /**/
                if (TryGetFloat(3, 5)) data["Stats.LifeTime"] = _f; //mLifetime
                /**/
                if (TryGetFloat(3, 6)) data["Stats.MaxLifeTime"] = _f; //mMaxLifetime
                /**/
                if (TryGetFloat(3, 7)) data["Stats.LifeTimeTicks"] = _f; //mLifetimeTicks
                /**/
                if (TryGetFloat(3, 8)) data["Stats.PerceptionRange.FlatMod"] = _f; //mFlatBubbleRadiusMod
                /**/
                if (TryGetFloat(3, 9)) data["Stats.PerceptionRange.PercentMod"] = _f; //mPercentBubbleRadiusMod
                if (TryGetFloat(3, 10)) data["Stats.GetTrueMoveSpeed()"] = _f; //mMoveSpeed
                if (TryGetFloat(3, 11)) data["Stats.Size.Total"] = _f; //mSkinScaleCoef(mistyped as mCrit)
                /**/
                if (TryGetFloat(3, 12)) data["Stats.FlatPathfindingRadiusMod"] = _f; //mPathfindingRadiusMod
                if (TryGetUint(3, 13)) data["Stats.Level"] = _u; //mLevelRef
                if (TryGetUint(3, 14)) data["Owner.MinionCounter"] = _u; //mNumNeutralMinionsKilled
                if (TryGetBool(3, 15)) data["Stats.IsTargetable"] = _b; //mIsTargetable
                if (TryGetUint(3, 16))
                    data["Stats.IsTargetableToTeam"] = ((SpellDataFlags)_u).ToString(); //mIsTargetableToTeamFlags

                break;

            case ReplicationType.Minion:
                if (TryGetFloat(1, 0)) data["Stats.CurrentHealth"] = _f; //mHP
                if (TryGetFloat(1, 1)) data["Stats.HealthPoints.Total"] = _f; //mMaxHP
                /**/
                if (TryGetFloat(1, 2)) data["Stats.LifeTime"] = _f; //mLifetime
                /**/
                if (TryGetFloat(1, 3)) data["Stats.MaxLifeTime"] = _f; //mMaxLifetime
                /**/
                if (TryGetFloat(1, 4)) data["Stats.LifeTimeTicks"] = _f; //mLifetimeTicks
                if (TryGetFloat(1, 5)) data["Stats.ManaPoints.Total"] = _f; //mMaxMP
                if (TryGetFloat(1, 6)) data["Stats.CurrentMana"] = _f; //mMP
                if (TryGetUint(1, 7)) data["Stats.ActionState"] = ((ActionState)_u).ToString(); //ActionState
                if (TryGetBool(1, 8)) data["Stats.IsMagicImmune"] = _b; //MagicImmune
                if (TryGetBool(1, 9)) data["Stats.IsInvulnerable"] = _b; //IsInvulnerable
                if (TryGetBool(1, 10)) data["Stats.IsPhysicalImmune"] = _b; //IsPhysicalImmune
                if (TryGetBool(1, 11)) data["Stats.IsLifestealImmune"] = _b; //IsLifestealImmune
                if (TryGetFloat(1, 12)) data["Stats.AttackDamage.BaseValue"] = _f; //mBaseAttackDamage
                if (TryGetFloat(1, 13)) data["Stats.Armor.Total"] = _f; //mArmor
                if (TryGetFloat(1, 14)) data["Stats.MagicResist.Total"] = _f; //mSpellBlock
                if (TryGetFloat(1, 15)) data["Stats.AttackSpeedMultiplier.Total"] = _f; //mAttackSpeedMod
                if (TryGetFloat(1, 16)) data["Stats.AttackDamage.FlatBonus"] = _f; //mFlatPhysicalDamageMod
                if (TryGetFloat(1, 17)) data["Stats.AttackDamage.PercentBonus"] = _f; //mPercentPhysicalDamageMod
                if (TryGetFloat(1, 18)) data["Stats.AbilityPower.Total"] = _f; //mFlatMagicDamageMod
                if (TryGetFloat(1, 19)) data["Stats.HealthRegeneration.Total"] = _f; //mHPRegenRate
                if (TryGetFloat(1, 20)) data["Stats.ManaRegeneration.Total"] = _f; //mPARRegenRate
                if (TryGetFloat(1, 21)) data["Stats.MagicResist.FlatBonus"] = _f; //mFlatMagicReduction
                if (TryGetFloat(1, 22)) data["Stats.MagicResist.PercentBonus"] = _f; //mPercentMagicReduction
                /**/
                if (TryGetFloat(3, 0)) data["Stats.PerceptionRange.FlatBonus"] = _f; //mFlatBubbleRadiusMod
                /**/
                if (TryGetFloat(3, 1)) data["Stats.PerceptionRange.PercentBonus"] = _f; //mPercentBubbleRadiusMod
                if (TryGetFloat(3, 2)) data["Stats.GetTrueMoveSpeed()"] = _f; //mMoveSpeed
                if (TryGetFloat(3, 3)) data["Stats.Size.Total"] = _f; //mSkinScaleCoef(mistyped as mCrit)
                if (TryGetBool(3, 4)) data["Stats.IsTargetable"] = _b; //mIsTargetable
                if (TryGetUint(3, 5)) data["Stats.IsTargetableToTeam"] = ((SpellDataFlags)_u).ToString(); //mIsTargetableToTeamFlags

                break; 
        }

        return data;
    }
    
    public enum ReplicationDataType
    {
        UNKNOWN,
        FLOAT,
        UINT,
        BOOL,
        ACTION_STATE,
        SPELL_DATA_FLAGS
    }

    internal static ReplicationDataType? GetReplicationValueType(int replicationType, byte index, byte sid)
    {
        var replicationSet = _replicationMaps[replicationType];
        var res = replicationSet[index, sid];
        return res;
    }
     
    private static bool TryGet(int primaryId, int secondaryId, ReplicationDataType replicationDataType)
    {
        //TODO: value.isFloat != isFloat
        if (_recording)
        {
            _replicationMaps[(int)_currentReplicationType][primaryId, secondaryId] = replicationDataType;
            return false;
        }

        Replicate value = _currentValues[primaryId, secondaryId];
        if (value == null)
        {
            return false;
        }


        switch (replicationDataType)
        {
            case ReplicationDataType.FLOAT:
                _f = value.Float;
                break;
            case ReplicationDataType.UINT:
                _u = value.Uint;
                break;
            case ReplicationDataType.BOOL:
                _b = value.Bool;
                break;
        }
        return true;
    }

    private static bool TryGetUint(int primaryId, int secondaryId)
    {
        return TryGet(primaryId, secondaryId, ReplicationDataType.UINT);
    }

    private static bool TryGetFloat(int primaryId, int secondaryId)
    {
        return TryGet(primaryId, secondaryId, ReplicationDataType.FLOAT);
    }
    
    private static bool TryGetBool(int primaryId, int secondaryId)
    {
        return TryGet(primaryId, secondaryId, ReplicationDataType.BOOL);
    }
}