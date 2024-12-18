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
            if (data?.Parts == null || prefab == null)
            {
                return;
            }

            _activeStyles.Clear();
            var allNameInModels = new HashSet<string>();
            var activeNameInModels = new HashSet<string>();

            foreach (var part in data.Parts)
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
                        _activeStyles[styleEntry.Key] = startStyle.label; 
                    }
                }
            }

            ApplyStyleToChildren(prefab, allNameInModels, activeNameInModels);
        }

        public void SetStyleByKeyAndLabel(FurnitureData data, GameObject prefab, string styleKey, string styleLabel)
        {
            if (data?.Parts == null || prefab == null)
            {
                return;
            }

            _activeStyles[styleKey] = styleLabel;

            var allNameInModels = new HashSet<string>();
            var activeNameInModels = new HashSet<string>();

            foreach (var part in data.Parts)
            {
                foreach (var styleEntry in part.Value.styles)
                {
                    foreach (var styleInfo in styleEntry.Value)
                    {
                        allNameInModels.Add(styleInfo.nameInModel);

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

                if (allNameInModels.Any(nameInModel => childName.Contains(nameInModel)))
                {
                    bool isActive = activeNameInModels.Any(activeName => childName.Contains(activeName));
                    child.gameObject.SetActive(isActive);
                }

                if (child.childCount > 0)
                {
                    ApplyStyleToChildren(child.gameObject, allNameInModels, activeNameInModels);
                }
            }
        }
    }
}
