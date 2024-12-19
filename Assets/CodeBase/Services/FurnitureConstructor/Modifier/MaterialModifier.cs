using System.Linq;
using CodeBase.Services.FurnitureConstructor.Data;
using UnityEngine;

namespace CodeBase.Services.FurnitureConstructor.Modifier
{
    public class MaterialModifier
    {
        private const int I = 4;

        public void InitializeMaterial(FurnitureData data, GameObject prefab)
        {
            if (data == null || prefab == null)
                return;

            foreach (var part in data.Parts.Values)
            {
                foreach (var materialGroup in part.materials.GroupBy(m => m.label))
                {
                    var firstMaterial = materialGroup.FirstOrDefault();
                    if (firstMaterial == null)
                        continue;

                    ApplyMaterialToPrefab(prefab, data, firstMaterial.nameInModel, firstMaterial.texturePath);
                }
            }
        }

        public void SetMaterialByLabel(FurnitureData data, GameObject prefab, string label, string textureName)
        {
            if (data == null || prefab == null)
            {
                return;
            }

            MaterialInfo targetMaterial = data.Parts.Values
                .SelectMany(part => part.materials)
                .FirstOrDefault(m => m.label == label && m.textureLabel == textureName);

            if (targetMaterial == null)
            {
                return;
            }

            string texturePath = targetMaterial.texturePath;
            if (string.IsNullOrEmpty(texturePath))
            {
                return;
            }

            ApplyMaterialToPrefab(prefab, data, targetMaterial.nameInModel, texturePath);
        }

        private void ApplyMaterialToPrefab(GameObject prefab, FurnitureData data, string nameInModel,
            string texturePath)
        {
            var renderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            Texture2D texture = null;
            if (!string.IsNullOrEmpty(texturePath))
            {
                texture = Resources.Load<Texture2D>(texturePath);
            }

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
                            ApplyTextureTiling(materialCopy, data, texturePath);
                        }

                        renderer.sharedMaterial = materialCopy;
                    }
                }
            }
        }

        public void ApplyTextureTiling(Material material, FurnitureData data, string texturePath)
        {
            MaterialInfo materialInfo = data.Parts.Values
                .SelectMany(part => part.materials)
                .FirstOrDefault(m => m.texturePath == texturePath);

            if (materialInfo == null)
            {
                return;
            }

         //   Debug.Log(material.GetTextureScale("baseColorTexture"));
            
            material.SetTextureScale("baseColorTexture", new Vector2(materialInfo.width / I, materialInfo.height / I));

           // Debug.Log(material.GetTextureScale("baseColorTexture"));
        }
    }
}