using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Services.FurnitureConstructor.Data;
using UnityEngine;

namespace CodeBase.Services.FurnitureConstructor.View
{
    public class FurniturePanel : MonoBehaviour
    {
        public event Action<FurnitureData, string, string, string> OnStyleChange;
        public event Action<FurnitureData, string, string> OnMaterialChange;
        public event Action<FurnitureData, MorphType, float> OnSizeChange;

        [Header("Section")] 
        [SerializeField] private DropDownSectionView modelSection;
        [SerializeField] private SliderSectionView sizeSection;
        [SerializeField] private DropDownSectionView materialSection;
        [SerializeField] private DropDownSectionView stylesSection;

        [Header("Prefab")] 
        [SerializeField] private DropDownView dropDownViewPrefab;

        private IEnumerable<Furniture> _furnitures;
        private FurnitureData _currentFurniture;

        public void Initialize(IEnumerable<Furniture> furnitures)
        {
            _furnitures = furnitures;
        }

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
            if (_currentFurniture == null) return;

            var morphes = _currentFurniture.Parts.Values.Select(p => p.morphInfo).ToList();
            var heightMorph = morphes.FirstOrDefault(m => m.label == "Height");
            var widthMorph = morphes.FirstOrDefault(m => m.label == "Width");
            var depthMorph = morphes.FirstOrDefault(m => m.label == "Depth");

            sizeSection.InitializeSliders(
                heightMorph?.min ?? 0.5f, heightMorph?.max ?? 2f,
                widthMorph?.min ?? 0.5f, widthMorph?.max ?? 2f,
                depthMorph?.min ?? 0.5f, depthMorph?.max ?? 2f
            );

            sizeSection.OnSizeChanged -= OnSizeChanged; // Удаление старых подписок
            sizeSection.OnSizeChanged += OnSizeChanged;
        }

        private void UpdateModelSection()
        {
            modelSection.Clear();

            if (_furnitures == null || !_furnitures.Any()) return;

            var furnitureNames = _furnitures.Select(f => f.Prefab.name).ToList();

            modelSection.AddDropDownView(
                dropDownViewPrefab,
                "Select Model",
                furnitureNames,
                OnModelChanged
            );

            modelSection.SetSelectedValue(furnitureNames.Count - 1, furnitureNames.Last());
        }

        private void UpdateCurrentFurniture()
        {
            _currentFurniture = _furnitures?.LastOrDefault()?.Data;
        }

        private void UpdateMaterialSection()
        {
            materialSection.Clear();

            if (_currentFurniture?.Parts == null) return;

            foreach (var part in _currentFurniture.Parts)
            {
                if (part.Value.materials?.Any() == true)
                {
                    var options = part.Value.materials
                        .Select(material => material.textureLabel ?? "None")
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

            if (_currentFurniture?.Parts == null) return;

            foreach (var part in _currentFurniture.Parts)
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
            var selectedFurniture = _furnitures?.FirstOrDefault(f => f.Prefab.name == selectedModelName);
            if (selectedFurniture != null)
            {
                _currentFurniture = selectedFurniture.Data;
                Debug.Log($"Selected furniture: {selectedModelName}");
                UpdateModifierSection();
            }
            else
            {
                Debug.LogWarning($"Furniture with name '{selectedModelName}' not found.");
            }
        }

        private void OnMaterialChanged(string partName, string selectedTextureName)
        {
            Debug.Log($"Selected texture for part '{partName}': {selectedTextureName}");
            OnMaterialChange?.Invoke(_currentFurniture, partName, selectedTextureName);
        }

        private void OnStyleChanged(string styleKey, string selectedStyleLabel)
        {
            if (_currentFurniture?.Parts == null) return;

            foreach (var part in _currentFurniture.Parts)
            {
                if (part.Value.styles.TryGetValue(styleKey, out var styleInfos))
                {
                    var selectedStyle = styleInfos.FirstOrDefault(style => style.label == selectedStyleLabel);

                    if (selectedStyle != null)
                    {
                        Debug.Log(
                            $"Selected style: Key={styleKey}, Label={selectedStyle.label}, NameInModel={selectedStyle.nameInModel}");
                        OnStyleChange?.Invoke(_currentFurniture, styleKey, selectedStyle.label, selectedStyle.nameInModel);
                        return;
                    }
                }
            }
        }

        private void OnSizeChanged(MorphType type, float value)
        {
            OnSizeChange?.Invoke(_currentFurniture, type, value);
        }
    }
}
