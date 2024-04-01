using System.Collections.Generic;
using UnityEngine;

namespace KitchenItemColorChanger.Extensions
{
    internal static class MaterialExtensions
    {
        static readonly Dictionary<string, string> _shaderColorKeys = new Dictionary<string, string>()
        {
            { "Simple Flat", "_Color0" },
            { "Simple Flat Transparent", "_Color0" },
            { "Lake Surface", "_Color0" },
            { "Simple Transparent", "_Color" },
        };

        public static bool SupportsShaderColorChange(Shader shader)
        {
            return _shaderColorKeys.ContainsKey(shader?.name);
        }

        public static bool SupportsColorChange(this Material material)
        {
            return SupportsShaderColorChange(material?.shader);
        }

        private static bool TryGetColorPropertyName(this Material material, out string propertyName)
        {
            if (material == null ||
                !_shaderColorKeys.TryGetValue(material.shader.name, out propertyName))
            {
                propertyName = default;
                return false;
            }
            return true;
        }

        public static bool TryGetColor(this Material material, out Color color)
        {
            if (!TryGetColorPropertyName(material, out string colorPropertyName))
            {
                color = default;
                return false;
            }
            color = material.GetColor(colorPropertyName);
            return true;
        }

        public static bool TrySetColor(this Material material, Color color)
        {
            if (!TryGetColorPropertyName(material, out string colorPropertyName))
                return false;

            material.SetColor(colorPropertyName, color);
            return true;
        }
    }
}
