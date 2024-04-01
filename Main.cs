using HarmonyLib;
using KitchenData;
using KitchenItemColorChanger.Extensions;
using KitchenMods;
using KitchenUITools;
using PreferenceSystem;
using System.Reflection;
using UnityEngine;

// Namespace should have "Kitchen" in the beginning
namespace KitchenItemColorChanger
{
    public class Main : IModInitializer
    {
        public const string MOD_GUID = $"IcedMilo.PlateUp.{MOD_NAME}";
        public const string MOD_NAME = "Item Color Changer";
        public const string MOD_VERSION = "0.1.0";

        internal static PreferenceSystemManager PrefManager;

        private ColorChangerWindow colorEditorWindow;

        public Main()
        {
            new Harmony(MOD_GUID).PatchAll(Assembly.GetExecutingAssembly());
        }

        public void PostActivate(KitchenMods.Mod mod)
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
        }

        public void PreInject()
        {
            
        }

        public void PostInject()
        {
            PrefManager = new PreferenceSystemManager(MOD_GUID, MOD_NAME);

            PrefManager
                .AddLabel("Item Color Changer")
                .AddButton("Open Editor", delegate (int _)
                {
                    if (colorEditorWindow == null)
                        colorEditorWindow = UITools.RequestWindow<ColorChangerWindow>("Color Editor");
                    colorEditorWindow.Show();
                }, closeOnPress: true)
                .AddSpacer()
                .AddSpacer();

            int propCount = 0;
            foreach (Appliance appliance in GameData.Main.Get<Appliance>())
            {
                if (appliance.Prefab == null)
                    continue;
                propCount += AddRendererProperties(appliance.ID, appliance.Prefab);
            }

            foreach (Item item in GameData.Main.Get<Item>())
            {
                if (item.Prefab == null)
                    continue;
                propCount += AddRendererProperties(item.ID, item.Prefab);
            }

            PrefManager.RegisterMenu(PreferenceSystemManager.MenuType.PauseMenu);

            Main.LogInfo($"Total prop count = {propCount}");
        }

        private int AddRendererProperties(int gdoID, GameObject gameObject, bool shouldClone = true)
        {
            int propCount = 0;
            Transform transform = gameObject.transform;
            Main.LogInfo($"{gdoID}");
            foreach (MeshRenderer meshRenderer in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                string[] pathParts = meshRenderer.transform.GetPath(transform, includeStopAt: true).Split('/');
                if (shouldClone)
                    pathParts[0] += "(Clone)";
                string path = string.Join("/", pathParts);
                Material[] sharedMaterials = meshRenderer.sharedMaterials;
                for (int i = 0; i < sharedMaterials.Length; i++)
                {
                    if (!MaterialExtensions.SupportsShaderColorChange(sharedMaterials[i]?.shader))
                        continue;

                    propCount++;

                    string prefKey = $"{gdoID}/{path}/{i}";
                    Main.LogInfo($"\t{prefKey}");

                    PrefManager.AddProperty(prefKey, $"Default");
                }
            }
            return propCount;
        }

        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }
}
