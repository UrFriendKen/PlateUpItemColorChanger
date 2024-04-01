using HarmonyLib;
using Kitchen;

namespace KitchenItemColorChanger.Patches
{
    [HarmonyPatch]
    static class ApplianceView_Patch
    {
        [HarmonyPatch(typeof(ApplianceView), "UpdateData")]
        [HarmonyPostfix]
        static void UpdateData_Postfix(ApplianceView __instance, ApplianceView.ViewData view_data)
        {
            ColorChanger colorChanger = __instance.GetComponent<ColorChanger>();
            if (!colorChanger)
                return;

            colorChanger.UpdateID(view_data.ApplianceID.ToString());
        }
    }
}
