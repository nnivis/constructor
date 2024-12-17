using System.Collections.Generic;
using System.Linq;
using CodeBase.Services.FurnitureConstructor.Data;
using UnityEngine;

namespace CodeBase.Services.FurnitureConstructor.Modifier
{
    public class StyleModifier
    {
        private readonly Dictionary<string, string> _activeStyles = new Dictionary<string, string>();

        public void InitializeStyle(FurnitureData data, GameObject prefab)
        {
            if (data?.parts == null || prefab == null)
            {
                return;
            }

            _activeStyles.Clear();
            var allNameInModels = new HashSet<string>();
            var activeNameInModels = new HashSet<string>();

            foreach (var part in data.parts)
            {
                foreach (var styleEntry in part.Value.styles)
                {
                    foreach (var style in styleEntry.Value)
                    {
                        allNameInModels.Add(style.nameInModel);
                    }

                    if (styleEntry.Value.Count > 0)
                    {
                        var startStyle = styleEntry.Value[0];
                        activeNameInModels.Add(startStyle.nameInModel);
                        _activeStyles[styleEntry.Key] = startStyle.label; // Сохраняем стартовый стиль
                    }
                }
            }

            ApplyStyleToChildren(prefab, allNameInModels, activeNameInModels);
        }

        public void SetStyleByKeyAndLabel(FurnitureData data, GameObject prefab, string styleKey, string styleLabel)
        {
            if (data?.parts == null || prefab == null)
            {
                return;
            }

            // Обновляем текущий стиль для данной группы
            _activeStyles[styleKey] = styleLabel;

            var allNameInModels = new HashSet<string>();
            var activeNameInModels = new HashSet<string>();

            foreach (var part in data.parts)
            {
                foreach (var styleEntry in part.Value.styles)
                {
                    foreach (var styleInfo in styleEntry.Value)
                    {
                        allNameInModels.Add(styleInfo.nameInModel);

                        // Если стиль из активной группы совпадает, добавляем его как активный
                        if (_activeStyles.TryGetValue(styleEntry.Key, out var activeLabel) &&
                            styleInfo.label == activeLabel)
                        {
                            activeNameInModels.Add(styleInfo.nameInModel);
                        }
                    }
                }
            }

            ApplyStyleToChildren(prefab, allNameInModels, activeNameInModels);
        }

        private void ApplyStyleToChildren(GameObject parent, HashSet<string> allNameInModels,
            HashSet<string> activeNameInModels)
        {
            foreach (Transform child in parent.transform)
            {
                string childName = child.name;

                // Проверка, присутствует ли имя в модели среди всех стилей
                if (allNameInModels.Any(nameInModel => childName.Contains(nameInModel)))
                {
                    bool isActive = activeNameInModels.Any(activeName => childName.Contains(activeName));
                    child.gameObject.SetActive(isActive);
                }

                // Рекурсивный вызов для детей
                if (child.childCount > 0)
                {
                    ApplyStyleToChildren(child.gameObject, allNameInModels, activeNameInModels);
                }
            }
        }
    }
}
