using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Services.FurnitureConstructor.Data
{
    public class FurnitureFactory
    {
        private const string Morph = "morph";
        private const string Height = "height";
        private const string Width = "width";
        private const string Depth = "depth";

        private readonly TextureData.TextureData _textureData;
        private readonly Database _database;
        private Modifier.Modifier _modifier;

        public FurnitureFactory()
        {
            var furnitureLoader = new FurnitureLoader();
            _database = furnitureLoader.LoadDatabase();

            _modifier = new Modifier.Modifier();
        }

        public Furniture CreateFurniture(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("Prefab is null");
                return null;
            }

            var furnitureName = prefab.name;

            var furnitureObject = FindFurnitureObject(furnitureName, _database);
            if (furnitureObject == null)
            {
                Debug.LogWarning($"Furniture '{furnitureName}' not found in database.");
                return null;
            }

            var data = new FurnitureData {name = furnitureObject.name, start = furnitureObject.start};

            PopulateDataFromDatabase(furnitureObject, data);
            SaveMorphUVs(prefab.transform, data);
            RemoveMorphElements(prefab.transform);

            var furnitureComponent = prefab.AddComponent<Furniture>();
            furnitureComponent.Initialize(prefab, data, _modifier);

            return furnitureComponent;
        }

        private FurnitureObject FindFurnitureObject(string name, Database database)
        {
            foreach (var category in database.modelsDB)
            {
                foreach (var obj in category.objects)
                {
                    if (obj.name == name)
                        return obj;
                }
            }

            return null;
        }

        private void PopulateDataFromDatabase(FurnitureObject furnitureObject, FurnitureData data)
        {
            foreach (var morph in furnitureObject.morph)
            {
                data.parts[morph.label] = new PartData {morphInfo = morph};
            }

            foreach (var material in furnitureObject.materials)
            {
//                Debug.Log($"Processing material: {material.label}");

                if (material.typeInfo != null)
                {
                    //Debug.Log($"Material '{material.label}' has {material.typeInfo.Count} typeInfo entries.");

                    foreach (var type in material.typeInfo)
                    {
                        Texture2D currentTexture = null;
                        if (!string.IsNullOrEmpty(type.texture))
                        {
                            var cleanedPath = CleanTexturePath(type.texture);
                            currentTexture = Resources.Load<Texture2D>(cleanedPath);
                        }

//                        Debug.Log(
                        //                          $"TypeInfo for material '{material.label}': Label: {type.label}, NameInModel: {type.name_in_model}, Texture: {type.texture}");

                        var materialInfo = new MaterialInfo
                        {
                            label = material.label,
                            nameInModel = material.name_in_model,
                            texture = currentTexture
                        };

                        data.AddMaterial(material.label, materialInfo);
                    }
                }
            }

            if (furnitureObject.styles != null)
            {
                foreach (var style in furnitureObject.styles)
                {
                    if (style.typeInfo != null)
                    {
                        foreach (var type in style.typeInfo)
                        {
                            var styleInfo = new StyleInfo
                            {
                                label = type.label,
                                nameInModel = type.name_in_model
                            };

                            string styleKey = style.types;
                            data.AddStyle(style.label, styleKey, styleInfo);

//                            Debug.Log(
                            //                              $"Added StyleInfo: Key: {styleKey}, Label: {styleInfo.label}, NameInModel: {styleInfo.nameInModel}");
                        }
                    }
                }
            }
        }

        private string CleanTexturePath(string path)
        {
            if (path.StartsWith("/"))
                path = path.Substring(1);

            if (path.EndsWith(".jpg") || path.EndsWith(".png"))
                path = path.Substring(0, path.LastIndexOf('.'));

            return path;
        }

        private void SaveMorphUVs(Transform parent, FurnitureData data)
        {
            foreach (Transform child in parent)
            {
                if (child.name.ToLower().Contains(Morph))
                {
                    if (child.childCount > 0)
                    {
                        foreach (Transform subChild in child)
                        {
                            SaveUV(subChild, data);
                        }
                    }
                    else
                    {
                        SaveUV(child, data);
                    }
                }
            }
        }

        private void SaveUV(Transform obj, FurnitureData data)
        {
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                string name = obj.name;
                MorphType? morphType = GetMorphType(name);
                if (morphType.HasValue)
                {
                    data.AddUV(morphType.Value, name, meshFilter.sharedMesh.uv);
                    // Debug.Log($"Saved UV for {name} with type {morphType.Value}");
                }
            }
        }

        private MorphType? GetMorphType(string name)
        {
            if (name.ToLower().Contains(Height)) return MorphType.Height;
            if (name.ToLower().Contains(Width)) return MorphType.Width;
            if (name.ToLower().Contains(Depth)) return MorphType.Depth;
            return null;
        }

        private void RemoveMorphElements(Transform parent)
        {
            var morphElements = new List<Transform>();

            foreach (Transform child in parent)
            {
                if (child.name.ToLower().Contains(Morph))
                {
                    morphElements.Add(child);
                }
            }

            foreach (Transform morphElement in morphElements)
            {
                Object.Destroy(morphElement.gameObject);
            }
        }
    }
}