using CodeBase.Services.FurnitureConstructor.Data;
using UnityEngine;

namespace CodeBase.Services.FurnitureConstructor.Modifier
{
    public class SizeModifier
    {
        private float[] _sizes;
        private float[] _influences;
        
        public void InitializeSize(FurnitureData data, GameObject prefab)
        {
            Morph heightMorph = FindMorph(data, MorphType.Height);
            Morph widthMorph = FindMorph(data, MorphType.Width);
            Morph depthMorph = FindMorph(data, MorphType.Depth);

            float height = heightMorph != null ? heightMorph.min : 0.72f;
            float width = widthMorph != null ? widthMorph.min : 1.52f;
            float depth = depthMorph != null ? depthMorph.min : 0.91f;

            _sizes = new float[3] {height, width, depth};

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
            }
        }

        private void UpdateInfluences(FurnitureData data, GameObject prefab)
        {
            _influences = new float[3];
            _influences[0] = data.SizeToInfluence(_sizes[0], data.start);
            _influences[1] = data.SizeToInfluence(_sizes[1], data.start);
            _influences[2] = data.SizeToInfluence(_sizes[2], data.start);

            UpdateMorphInfluences(prefab);
        }

        private void UpdateMorphInfluences(GameObject prefab)
        {
            var renderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var skinnedRenderer in renderers)
            {
                if (skinnedRenderer.sharedMesh != null)
                {
                    int blendShapeCount = skinnedRenderer.sharedMesh.blendShapeCount;
                    if (blendShapeCount >= 3)
                    {
                        //Debug.Log(
                          //  $"Applying BlendShapes: Height={_influences[0]}, Width={_influences[1]}, Depth={_influences[2]}");

                        skinnedRenderer.SetBlendShapeWeight(0, _influences[0]);
                        skinnedRenderer.SetBlendShapeWeight(1, _influences[1]);
                        skinnedRenderer.SetBlendShapeWeight(2, _influences[2]);
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
                {
                    return part.morphInfo;
                }
            }

            return null;
        }

        private int MorphTypeToIndex(MorphType type)
        {
            switch (type)
            {
                case MorphType.Height:
                    return 0;
                case MorphType.Width:
                    return 1;
                case MorphType.Depth:
                    return 2;
                default:
                    return -1;
            }
        }
    }
}