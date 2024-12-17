﻿using System;
using CodeBase.Services.FurnitureConstructor.Data;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.Services.FurnitureConstructor.View
{
    public class SliderSectionView : MonoBehaviour
    {
        [SerializeField] private Slider heightSlider;
        [SerializeField] private Slider widthSlider;
        [SerializeField] private Slider depthSlider;

        public event Action<MorphType, float> OnSizeChanged;

        public void InitializeSliders(
            float? heightMin, float? heightMax, 
            float? widthMin, float? widthMax, 
            float? depthMin, float? depthMax)
        {
            SetupSlider(heightSlider, heightMin, heightMax, MorphType.Height);
            SetupSlider(widthSlider, widthMin, widthMax, MorphType.Width);
            SetupSlider(depthSlider, depthMin, depthMax, MorphType.Depth);
        }

        private void SetupSlider(Slider slider, float? min, float? max, MorphType type)
        {
            if (slider == null)
            {
                Debug.LogError($"Slider for {type} is missing.");
                return;
            }

            if (max.HasValue && max.Value > 0)
            {
                slider.minValue = min ?? 0;  
                slider.maxValue = max.Value;
                slider.value = min ?? 0;

                slider.onValueChanged.RemoveAllListeners();
                slider.onValueChanged.AddListener(value => OnSizeChanged?.Invoke(type, value));

                slider.gameObject.SetActive(true);
            }
            else
            {
                slider.gameObject.SetActive(false);
            }
        }
    }
}