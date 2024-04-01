using HarmonyLib;
using Kitchen;
using UnityEngine;

namespace KitchenItemColorChanger.Patches
{
    [HarmonyPatch]
    static class LocalViewRouter_Patch
    {
        [HarmonyPatch(typeof(LocalViewRouter), "GetPrefab")]
        [HarmonyPostfix]
        static void GetPrefab_Postfix(ViewType view_type, ref GameObject __result)
        {
            if (view_type == ViewType.Appliance &&
                __result != default &&
                __result.GetComponent<ColorChanger>() == default)
            {
                ColorChanger colorChanger = __result.AddComponent<ColorChanger>();
                colorChanger.Container = __result.transform.Find("Container");
            }

            if (view_type == ViewType.Item &&
                __result != default &&
                __result.GetComponent<ColorChanger>() == default)
            {
                ColorChanger colorChanger = __result.AddComponent<ColorChanger>();
                colorChanger.Container = __result.transform;
            }
        }
    }
}
