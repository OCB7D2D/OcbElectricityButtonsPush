using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

public class ElectricityNoWires : IModApi
{
    public void InitMod(Mod mod)
    {
        Debug.Log("Loading OCB Push Circuit Patch: " + GetType().ToString());
        var harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    [HarmonyPatch(typeof(TileEntity))]
    [HarmonyPatch("Instantiate")]
    public class TileEntity_Instantiate
    {
        public static bool Prefix(
            ref TileEntity __result,
            TileEntityType type,
            Chunk _chunk)
        {
            if (type == TileEntityButtonPush.Type)
            {
                __result = new TileEntityButtonPush(_chunk);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PowerItem))]
    [HarmonyPatch("CreateItem")]
    public class PowerItem_CreateItem
    {
        public static bool Prefix(
            PowerItem.PowerItemTypes itemType,
            ref PowerItem __result)
        {
            if (itemType == PowerPushButton.Type)
            {
                __result = new PowerPushButton();
                return false;
            }
            return true;
        }
    }


    // Make sure we always lock root item
    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("TELockServer")]
    public class GameManager_TELockServer
    {
        public static void Prefix(
            World ___m_World,
            int _clrIdx,
            ref Vector3i _blockPos,
            int _lootEntityId)
        {
            if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            {
                TileEntity entity = _lootEntityId != -1 ?
                    ___m_World.GetTileEntity(_lootEntityId) :
                    ___m_World.GetTileEntity(_clrIdx, _blockPos);
                if (!(entity is TileEntityButtonPush te)) return;
                if (!(te.GetRootCircuitItem() is PowerPushButton root)) return;
                if (root.TileEntity != null) _blockPos = root.Position;
            }
        }
    }

    [HarmonyPatch(typeof(XUiC_PowerTriggerOptions))]
    [HarmonyPatch("OnOpen")]
    public class XUiC_PowerTriggerOptions_OnOpen
    {
        public static void Postfix(
            TileEntity ___tileEntity,
            XUiController ___pnlTargeting)
        {
            if (___tileEntity is TileEntityButtonPush) {
                ___pnlTargeting.ViewComponent.IsVisible = false;
            } else {
                ___pnlTargeting.ViewComponent.IsVisible = true;
            }
        }
    }


    [HarmonyPatch(typeof(PowerManager))]
    [HarmonyPatch("Update")]
    public class PowerManager_Update
    {
        public static bool Prefix(
            PowerManager __instance,
            ref float ___updateTime,
            ref List<PowerSource> ___PowerSources,
            ref List<PowerTrigger> ___PowerTriggers
            )
        {
            if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && GameManager.Instance.gameStateManager.IsGameStarted())
            {
                ___updateTime -= Time.deltaTime;
                if ((double)___updateTime <= 0.0)
                {
                    for (int index = 0; index < ___PowerSources.Count; ++index)
                        ___PowerSources[index].Update();
                    for (int index = 0; index < ___PowerTriggers.Count; ++index)
                        ___PowerTriggers[index].CachedUpdateCall();
                    ___updateTime = 4.16f;
                }
            }
            for (int index = 0; index < __instance.ClientUpdateList.Count; ++index)
                __instance.ClientUpdateList[index].ClientUpdate();
            return false;
        }
    }

    // Resets duration when triggered again
    // Copied from ElectricityWorkarounds
    [HarmonyPatch(typeof(PowerTrigger))]
    [HarmonyPatch("set_IsTriggered")]
    public class PowerTrigger_SetIsTriggered
    {
        static void Postfix(PowerTrigger __instance,
            float ___delayStartTime,
            ref bool ___isActive,
            ref float ___lastPowerTime,
            ref float ___powerTime)
        {
            if (__instance.TriggerType != PowerTrigger.TriggerTypes.Switch)
            {
                if (___delayStartTime == -1.0)
                {
                    ___isActive = true;
                    ___lastPowerTime = Time.time;
                    // Had to copy `SetupDurationTime` due to protection
                    // This way we keep the patch EAC compatible (I guess)
                    switch (__instance.TriggerPowerDuration)
                    {
                        case PowerTrigger.TriggerPowerDurationTypes.Always:
                            ___powerTime = -1f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.Triggered:
                            ___powerTime = 0.0f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.OneSecond:
                            ___powerTime = 1f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.TwoSecond:
                            ___powerTime = 2f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.ThreeSecond:
                            ___powerTime = 3f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.FourSecond:
                            ___powerTime = 4f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.FiveSecond:
                            ___powerTime = 5f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.SixSecond:
                            ___powerTime = 6f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.SevenSecond:
                            ___powerTime = 7f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.EightSecond:
                            ___powerTime = 8f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.NineSecond:
                            ___powerTime = 9f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.TenSecond:
                            ___powerTime = 10f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.FifteenSecond:
                            ___powerTime = 15f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.ThirtySecond:
                            ___powerTime = 30f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.FourtyFiveSecond:
                            ___powerTime = 45f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.OneMinute:
                            ___powerTime = 60f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.FiveMinute:
                            ___powerTime = 300f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.TenMinute:
                            ___powerTime = 600f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.ThirtyMinute:
                            ___powerTime = 1800f;
                            break;
                        case PowerTrigger.TriggerPowerDurationTypes.SixtyMinute:
                            ___powerTime = 3600f;
                            break;
                    }
                }
            }
        }
    }

}
