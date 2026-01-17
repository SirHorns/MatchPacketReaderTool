namespace LeagueReplayFileSerializer.Enums;

public enum GameObjectTypes
{
    Unknown,
    InfoPoint,
    EffectEmitter,
    LevelProp,
    Missile,
    ChainMissile = Missile,
    CircleMissile = Missile,
    LineMissile = Missile,
    NeutralMinionCamp,
    AttackableUnit,
    ObjAIBase = AttackableUnit,
    ObjAIBase_Hero = ObjAIBase,
    ObjAIBase_Turret = ObjAIBase,
    ObjAIBase_Minion = ObjAIBase,
    ObjAIBase_Marker = ObjAIBase,
    ObjAIBase_LevelProp = ObjAIBase,
    ObjAIBase_FollowerObject = ObjAIBase,
    ObjBuilding,
    ObjBuilding_Shop = ObjBuilding,
    ObjBuilding_Levelsizer  = ObjBuilding,
    ObjBuilding_NavPoint = ObjBuilding,
    ObjBuilding_Lake = ObjBuilding,
    ObjBuilding_SpawnPoint = ObjBuilding,
    ObjBuildingBarracks = ObjBuilding,
    ObjBuilding_Animated = ObjBuilding,
    ObjAnimated_Turret = ObjBuilding_Animated,
    ObjAnimated_HQ = ObjBuilding_Animated,
    ObjAnimated_BarracksDampener = ObjBuilding_Animated,
        
}