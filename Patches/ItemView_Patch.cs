using HarmonyLib;
using Kitchen;

namespace KitchenItemColorChanger.Patches
{
    [HarmonyPatch]
    static class ItemView_Patch
    {
        [HarmonyPatch(typeof(ItemView), "UpdateData")]
        [HarmonyPostfix]
        static void UpdateData_Postfix(ItemView __instance, ItemView.ViewData view_data)
        {
            ColorChanger colorChanger = __instance.GetComponent<ColorChanger>();
            if (!colorChanger)
                return;

            colorChanger.UpdateID(view_data.ItemID.ToString());
        }
    }
}
