using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Services.FurnitureConstructor.Data
{
    [Serializable]
    public class Database
    {
        public List<Category> modelsDB;
    }

    [Serializable]
    public class Category
    {
        public string category;
        public List<FurnitureData> objects;
    }

    [Serializable]
    public class FurnitureData
    {
        public string name;
        public string model;
        public float start;
        public List<Morph> morph;
        public List<CustomMaterial> materials;
        public List<Style> styles;

        [NonSerialized] public Dictionary<string, PartData> Parts = new Dictionary<string, PartData>();

        [NonSerialized] public Dictionary<MorphType, Dictionary<string, Vector2[]>> MorphUVs =
            new Dictionary<MorphType, Dictionary<string, Vector2[]>>();

        public float SizeToInfluence(float size, float startValueInMeters)
        {
            float startInches = startValueInMeters * 39.37f;
            return (startInches - size * 39.37f) / (startInches - 300f);
        }

        public void AddUV(MorphType type, string objectName, Vector2[] uv)
        {
            int morphIndex = objectName.IndexOf("-morph", StringComparison.Ordinal);
            if (morphIndex >= 0)
            {
                objectName = objectName.Substring(0, morphIndex);
            }

            if (!MorphUVs.ContainsKey(type))
                MorphUVs[type] = new Dictionary<string, Vector2[]>();

            MorphUVs[type][objectName] = uv;
        }

        public Vector2[] GetUV(MorphType type, string objectName)
        {
            if (MorphUVs.TryGetValue(type, out var uvDict) && uvDict.TryGetValue(objectName, out var uv))
            {
                return uv;
            }

            foreach (var key in uvDict.Keys)
            {
                if (key.StartsWith(objectName))
                {
                    Debug.Log($"Using partial match for {objectName} -> {key}");
                    return uvDict[key];
                }
            }

            Debug.LogWarning($"UV not found for {type} - Object: {objectName}");
            return null;
        }


        public void AddMaterial(string partName, MaterialInfo material)
        {
            if (!Parts.ContainsKey(partName))
                Parts[partName] = new PartData();

            Parts[partName].materials.Add(material);
        }

        public void AddStyle(string partName, string styleKey, StyleInfo style)
        {
            if (!Parts.ContainsKey(partName)) Parts[partName] = new PartData();

            if (!Parts[partName].styles.ContainsKey(styleKey))
                Parts[partName].styles[styleKey] = new List<StyleInfo>();

            if (!Parts[partName].styles[styleKey]
                    .Exists(s => s.label == style.label && s.nameInModel == style.nameInModel))
                Parts[partName].styles[styleKey].Add(style);
        }
    }

    [Serializable]
    public class Morph
    {
        public string label;
        public float min;
        public float max;
    }

    [Serializable]
    public class CustomMaterial
    {
        public string label, types, name_in_model;
        public List<TypeInfo> typeInfo;
        public Texture2D texture;
    }

    [Serializable]
    public class Style
    {
        public string label, types;
        public List<TypeInfo> typeInfo;
    }

    [Serializable]
    public class TypeInfo
    {
        public string label;
        public string name_in_model;
        public string texture;
    }

    [Serializable]
    public class PartData
    {
        public Morph morphInfo;
        public List<MaterialInfo> materials = new List<MaterialInfo>();
        public Dictionary<string, List<StyleInfo>> styles = new Dictionary<string, List<StyleInfo>>();
    }

    [Serializable]
    public class MaterialInfo
    {
        public string label;
        public string nameInModel;
        public string texturePath;
        public string textureLabel;
    }

    [Serializable]
    public class StyleInfo
    {
        public string label;
        public string nameInModel;
    }

    public enum MorphType
    {
        Height,
        Width,
        Depth
    }
}