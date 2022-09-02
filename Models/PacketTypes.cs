using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MatchPacketReaderTool.Models;

public class PacketTypes
{
    private static List<string> _filters = new List<string>() 
    {
            "All_Packets", "AI_TargetHeroS2C", "AI_TargetS2C", "AddRegion", "AvatarInfo_Server", "Barrack_SpawnUnit",
            "Basic_Attack", "Basic_Attack_Pos", "Building_Die", "BuyItemAns", "CHAR_SetCooldown", "CHAR_SpawnPet",
            "ChangeSlotSpellData", "ChangeSlotSpellData_OwnerOnly", "Chat", "DampenerSwitchStates", "Dummy",
            "FX_Create_Group", "FX_Kill", "HeroReincarnateAlive", "KeyCheckPacket", "MissileReplication",
            "ModifyShield", "NPC_BuffAdd2", "NPC_BuffRemove2", "NPC_BuffReplace", "NPC_BuffUpdateCount",
            "NPC_BuffUpdateNumCounter", "NPC_CastSpellAns", "NPC_Die_Broadcast", "NPC_Die_EventHistory", "NPC_Hero_Die",
            "NPC_InstantStop_Attack", "NPC_LevelUp", "NPC_MessageToClient_Broadcast", "NPC_MessageToClient_MapView",
            "NPC_SetAutocast", "NPC_UpgradeSpellAns", "OnEnterLocalVisibilityClient", "OnEnterVisibilityClient",
            "OnEvent", "OnLeaveLocalVisibilityClient", "OnLeaveVisibilityClient", "OnReplication", "RemoveItemAns",
            "RemoveRegion", "ReplayOnly_GoldEarned", "ReplayOnly_MultiKillCountUpdate", "RequestRename",
            "RequestReskin", "S2C_ActivateMinionCamp", "S2C_AmmoUpdate", "S2C_CameraBehavior",
            "S2C_ChangeCharacterData", "S2C_ChangeCharacterVoice", "S2C_ChangeEmitterGroup", "S2C_ChangeMissileSpeed",
            "S2C_CloseShop", "S2C_CreateHero", "S2C_CreateMinionCamp", "S2C_CreateNeutral", "S2C_CreateTurret",
            "S2C_DestroyClientMissile", "S2C_DisableHUDForEndOfGame", "S2C_EndGame", "S2C_EndSpawn",
            "S2C_FX_OnEnterTeamVisibility", "S2C_FX_OnLeaveTeamVisibility", "S2C_FaceDirection", "S2C_FadeMinions",
            "S2C_FadeOutMainSFX", "S2C_ForceCreateMissile", "S2C_IncrementMinionKills", "S2C_InteractiveMusicCommand",
            "S2C_LineMissileHitList", "S2C_MapPing", "S2C_MoveCameraToPoint", "S2C_NPC_Die_MapView",
            "S2C_NeutralMinionTimerUpdate", "S2C_Neutral_Camp_Empty", "S2C_NotifyContextualSituation",
            "S2C_OnEnterTeamVisibility", "S2C_OnEventWorld", "S2C_OnLeaveTeamVisibility", "S2C_PauseAnimation",
            "S2C_Ping_Load_Info", "S2C_PlayAnimation", "S2C_PlayEmote", "S2C_PlayVOCommand", "S2C_PopCharacterData",
            "S2C_SetAnimStates", "S2C_SetCanSurrender", "S2C_SetCircularMovementRestriction",
            "S2C_SetGreyscaleEnabledWhenDead", "S2C_SetHoverIndicatorEnabled", "S2C_SetHoverIndicatorTarget",
            "S2C_SetInputLockFlag", "S2C_SetInventory_MapView", "S2C_SetItemCharges", "S2C_SetSpellData",
            "S2C_ShowHealthBar", "S2C_StartGame", "S2C_StartSpawn", "S2C_StopAnimation", "S2C_TeamSurrenderStatus",
            "S2C_TeamSurrenderVote", "S2C_TeamUpdateDragonBuffCount", "S2C_ToolTipVars", "S2C_UnitSetLookAt",
            "S2C_UnitSetMinimapIcon", "S2C_UnitSetSpellPARCost", "S2C_UpdateAttackSpeedCapOverrides",
            "S2C_UpdateDeathTimer", "SetFadeOut_Pop", "SetFadeOut_Push", "SpawnLevelPropS2C", "SpawnMinionS2C",
            "SwapItemAns", "SyncMissionStartTimeS2C", "SyncSimTimeFinalS2C", "SynchSimTimeS2C", "SynchVersionS2C",
            "TeamRosterUpdate", "UnitAddEXP", "UnitAddGold", "UnitApplyDamage", "UpdateGoldRedirectTarget",
            "UpdateLevelPropS2C", "UseItemAns", "WaypointGroup", "WaypointGroupWithSpeed", "World_SendGameNumber"
        };
    
    public static async Task<List<string>> Filters()
    {
        return _filters;
    }
}