using HarmonyLib;
using System.Reflection;

public class ElectricityNoWires : IModApi
{

    // ####################################################################
    // ####################################################################

    public void InitMod(Mod mod)
    {
        Log.Out("OCB Harmony Patch: " + GetType().ToString());
        Harmony harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    // ####################################################################
    // Patch save-file loading to instantiate our new `TileEntity`
    // ####################################################################

    [HarmonyPatch(typeof (TileEntity), "Instantiate")]
    public class TileEntity_Instantiate
    {
        static bool Prefix(ref TileEntity __result, TileEntityType type, Chunk _chunk)
        {
            if (type == BlockButtonPush.TileEntityType)
            {
                __result = new TileEntityButtonPush(_chunk);
                return false;
            }
            return true;
        }
    }

    // ####################################################################
    // ####################################################################

    [HarmonyPatch(typeof(PowerItem), "CreateItem")]
    public class PowerItem_CreateItem
    {
        public static bool Prefix(PowerItem.PowerItemTypes itemType, ref PowerItem __result)
        {
            if (itemType == BlockButtonPush.PowerItemType)
            {
                __result = new PowerPushButton();
                return false;
            }
            return true;
        }
    }

    // ####################################################################
    // ####################################################################

}
