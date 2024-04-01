using Kitchen;
using Kitchen.Layouts;
using KitchenData;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace KitchenItemColorChanger
{
    internal static class CustomPrefabSnapshot
    {
        private static float NightFade;

        private static readonly int Fade = Shader.PropertyToID("_NightFade");

        private static int CacheMaxSize = 20;

        private static Dictionary<int, Texture2D> _CachedImages = new Dictionary<int, Texture2D>();

        private static void CacheShaderValues()
        {
            NightFade = Shader.GetGlobalFloat(Fade);
            Shader.SetGlobalFloat(Fade, 0f);
        }

        private static void ResetShaderValues()
        {
            Shader.SetGlobalFloat(Fade, NightFade);
        }

        public static Texture2D GetSnapshot(GameObject prefab, float scale = 1f, int imageSize = 512)
        {
            Quaternion rotation = Quaternion.LookRotation(new Vector3(0f, 0f, 1f), Vector3.up);
            return GetSnapshot(prefab, default, rotation, scale, imageSize);
        }

        public static Texture2D GetSnapshot(GameObject prefab, Vector3 position, Quaternion rotation, float scale = 1f, int imageSize = 512)
        {
            CacheShaderValues();
            SnapshotTexture snapshotTexture = Snapshot.RenderPrefabToTexture(imageSize, imageSize, prefab, rotation, 0.5f, 0.5f, -10f, 10f, scale, position);
            ResetShaderValues();
            return snapshotTexture.Snapshot;
        }

        public static Texture2D GetCardSnapshot(UnlockCardElement element, ICard card, int width = 512, int height = 512)
        {
            int num = (card as UnlockCard)?.ID ?? (card as Dish)?.ID ?? 0;
            CacheShaderValues();
            element.SetUnlock(card);
            SnapshotTexture snapshotTexture = Snapshot.RenderToTexture(width, height, element.gameObject, 1f, 1f, -10f, 10f, element.transform.localPosition);
            ResetShaderValues();
            return snapshotTexture.Snapshot;
        }

        public static Texture2D GetItemSnapshot(GameObject prefab)
        {
            CacheShaderValues();
            GameObject gO = GameObject.Instantiate(prefab);
            DisableProcessesIcon(gO);
            DisableColorblindLabels(gO);
            Quaternion rotation = Quaternion.LookRotation(new Vector3(1f, -1f, 1f), new Vector3(0f, 1f, 1f));
            SnapshotTexture snapshotTexture = Snapshot.RenderPrefabToTexture(512, 512, gO, rotation, 0.5f, 0.5f);
            GameObject.Destroy(gO);
            ResetShaderValues();
            return snapshotTexture.Snapshot;
        }

        public static Texture2D GetApplianceSnapshot(GameObject prefab, Vector3 offset = default)
        {
            CacheShaderValues();
            GameObject gO = GameObject.Instantiate(prefab);
            DisableProcessesIcon(gO);
            DisableColorblindLabels(gO);
            Quaternion rotation = Quaternion.LookRotation(new Vector3(1f, -1f, 1f), new Vector3(0f, 1f, 1f));
            SnapshotTexture snapshotTexture = Snapshot.RenderPrefabToTexture(512, 512, gO, rotation, 0.5f, 0.5f, -10f, 10f, 0.5f, (-0.25f * new Vector3(0f, 1f, 1f)) + offset);
            GameObject.Destroy(gO);
            ResetShaderValues();
            return snapshotTexture.Snapshot;
        }

        public static Texture2D GetFoodSnapshot(GameObject prefab, ItemView.ViewData data)
        {
            GameObject gameObject = Object.Instantiate(prefab);
            ItemView component = gameObject.GetComponent<ItemView>();
            component.UpdateData(data);
            CacheShaderValues();
            Quaternion rotation = Quaternion.LookRotation(new Vector3(0f, -0.5f, 0.5f), Vector3.up);
            SnapshotTexture snapshotTexture = Snapshot.RenderPrefabToTexture(128, 128, gameObject, rotation, 0.5f, 0.5f);
            ResetShaderValues();
            Object.Destroy(gameObject);
            return snapshotTexture.Snapshot;
        }

        public static Texture2D GetLayoutSnapshot(SiteView prefab, LayoutBlueprint blueprint)
        {
            int iD = blueprint.ID;
            if (_CachedImages.TryGetValue(iD, out var value) && value != null)
            {
                return value;
            }

            SiteView siteView = Object.Instantiate(prefab);
            siteView.UpdateData(new SiteView.ViewData
            {
                Floorplan = blueprint
            });
            CacheShaderValues();
            Quaternion rotation = Quaternion.LookRotation(new Vector3(1f, 0f, 0f), Vector3.up);
            SnapshotTexture snapshotTexture = Snapshot.RenderPrefabToTexture(128, 128, siteView.gameObject, rotation, 1.5f, 1.5f);
            ResetShaderValues();
            Object.Destroy(siteView.GameObject);
            if (_CachedImages.Count > CacheMaxSize)
            {
                _CachedImages.Clear();
            }

            _CachedImages[iD] = snapshotTexture.Snapshot;
            return snapshotTexture.Snapshot;
        }

        private static void DisableProcessesIcon(GameObject gO)
        {
            foreach (GameObject child in gO.GetComponentsInChildren<ItemView>().Select(x => x.gameObject))
            {
                child.transform.Find("Processes Icon")?.gameObject.SetActive(false);
            }
        }

        private static void DisableColorblindLabels(GameObject gO)
        {
            foreach (ColourBlindMode colourBlindMode in gO.GetComponentsInChildren<ColourBlindMode>())
            {
                colourBlindMode.Element?.gameObject.SetActive(false);
            }
        }
    }
}
