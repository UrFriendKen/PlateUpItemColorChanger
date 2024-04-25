using KitchenData;
using KitchenItemColorChanger.Extensions;
using KitchenUITools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KitchenItemColorChanger
{
    public class ColorChangerWindow : BaseWindowController
    {
        internal static event EventHandler<string> OnColorPreferenceChanged;

        class EditableItem
        {

            public readonly string ID;
            public readonly string Name;
            public readonly GameObject Prefab;

            public EditableItem(string id, string name, GameObject prefab)
            {
                ID = id;
                Name = name;
                Prefab = prefab;
            }
        }

        List<EditableItem> Appliances;

        List<EditableItem> Items;

        void Start()
        {
            Appliances = GameData.Main.Get<Appliance>()
                .Where(appliance => appliance.Prefab != null &&
                    appliance.Prefab.GetComponentsInChildren<MeshRenderer>(includeInactive: true)
                        .SelectMany(meshRenderer => meshRenderer.sharedMaterials)
                        .Where(material => material.SupportsColorChange())
                        .Any())
                .Select(appliance => new EditableItem(appliance.ID.ToString(), GetGDOName(appliance), appliance.Prefab))
                .ToList();

            Items = GameData.Main.Get<Item>()
                .Where(item => item.Prefab != null &&
                    item.Prefab.GetComponentsInChildren<MeshRenderer>(includeInactive: true)
                        .SelectMany(meshRenderer => meshRenderer.sharedMaterials)
                        .Where(material => material.SupportsColorChange())
                        .Any())
                .Select(item => new EditableItem(item.ID.ToString(), GetGDOName(item), item.Prefab))
                .ToList();

            _instanceItems = Appliances;
        }

        List<EditableItem> _instanceItems;
        Vector2 _instanceListScrollPosition = Vector2.zero;
        string _instanceListFilter = string.Empty;

        EditableItem _selectedGDOItem = null;
        Texture _selectedGameObjectTexture = null;
        string _editListFilter = string.Empty;
        Vector2 _editScrollPosition = Vector2.zero;
        List<MaterialItem> _editableMaterials = new List<MaterialItem>();

        class MaterialItem
        {
            public readonly string PrefKey;
            public readonly string DisplayName;

            private const string ALLOWED_HEX_CHARS = "abcdef1234567890";

            private string _hexEntry = string.Empty;
            public string HexEntry
            {
                get
                {
                    return _hexEntry;
                }
                set
                {
                    if (_hexEntry == value)
                        return;
                    if (value == null)
                        value = string.Empty;
                    value = value.ToLowerInvariant();

                    string validString = string.Empty;
                    for (int i = 0; i < value.Length && validString.Length < 6; i++)
                    {
                        if (!ALLOWED_HEX_CHARS.Contains(value[i]))
                            continue;
                        validString += value[i].ToString();
                    }
                    _hexEntry = validString;
                }
            }

            float _defaultR;
            float _r;
            public float R
            {
                get { return _r; }
                set
                {
                    value = Mathf.Clamp01(value);
                    if (value == _r)
                        return;
                    _r = value;
                    HasChanged = true;
                }
            }
            float _defaultG;
            float _g;
            public float G
            {
                get { return _g; }
                set
                {
                    value = Mathf.Clamp01(value);
                    if (value == _g)
                        return;
                    _g = value;
                    HasChanged = true;
                }
            }
            float _defaultB;
            float _b;
            public float B
            {
                get { return _b; }
                set
                {
                    value = Mathf.Clamp01(value);
                    if (value == _b)
                        return;
                    _b = value;
                    HasChanged = true;
                }
            }

            public bool HasChanged { get; private set; } = false;

            public Color Color => new Color(R, G, B);
            public string HexColor => $"#{ColorUtility.ToHtmlStringRGB(Color)}";

            public MaterialItem(string prefKey, string displayName, Color color, Color defaultColor)
            {
                PrefKey = prefKey ?? string.Empty;
                DisplayName = displayName ?? string.Empty;

                _r = color.r;
                _g = color.g;
                _b = color.b;

                _defaultR = defaultColor.r;
                _defaultG = defaultColor.g;
                _defaultB = defaultColor.b;

                HexEntry = HexColor;
            }

            public void HandledChange()
            {
                HasChanged = false;
            }

            public void ApplyColor(Color color)
            {
                R = color.r;
                G = color.g;
                B = color.b;
            }

            public void Reset()
            {
                R = _defaultR;
                G = _defaultG;
                B = _defaultB;
            }
        }

        protected override void DrawWindowContent(int windowID)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Appliances"))
                _instanceItems = Appliances;
            if (GUILayout.Button("Items"))
                _instanceItems = Items;

            if (GUILayout.Button("Close", GUILayout.Width(WindowWidth * 0.1f)))
                Hide();
            GUILayout.EndHorizontal();

            if (_instanceItems == null)
            {
                return;
            }

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(WindowWidth * 0.3f));
            _instanceListFilter = GUILayout.TextField(_instanceListFilter.ToLowerInvariant());
            _instanceListScrollPosition = GUILayout.BeginScrollView(_instanceListScrollPosition);
            for (int i = 0; i < _instanceItems.Count; i++)
            {
                EditableItem gdoItem = _instanceItems[i];
                string gdoName = gdoItem.Name;

                if (!string.IsNullOrEmpty(_instanceListFilter) &&
                    !gdoName.ToLowerInvariant().Contains(_instanceListFilter))
                    continue;

                if (GUILayout.Button(gdoName))
                {
                    _selectedGDOItem = gdoItem;
                    _editScrollPosition = default;

                    RecaptureSnapshot();

                    PopulateEditableMaterials(_selectedGDOItem.ID, _selectedGDOItem.Prefab);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();


            if (_selectedGDOItem != null)
            {
                GUILayout.BeginVertical();
                if (_selectedGameObjectTexture)
                    GUILayout.Box(_selectedGameObjectTexture);

                _editListFilter = GUILayout.TextField(_editListFilter).ToLowerInvariant();

                _editScrollPosition = GUILayout.BeginScrollView(_editScrollPosition);

                bool resetAllPressed = GUILayout.Button("Reset All");

                GUILayoutOption actionButtonWidth = GUILayout.Width(Mathf.Max(0.1f * WindowWidth, 50f));

                bool resetPressed;
                bool applyPressed;
                Color hexColorToApply;
                foreach (MaterialItem materialItem in _editableMaterials)
                {
                    if (!string.IsNullOrEmpty(_editListFilter) &&
                        !materialItem.DisplayName.ToLowerInvariant().Contains(_editListFilter))
                        continue;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(materialItem.DisplayName);

                    string hexDisplayText = $"#{materialItem.HexEntry}";
                    materialItem.HexEntry = GUILayout.TextField(hexDisplayText, actionButtonWidth).Replace("#", "");
                    if (!ColorUtility.TryParseHtmlString(hexDisplayText, out hexColorToApply))
                        GUI.enabled = false;
                    applyPressed = GUILayout.Button("Apply Hex", actionButtonWidth);
                    GUI.enabled = true;
                    resetPressed = GUILayout.Button("Reset", actionButtonWidth);
                    GUILayout.EndHorizontal();
                    materialItem.R = GUILayout.HorizontalSlider(materialItem.R, 0f, 1f);
                    materialItem.G = GUILayout.HorizontalSlider(materialItem.G, 0f, 1f);
                    materialItem.B = GUILayout.HorizontalSlider(materialItem.B, 0f, 1f);

                    if (resetPressed || resetAllPressed)
                        materialItem.Reset();

                    if (applyPressed)
                        materialItem.ApplyColor(hexColorToApply);

                    if (materialItem.HasChanged)
                    {
                        materialItem.HandledChange();
                        Main.PrefManager.Set(materialItem.PrefKey, resetPressed ? "Default" : materialItem.HexColor);
                        materialItem.HexEntry = materialItem.HexColor;
                        RecaptureSnapshot();
                        OnColorPreferenceChanged.Invoke(this, _selectedGDOItem.ID);
                    }
                }
                GUILayout.EndScrollView();

                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private static string GetGDOName<T>(T gdo) where T : GameDataObject
        {
            if (gdo is Appliance appliance)
                return string.IsNullOrEmpty(appliance.Name) ? appliance.name : appliance.Name;
            return gdo?.name;
        }

        private void PopulateEditableMaterials(string id, GameObject gameObject)
        {
            _editableMaterials.Clear();
            Transform rootTransform = gameObject.transform;
            foreach (MeshRenderer meshRenderer in gameObject.GetComponentsInChildren<MeshRenderer>(includeInactive: true))
            {
                string[] pathParts = meshRenderer.transform.GetPath(rootTransform, includeStopAt: true).Split('/');
                pathParts[0] += "(Clone)";
                string rendererDisplayName = pathParts.Last();
                string path = string.Join("/", pathParts);

                Material[] materials = meshRenderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];
                    if (!(material?.SupportsColorChange() ?? false))
                        continue;

                    string prefKey = $"{id}/{path}/{i}";
                    string materialDisplayName = $"{rendererDisplayName} - Material {i + 1}";

                    if (!material.TryGetColor(out Color defaultColor))
                        continue;

                    Color color;
                    if (Main.PrefManager.TryGet(prefKey, out string prefValue) &&
                        ColorUtility.TryParseHtmlString(prefValue, out Color prefColor))
                        color = prefColor;
                    else
                        color = defaultColor;

                    _editableMaterials.Add(new MaterialItem(prefKey, materialDisplayName, color, defaultColor));
                }
            }
        }

        private void RecaptureSnapshot()
        {
            if (_selectedGameObjectTexture != null)
                Destroy(_selectedGameObjectTexture);
            
            if (_selectedGDOItem?.Prefab == null)
                return;

            string id = _selectedGDOItem.ID;
            GameObject instance = Instantiate(_selectedGDOItem.Prefab);
            List<Material> instanceMaterials = new List<Material>();
            try
            {
                Transform instanceTransform = instance.transform;

                foreach (MeshRenderer meshRenderer in instance.GetComponentsInChildren<MeshRenderer>(includeInactive: true))
                {
                    string meshRendererPath = $"{meshRenderer.transform.GetPath(instanceTransform, includeStopAt: true)}";
                    Material[] meshInstanceMaterials = meshRenderer.materials;
                    instanceMaterials.AddRange(meshInstanceMaterials);

                    for (int i = 0; i < meshInstanceMaterials.Length; i++)
                    {
                        Material meshMaterial = meshInstanceMaterials[i];
                        if (!(meshMaterial?.SupportsColorChange() ?? false))
                            continue;

                        string prefKey = $"{id}/{meshRendererPath}/{i}";

                        Color color;
                        if (!Main.PrefManager.TryGet(prefKey, out string prefValue) ||
                            !ColorUtility.TryParseHtmlString(prefValue, out Color prefColor))
                            continue;
                        color = prefColor;
                        meshMaterial.TrySetColor(color);
                    }
                }
                _selectedGameObjectTexture = CustomPrefabSnapshot.GetApplianceSnapshot(instance);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                Destroy(instance);
                for (int i = instanceMaterials.Count - 1; i > -1; i--)
                {
                    if (instanceMaterials[i] == null)
                        continue;
                    Destroy(instanceMaterials[i]);
                }
            }
        }
    }
}
