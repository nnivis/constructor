using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeBase.Services.FurnitureConstructor.Data
{
    public enum MorphType
    {
        Height,
        Width,
        Depth
    }

    [System.Serializable]
    public class FurnitureData
    {
        public string name;
        public float start;

        public Dictionary<string, PartData> parts = new Dictionary<string, PartData>();

        public Dictionary<MorphType, Dictionary<string, Vector2[]>> morphUVs =
            new Dictionary<MorphType, Dictionary<string, Vector2[]>>();

        public float SizeToInfluence(float size, float startValueInMeters)
        {
            float startInches = startValueInMeters * 39.37f;
            float inches = size * 39.37f;
            float influence = (startInches - inches) / (startInches - 300f);

            return influence;
        }


        public void AddUV(MorphType type, string objectName, Vector2[] uv)
        {
            if (!morphUVs.ContainsKey(type))
                morphUVs[type] = new Dictionary<string, Vector2[]>();

            morphUVs[type][objectName] = uv;
        }

        public Vector2[] GetUV(MorphType type, string objectName)
        {
            if (morphUVs.TryGetValue(type, out var uvDict) && uvDict.TryGetValue(objectName, out var uv))
            {
                return uv;
            }

            Debug.LogWarning($"UV not found for {objectName} with type {type}");
            return null;
        }

        public void AddMaterial(string partName, MaterialInfo material)
        {
            if (!parts.ContainsKey(partName))
                parts[partName] = new PartData();

            parts[partName].materials.Add(material);
        }

        public void AddStyle(string partName, string styleKey, StyleInfo style)
        {
            if (!parts.ContainsKey(partName))
            {
                parts[partName] = new PartData();
            }

            if (!parts[partName].styles.ContainsKey(styleKey))
            {
                parts[partName].styles[styleKey] = new List<StyleInfo>();
            }

            if (!parts[partName].styles[styleKey].Any(existingStyle =>
                    existingStyle.label == style.label &&
                    existingStyle.nameInModel == style.nameInModel))
            {
                parts[partName].styles[styleKey].Add(style);
//                Debug.Log($"Added new style: Key='{styleKey}', Label='{style.label}', NameInModel='{style.nameInModel}'.");
            }
            else
            {
                //              Debug.LogWarning($"Style already exists: Key='{styleKey}', Label='{style.label}', NameInModel='{style.nameInModel}'.");
            }
        }
    }

    [System.Serializable]
    public class PartData
    {
        public Morph morphInfo;
        public List<MaterialInfo> materials = new List<MaterialInfo>();
        public Dictionary<string, List<StyleInfo>> styles = new Dictionary<string, List<StyleInfo>>();
    }


    [System.Serializable]
    public class MaterialInfo
    {
        public string label;
        public string nameInModel;
        public Texture2D texture;
    }

    [System.Serializable]
    public class StyleInfo
    {
        public string label;
        public string nameInModel;
    }
}