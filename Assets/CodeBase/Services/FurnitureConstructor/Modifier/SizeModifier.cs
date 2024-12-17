using UnityEngine;
using CodeBase.Services.FurnitureConstructor.Data;

namespace CodeBase.Services.FurnitureConstructor.Modifier
{
    public class SizeModifier
    {
        private float[] _sizes;
        private float[] _influences;

        private Vector2[] _originalUVs; 

        public void InitializeSize(FurnitureData data, GameObject prefab) {
            _sizes = new float[3] {
                FindMorph(data, MorphType.Height)?.min ?? 0.72f, 
                FindMorph(data, MorphType.Width)?.min ?? 1.52f, 
                FindMorph(data, MorphType.Depth)?.min ?? 0.91f
            };

            SaveOriginalUVs(prefab);
            UpdateInfluences(data, prefab);
        }

        public void SetSize(FurnitureData data, GameObject prefab, MorphType type, float newValue)
        {
            if (_sizes == null || _sizes.Length < 3)
                return;

            int index = MorphTypeToIndex(type);
            if (index >= 0 && index < _sizes.Length)
            {
                _sizes[index] = newValue;
                UpdateInfluences(data, prefab);

                UpdateUVs(prefab, type, newValue);
            }
        }

        private void SaveOriginalUVs(GameObject prefab)
        {
            var renderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
            if (renderer != null && renderer.sharedMesh != null)
                _originalUVs = renderer.sharedMesh.uv;
        }

        private void UpdateUVs(GameObject prefab, MorphType type, float scale)
        {
            var renderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();

            if (renderer != null && _originalUVs != null) {
                var mesh = renderer.sharedMesh;
                var scaledUVs = new Vector2[_originalUVs.Length];

                for (var i = 0; i < _originalUVs.Length; i++) {
                    // Масштабируем UV в зависимости от изменяемой оси
                    scaledUVs[i] = _originalUVs[i];
                    scaledUVs[i] *= type switch
                    {
                        MorphType.Width => new Vector2(scale, 1),
                        MorphType.Height => new Vector2(1, scale),
                        MorphType.Depth => new Vector2(scale, scale),
                        _ => Vector2.one
                    };
                }

                mesh.uv = scaledUVs;
                renderer.sharedMesh = mesh;
            }
        }

        private void UpdateInfluences(FurnitureData data, GameObject prefab) {
            _influences = new float[3];
            for (int i = 0; i < 3; i++)
                _influences[i] = data.SizeToInfluence(_sizes[i], data.start);

            UpdateMorphInfluences(prefab);
        }

        private void UpdateMorphInfluences(GameObject prefab) {
            var renderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var skinnedRenderer in renderers) {
                if (skinnedRenderer.sharedMesh != null) {
                    var blendShapeCount = skinnedRenderer.sharedMesh.blendShapeCount;
                    if (blendShapeCount >= 3) {
                        for (int i = 0; i < 3; i++)
                            skinnedRenderer.SetBlendShapeWeight(i, _influences[i]);
                    }
                }
            }
        }

        private Morph FindMorph(FurnitureData data, MorphType type)
        {
            foreach (var part in data.parts.Values)
            {
                if (part.morphInfo != null &&
                    part.morphInfo.label.Equals(type.ToString(), System.StringComparison.OrdinalIgnoreCase))
                    return part.morphInfo;
            }

            return null;
        }

        private int MorphTypeToIndex(MorphType type) {
            return type switch {
                MorphType.Height => 0,
                MorphType.Width => 1,
                MorphType.Depth => 2,
                _ => -1
            };
        }
    }
}
