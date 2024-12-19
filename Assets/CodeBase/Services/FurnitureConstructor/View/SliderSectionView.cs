using System;
using CodeBase.Services.FurnitureConstructor.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.Services.FurnitureConstructor.View
{
    public class SliderSectionView : MonoBehaviour
    {
        [SerializeField] private Slider heightSlider;
        [SerializeField] private Slider widthSlider;
        [SerializeField] private Slider depthSlider;

        [SerializeField] private TextMeshProUGUI heightText;
        [SerializeField] private TextMeshProUGUI widthText;
        [SerializeField] private TextMeshProUGUI depthText;

        public event Action<MorphType, float> OnSizeChanged;

        public void InitializeSliders(
            float? heightMin, float? heightMax,
            float? widthMin, float? widthMax,
            float? depthMin, float? depthMax)
        {
            SetupSlider(heightSlider, heightMin, heightMax, MorphType.Height, heightText);
            SetupSlider(widthSlider, widthMin, widthMax, MorphType.Width, widthText);
            SetupSlider(depthSlider, depthMin, depthMax, MorphType.Depth, depthText);
        }


        private void SetupSlider(Slider slider, float? min, float? max, MorphType type, TextMeshProUGUI textElement)
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
                slider.onValueChanged.AddListener(value =>
                {
                    OnSizeChanged?.Invoke(type, value);
                    UpdateText(textElement, value);
                });

                slider.gameObject.SetActive(true);

                UpdateText(textElement, slider.value);
            }
            else
            {
                slider.gameObject.SetActive(false);

                UpdateText(textElement, min ?? 0);
            }
        }

        private void UpdateText(TextMeshProUGUI text, float value)
        {
            if (text != null)
            {
                text.text = $"{Mathf.RoundToInt(value * 100)}";
            }
        }

        private TextMeshProUGUI GetTextByMorphType(MorphType type)
        {
            return type switch
            {
                MorphType.Height => heightText,
                MorphType.Width => widthText,
                MorphType.Depth => depthText,
                _ => null
            };
        }
    }
}