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

        private readonly Database _database;
        private readonly Modifier.Modifier _modifier;

        public FurnitureFactory()
        {
            var furnitureLoader = new FurnitureLoader();
            _database = furnitureLoader.LoadDatabase();
            _modifier = new Modifier.Modifier();
        }

        public Furniture CreateFurniture(GameObject prefab)
        {
            if (prefab == null) return null;

            var furnitureName = prefab.name;
            var furnitureObject = FindFurnitureObject(furnitureName, _database);

            if (furnitureObject == null) return null;

            var data = new FurnitureData
            {
                name = furnitureObject.name,
                start = furnitureObject.start
            };

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
                var obj = category.objects.Find(o => o.name == name);
                if (obj != null) return obj;
            }
            return null;
        }

        private void PopulateDataFromDatabase(FurnitureObject furnitureObject, FurnitureData data)
        {
            foreach (var morph in furnitureObject.morph)
            {
                data.parts[morph.label] = new PartData { morphInfo = morph };
            }

            foreach (var material in furnitureObject.materials)
            {
                if (material.typeInfo == null) continue;

                foreach (var type in material.typeInfo)
                {
                    if (!string.IsNullOrEmpty(type.texture))
                    {
                        data.AddMaterial(
                            material.label,
                            new MaterialInfo
                            {
                                label = material.label,
                                nameInModel = material.name_in_model,
                                texture = Resources.Load<Texture2D>(CleanTexturePath(type.texture))
                            }
                        );
                    }
                }
            }

            if (furnitureObject.styles == null) return;

            foreach (var style in furnitureObject.styles)
            {
                if (style.typeInfo == null) continue;

                foreach (var type in style.typeInfo)
                {
                    data.AddStyle(
                        style.label,
                        style.types,
                        new StyleInfo
                        {
                            label = type.label,
                            nameInModel = type.name_in_model
                        }
                    );
                }
            }
        }

        private string CleanTexturePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;

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
                if (!child.name.ToLower().Contains(Morph)) continue;

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

        private void SaveUV(Transform obj, FurnitureData data)
        {
            var meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter?.sharedMesh == null) return;

            string name = obj.name;
            MorphType? morphType = GetMorphType(name);
            if (morphType.HasValue)
            {
                data.AddUV(morphType.Value, name, meshFilter.sharedMesh.uv);
            }
        }

        private MorphType? GetMorphType(string name)
        {
            var lowerName = name.ToLower();
            if (lowerName.Contains(Height)) return MorphType.Height;
            if (lowerName.Contains(Width)) return MorphType.Width;
            if (lowerName.Contains(Depth)) return MorphType.Depth;
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
