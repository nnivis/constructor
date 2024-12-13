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
        }

        public void GenerateFurniture(GameObject furniturePrefab) => CreateFurniture(furniturePrefab);

        private void CreateFurniture(GameObject furniturePrefab)
        {
            var furniture = _furnitureFactory.CreateFurniture(furniturePrefab);

            if (furniture != null)
            {
                Debug.Log("Мебель успешно создана!");
                _furnitures.Add(furniture);
                furniturePanel.UpdateSection(); 
            }
            else
            {
                Debug.LogError("Ошибка создания мебели.");
            }
        }
    }
}