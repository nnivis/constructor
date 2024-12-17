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

        private readonly FurnitureLoader _furnitureLoader;
        private readonly Modifier.Modifier _modifier;

        public FurnitureFactory(FurnitureLoader furnitureLoader)
        {
            _furnitureLoader = furnitureLoader;
            _modifier = new Modifier.Modifier();
        }

        public Furniture CreateFurniture(GameObject prefab)
        {
            if (prefab == null) return null;

            var data = _furnitureLoader.GetFurnitureData(prefab.name);
            if (data == null)
            {
                Debug.LogError($"Failed to get FurnitureData for: {prefab.name}");
                return null;
            }

            SaveMorphUVs(prefab.transform, data);
            RemoveMorphElements(prefab.transform);

            var furnitureComponent = prefab.AddComponent<Furniture>();
            furnitureComponent.Initialize(prefab, data, _modifier);

            return furnitureComponent;
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
