using System.Reflection;
using HarmonyLib;
using UnityEngine;

public class ElectricityNoWires : IModApi
{

    public void InitMod(Mod mod)
    {
        Debug.Log("Loading OCB Push Button Circuit Patch: " + GetType().ToString());
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
                __result = (TileEntity) new TileEntityButtonPush(_chunk);
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

}
