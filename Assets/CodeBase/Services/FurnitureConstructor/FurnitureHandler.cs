using System.Collections.Generic;
using CodeBase.Services.FurnitureConstructor.Data;
using CodeBase.Services.FurnitureConstructor.View;
using UnityEngine;

namespace CodeBase.Services.FurnitureConstructor
{
    public class FurnitureHandler : MonoBehaviour
    {
        public IEnumerable<Furniture> Furnitures => _furnitures;

        [SerializeField] private FurniturePanel furniturePanel;

        private FurnitureFactory _furnitureFactory;
        private Furniture _currentFurniture;
        private List<Furniture> _furnitures = new List<Furniture>();

        public void Initialize()
        {
            _furnitureFactory = new FurnitureFactory();
            furniturePanel.Initialize(Furnitures);

            furniturePanel.OnMaterialChange += ChangeMaterialCurrentFurniture;
            furniturePanel.OnStyleChange += ChangeStyleCurrentFurniture;
            furniturePanel.OnSizeChange += ChangeSizeCurrentFurniture;
        }

        public void GenerateFurniture(GameObject furniturePrefab) => CreateFurniture(furniturePrefab);

        private void CreateFurniture(GameObject furniturePrefab)
        {
            var furniture = _furnitureFactory.CreateFurniture(furniturePrefab);

            if (furniture != null)
            {
                Debug.Log("Мебель успешно создана!");
                _currentFurniture = furniture;
                _furnitures.Add(furniture);
                furniturePanel.UpdateSection();
            }
            else
            {
                Debug.LogError("Ошибка создания мебели.");
            }
        }

        private void ChangeMaterialCurrentFurniture(FurnitureData data, string partName, string selectedTextureName)
        {
            if (CheckData(data)) return;

            _currentFurniture.ApplyNewMaterial(partName, selectedTextureName);
        }

        private void ChangeStyleCurrentFurniture(FurnitureData data, string keyStyle, string nameStyle,
            string nameInModel)
        {
            if (CheckData(data)) return;

            _currentFurniture.ApplyNewStyle(keyStyle, nameStyle);
        }

        private void ChangeSizeCurrentFurniture(FurnitureData data, MorphType type, float value)
        {
            if (CheckData(data)) return;

            _currentFurniture.ApplyNewSize(type, value);
        }

        private bool CheckData(FurnitureData data)
        {
            if (!ReferenceEquals(data, _currentFurniture.Data))
            {
                return true;
            }

            return false;
        }
    }
}