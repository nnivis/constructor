using UnityEngine;
using CodeBase.Services.FurnitureConstructor.Data;
using System.Collections.Generic;

namespace CodeBase.Services.FurnitureConstructor.Modifier
{
    public class SizeModifier
    {
        private float[] _sizes;
        private float[] _influences;

        private Dictionary<SkinnedMeshRenderer, Vector2[]> _originalUVsDict;

        public void InitializeSize(FurnitureData data, GameObject prefab)
        {
            _sizes = new float[3]
            {
                FindMorph(data, MorphType.Height)?.min ?? 0.72f,
                FindMorph(data, MorphType.Width)?.min ?? 1.52f,
                FindMorph(data, MorphType.Depth)?.min ?? 0.91f
            };

            SaveOriginalUVs(prefab);

            UpdateInfluences(data, prefab);
            UpdateUVs(data, prefab);
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
                UpdateUVs(data, prefab);
            }
        }

        private void SaveOriginalUVs(GameObject prefab)
        {
            _originalUVsDict = new Dictionary<SkinnedMeshRenderer, Vector2[]>();
            var renderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var renderer in renderers)
            {
                if (renderer.sharedMesh != null)
                {
                    _originalUVsDict[renderer] = (Vector2[]) renderer.sharedMesh.uv.Clone();
                }
            }
        }

        private void UpdateUVs(FurnitureData data, GameObject prefab)
        {
            var renderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var renderer in renderers)
            {
                if (renderer.sharedMesh != null && _originalUVsDict.TryGetValue(renderer, out var originalUVs))
                {
                    Vector2[] newUVs = (Vector2[]) originalUVs.Clone();

                    foreach (MorphType type in System.Enum.GetValues(typeof(MorphType)))
                    {
                        int index = MorphTypeToIndex(type);
                        float influence = _influences[index];

                        if (influence == 0f)
                            continue;

                        string objectName = renderer.gameObject.name;
                        Vector2[] morphUVs = data.GetUV(type, objectName);

                        if (morphUVs != null && morphUVs.Length == newUVs.Length)
                        {
                            for (int i = 0; i < newUVs.Length; i++)
                            {
                                newUVs[i] += (morphUVs[i] - originalUVs[i]) * influence;
                            }
                        }
                        else
                        {
                          //  Debug.LogWarning($"Morph UVs not found or length mismatch for {type} on {objectName}");
                        }
                    }


                    Mesh updatedMesh = Object.Instantiate(renderer.sharedMesh);
                    updatedMesh.name = renderer.sharedMesh.name + "_UpdatedUVs";
                    updatedMesh.uv = newUVs;
                    updatedMesh.RecalculateBounds();

                    // Назначаем обновленный Mesh
                    renderer.sharedMesh = updatedMesh;
                }
            }
        }

        private void UpdateInfluences(FurnitureData data, GameObject prefab)
        {
            _influences = new float[3];
            for (int i = 0; i < 3; i++)
                _influences[i] = data.SizeToInfluence(_sizes[i], data.start);

            UpdateMorphInfluences(prefab);
        }

        private void UpdateMorphInfluences(GameObject prefab)
        {
            var renderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var skinnedRenderer in renderers)
            {
                if (skinnedRenderer.sharedMesh != null)
                {
                    var blendShapeCount = skinnedRenderer.sharedMesh.blendShapeCount;
                    if (blendShapeCount >= 3)
                    {
                        for (int i = 0; i < 3; i++)
                            skinnedRenderer.SetBlendShapeWeight(i, _influences[i]);
                    }
                }
            }
        }

        private Morph FindMorph(FurnitureData data, MorphType type)
        {
            foreach (var part in data.Parts.Values)
            {
                if (part.morphInfo != null &&
                    part.morphInfo.label.Equals(type.ToString(), System.StringComparison.OrdinalIgnoreCase))
                    return part.morphInfo;
            }

            return null;
        }

        private int MorphTypeToIndex(MorphType type)
        {
            return type switch
            {
                MorphType.Height => 0,
                MorphType.Width => 1,
                MorphType.Depth => 2,
                _ => -1
            };
        }
    }
}