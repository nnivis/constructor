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
                return;

            foreach (var part in data.parts.Values) 
            {
                foreach (var materialGroup in part.materials.GroupBy(m => m.label)) 
                {
                    var firstMaterial = materialGroup.FirstOrDefault();
                    if (firstMaterial == null)
                        continue;

                    // Теперь у нас нет поля texture, поэтому загружаем текстуру по пути
                    ApplyMaterialToPrefab(prefab, firstMaterial.nameInModel, firstMaterial.texturePath);
                }
            }
        }

        public void SetMaterialByLabel(FurnitureData data, GameObject prefab, string label, string textureName)
        {
            if (data == null || prefab == null)
                return;

            var targetMaterial = data.parts.Values
                .SelectMany(part => part.materials)
                .Where(m => m.texturePath != null)
                .FirstOrDefault(m => m.label == label && 
                                     m.texturePath.EndsWith(textureName, System.StringComparison.OrdinalIgnoreCase));

            if (targetMaterial == null)
                return;

            // При смене материала также загружаем текстуру при необходимости
            ApplyMaterialToPrefab(prefab, targetMaterial.nameInModel, targetMaterial.texturePath);
        }

        private void ApplyMaterialToPrefab(GameObject prefab, string nameInModel, string texturePath)
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
                        }

                        renderer.sharedMaterial = materialCopy;
                    }
                }
            }
        }
    }
}
