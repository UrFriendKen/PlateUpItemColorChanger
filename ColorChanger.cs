using KitchenItemColorChanger.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenItemColorChanger
{
    public class ColorChanger : ManagedMonoBehaviour
    {
        private string ID = null;

        public Transform Container;

        Dictionary<string, Material[]> _materials;

        Dictionary<string, Color?[]> _defaultColors;

        protected virtual void Start()
        {
            ColorChangerWindow.OnColorPreferenceChanged += ColorPreferenceChangedHandler;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ColorChangerWindow.OnColorPreferenceChanged -= ColorPreferenceChangedHandler;
        }

        protected virtual void PopulateRenderers()
        {
            if (_materials == null)
                _materials = new Dictionary<string, Material[]>();
            if (_defaultColors == null)
                _defaultColors = new Dictionary<string, Color?[]>();

            _materials.Clear();
            _defaultColors.Clear();

            foreach (MeshRenderer meshRenderer in transform.GetComponentsInChildren<MeshRenderer>())
            {
                Transform rendererTransform = meshRenderer.transform;
                Transform stopAt = Container;

                string path = rendererTransform.GetPath(stopAt);

                Material[] materials = meshRenderer.materials;
                Color?[] defaultColors = new Color?[materials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];
                    RegisterDisposable(material);
                    defaultColors[i] = material.TryGetColor(out Color matColor) ? matColor : null;
                }
                _materials[path] = materials;
                _defaultColors[path] = defaultColors;
            }
        }

        protected virtual void ColorPreferenceChangedHandler(object sender, string id)
        {
            if (string.IsNullOrEmpty(ID) || ID != id)
                return;

            UpdateColors();
        }

        public virtual void UpdateID(string id)
        {
            if (ID == id)
                return;

            ID = id;

            PopulateRenderers();
            UpdateColors();
        }

        protected virtual void UpdateColors()
        {
            if (string.IsNullOrEmpty(ID))
                return;

            foreach (KeyValuePair<string, Material[]> kvp in _materials)
            {
                if (kvp.Key == null || kvp.Value == null)
                    continue;

                for (int i = 0; i < kvp.Value.Length; i++)
                {
                    if (!kvp.Value[i].SupportsColorChange())
                        continue;

                    Color color;
                    string prefKey = $"{ID}/{kvp.Key}/{i}";
                    if (Main.PrefManager.TryGet(prefKey, out string prefValue) && ColorUtility.TryParseHtmlString(prefValue, out Color prefColor))
                        color = prefColor;
                    else if (_defaultColors.TryGetValue(kvp.Key, out Color?[] defaultColors) && defaultColors[i].HasValue)
                        color = defaultColors[i].Value;
                    else
                        continue;

                    kvp.Value[i].TrySetColor(color);
                }
            }
        }
    }
}
