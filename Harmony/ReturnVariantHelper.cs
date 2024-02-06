using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

public static class UpgradeVariantHelperPatch
{

    // ####################################################################
    // ####################################################################

    // Used by patched function below
    static void UpgradeVariantHelper(ItemStack stack)
    {
        // Check if we are dealing with a block
        if (stack.itemValue.type < Block.ItemsStartHere)
        {
            // Check if the block has `ReturnVariantHelper` set
            if (Block.list[stack.itemValue.type].Properties.Values
                .TryGetString("ReturnVariantHelper", out string variant))
            {
                // Upgrade `itemValue` to variant helper block type
                if (Block.GetBlockByName(variant) is Block helper)
                    stack.itemValue = new ItemValue(helper.blockID);
            }
        }
    }

    // ####################################################################
    // ####################################################################

    [HarmonyPatch(typeof(BlockPowered), "EventData_Event")]
    public static class BlockPowered_EventData_Event
    {

        static IEnumerable<CodeInstruction> Transpiler
            (IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            bool searchFirstMarker = true;

            for (var i = 0; i < codes.Count; i++)
            {
                if (searchFirstMarker)
                {
                    if (codes[i].opcode == OpCodes.Call)
                    {
                        // Simply compare to string representation (any better way?)
                        if (codes[i].operand.ToString().StartsWith("ItemValue ToItemValue("))
                        {
                            searchFirstMarker = false;
                        }
                    }
                }
                else if (codes[i].opcode == OpCodes.Stloc_S)
                {
                    // Create the new OpCodes to be inserted
                    var op1 = new CodeInstruction(OpCodes.Ldloc_S, codes[i].operand);
                    var op2 = CodeInstruction.Call(typeof(UpgradeVariantHelperPatch), "UpgradeVariantHelper");
                    if (i + 2 < codes.Count)
                    {
                        // Check if the code has already been patched by us?
                        if (codes[i + 1].opcode == op1.opcode && codes[i + 1].operand == op1.operand)
                        {
                            // Do some heuristics as we may not reference the same function call
                            if (codes[i + 2].opcode == OpCodes.Call && codes[i + 2].operand.ToString().Contains("UpgradeVariantHelper"))
                            {
                                break;
                            }
                        }
                    }
                    // Insert new code line
                    codes.Insert(i + 1, op1);
                    codes.Insert(i + 2, op2);
                    // Finished patching
                    break;
                }
            }
            return codes;
        }
    }

    // ####################################################################
    // ####################################################################

}
