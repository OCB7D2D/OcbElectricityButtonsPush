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

    [HarmonyPatch(typeof (TileEntity))]
    [HarmonyPatch("Instantiate")]
    public class TileEntity_Instantiate
    {
        public static bool
        Prefix(ref TileEntity __result, TileEntityType type, Chunk _chunk)
        {
            if (type == (TileEntityType) 243)
            {
                __result = new TileEntityButtonPush(_chunk);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof (PowerTrigger))]
    [HarmonyPatch("HandleDisconnectChildren")]
    public class PowerTrigger_HandleDisconnect
    {
        public static void Postfix(PowerTrigger __instance)
        {
            if (__instance.TileEntity is TileEntityButtonPush pushbtn) {
                if (GameManager.IsDedicatedServer) {
                    pushbtn.SetModified();
                }
                else {
                    pushbtn.UpdateEmissionColor(null);
                }
            }
        }
    }

    [HarmonyPatch(typeof (XUiC_PowerTriggerOptions))]
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
