using System.Collections.Generic;
using System.Linq;
using CodeBase.Services.FurnitureConstructor.Data;
using UnityEngine;

namespace CodeBase.Services.FurnitureConstructor.View
{
    public class FurniturePanel : MonoBehaviour
    {
        [Header("Section")] [SerializeField] private DropDownSectionView modelSection;

        [SerializeField] private SliderSectionView sizeSection;
        [SerializeField] private DropDownSectionView materialSection;
        [SerializeField] private DropDownSectionView stylesSection;

        [Header("Prefab")] [SerializeField] private DropDownView dropDownViewPrefab;

        private IEnumerable<Furniture> _furnitures;
        private FurnitureData _currentFurniture;

        public void Initialize(IEnumerable<Furniture> furnitures) => _furnitures = furnitures;

        public void UpdateSection()
        {
            UpdateModelSection();
            UpdateCurrentFurniture();
            UpdateModifierSection();
        }

        private void UpdateModifierSection()
        {
            UpdateSizeSection();
            UpdateMaterialSection();
            UpdateStyleSection();
        }

        private void UpdateSizeSection()
        {
            
        }

        private void UpdateModelSection()
        {
            modelSection.Clear();

            if (_furnitures == null || !_furnitures.Any())
            {
                return;
            }

            var furnitureNames = _furnitures.Select(f => f.Prefab.name).ToList();

            modelSection.AddDropDownView(
                dropDownViewPrefab,
                "Select Model",
                furnitureNames,
                OnModelChanged
            );

            int lastIndex = furnitureNames.Count - 1;

            modelSection.SetSelectedValue(lastIndex, furnitureNames.Last());
        }

        private void UpdateCurrentFurniture()
        {
            if (_furnitures != null && _furnitures.Any())
            {
                var lastFurniture = _furnitures.Last();
                _currentFurniture = lastFurniture.Data;
            }
        }

        private void UpdateMaterialSection()
        {
            materialSection.Clear();

            if (_currentFurniture == null || _currentFurniture.parts == null || !_currentFurniture.parts.Any())
            {
                return;
            }

            foreach (var part in _currentFurniture.parts)
            {
                if (part.Value.materials != null && part.Value.materials.Any())
                {
                    var options = part.Value.materials
                        .Select(material => material.texture != null ? material.texture.name : "None")
                        .ToList();

                    materialSection.AddDropDownView(
                        dropDownViewPrefab,
                        part.Key,
                        options,
                        selectedOption => OnMaterialChanged(part.Key, selectedOption)
                    );

                    materialSection.SetSelectedValue(materialSection.DropDownViews.Count - 1, options.First());
                }
            }
        }

        private void UpdateStyleSection()
        {
            stylesSection.Clear();

            if (_currentFurniture == null || _currentFurniture.parts == null)
            {
                return;
            }

            foreach (var part in _currentFurniture.parts)
            {
                foreach (var styleEntry in part.Value.styles)
                {
                    var styleKey = styleEntry.Key;
                    var styleOptions = styleEntry.Value.Select(style => style.label).ToList();

                    stylesSection.AddDropDownView(
                        dropDownViewPrefab,
                        styleKey,
                        styleOptions,
                        selectedLabel => OnStyleChanged(styleKey, selectedLabel)
                    );

                    if (styleOptions.Any())
                    {
                        stylesSection.SetSelectedValue(stylesSection.DropDownCount - 1, styleOptions.First());
                    }
                }
            }
        }
        
        private void OnModelChanged(string selectedModelName)
        {
            var selectedFurniture = _furnitures.FirstOrDefault(f => f.Prefab.name == selectedModelName);
            if (selectedFurniture != null)
            {
                _currentFurniture = selectedFurniture.Data;
                Debug.Log($"Selected furniture: {selectedModelName}");
                UpdateMaterialSection();
            }
            else
            {
                Debug.LogWarning($"Furniture with name {selectedModelName} not found.");
            }
        }

        private void OnMaterialChanged(string partName, string selectedTextureName)
        {
            Debug.Log($"Selected texture for part '{partName}': {selectedTextureName}");
        }

        private void OnStyleChanged(string styleKey, string selectedStyleLabel)
        {
            if (_currentFurniture == null || _currentFurniture.parts == null)
            {
                return;
            }

            foreach (var part in _currentFurniture.parts)
            {
                if (part.Value.styles.TryGetValue(styleKey, out var styleInfos))
                {
                    var selectedStyle = styleInfos.FirstOrDefault(style => style.label == selectedStyleLabel);

                    if (selectedStyle != null)
                    {
                        Debug.Log($"Selected style: Key={styleKey}, Label={selectedStyle.label}, NameInModel={selectedStyle.nameInModel}");
                        return;
                    }
                }
            }
        }
    }
}