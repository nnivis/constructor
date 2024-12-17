using System.Linq;
using CodeBase.Services.FurnitureConstructor.Data;
using UnityEngine;

namespace CodeBase.Services.FurnitureConstructor.Modifier 
{
    public class MaterialModifier 
    {
        public void InitializeMaterial(FurnitureData data, GameObject prefab) 
        {
            if (data == null || prefab == null)
            {
                return;
            }

            foreach (var part in data.parts.Values) 
            {
                foreach (var materialGroup in part.materials.GroupBy(m => m.label)) 
                {
                    var firstMaterial = materialGroup.FirstOrDefault();

                    if (firstMaterial == null)
                    {
                        continue;
                    }

                    ApplyMaterialToPrefab(prefab, firstMaterial.nameInModel, firstMaterial.texture);
                }
            }
        }

        public void SetMaterialByLabel(FurnitureData data, GameObject prefab, string label, string textureName)
        {
            if (data == null || prefab == null)
            {
                return;
            }

            var targetMaterial = data.parts.Values
                .SelectMany(part => part.materials)
                .FirstOrDefault(m => m.label == label && m.texture?.name == textureName);

            if (targetMaterial == null)
            {
                return;
            }

            ApplyMaterialToPrefab(prefab, targetMaterial.nameInModel, targetMaterial.texture);
        }

        private void ApplyMaterialToPrefab(GameObject prefab, string nameInModel, Texture2D texture)
        {
            var renderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            foreach (var renderer in renderers)
            {
                if (renderer.name.Contains(nameInModel))
                {
                    if (renderer.sharedMaterial != null)
                    {
                        Material materialCopy = Object.Instantiate(renderer.sharedMaterial);

                        if (texture != null)
                        {
                            materialCopy.SetTexture("baseColorTexture", texture);
                            // Debug.Log($"Applied texture '{texture.name}' to {renderer.name}");
                        }

                        renderer.sharedMaterial = materialCopy;
                    }
                }
            }
        }
    }
}
