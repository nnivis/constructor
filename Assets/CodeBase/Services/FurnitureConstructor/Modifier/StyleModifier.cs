using System.Collections.Generic;
using CodeBase.Services.FurnitureConstructor.Data;
using UnityEngine;

namespace CodeBase.Services.FurnitureConstructor.Modifier
{
    public class StyleModifier
    {
        public void SetStyleByKeyAndLabel(FurnitureData data, GameObject prefab, string styleKey, string styleLabel)
        {
            if (data?.parts == null || prefab == null)
            {
                //  Debug.LogWarning("Parts or prefab is null. Cannot set style.");
                return;
            }

            HashSet<string> allNameInModels = new HashSet<string>();
            foreach (var part in data.parts)
            {
                foreach (var styleList in part.Value.styles.Values)
                {
                    foreach (var styleInfo in styleList)
                    {
                        allNameInModels.Add(styleInfo.nameInModel);
                    }
                }
            }

            HashSet<string> activeNameInModels = new HashSet<string>();
            foreach (var part in data.parts)
            {
                if (part.Value.styles.TryGetValue(styleKey, out var styleInfos))
                {
                    foreach (var styleInfo in styleInfos)
                    {
                        if (styleInfo.label == styleLabel)
                        {
                            activeNameInModels.Add(styleInfo.nameInModel);
                            // Debug.Log(
                            //   $"Selected style '{styleKey}' with label '{styleLabel}' activates '{styleInfo.nameInModel}'");
                        }
                    }
                }
            }

            ApplyStyleToChildren(prefab, allNameInModels, activeNameInModels);
        }

        public void SetStartStyle(FurnitureData data, GameObject prefab)
        {
            if (data.parts == null || prefab == null)
            {
                return;
            }

            HashSet<string> allNameInModels = new HashSet<string>();
            HashSet<string> activeNameInModels = new HashSet<string>();

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
                        //Debug.Log($"Style Key '{styleEntry.Key}' will activate '{startStyle.nameInModel}'");
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
                bool isActiveStyle = false;
                bool isInAnyStyle = false;

                foreach (var activeName in activeNameInModels)
                {
                    if (childName.Contains(activeName))
                    {
                        isActiveStyle = true;
                        isInAnyStyle = true;
                        break;
                    }
                }

                if (!isActiveStyle)
                {
                    foreach (var nameInModel in allNameInModels)
                    {
                        if (childName.Contains(nameInModel))
                        {
                            isInAnyStyle = true;
                            break;
                        }
                    }
                }

                if (isInAnyStyle)
                {
                    child.gameObject.SetActive(isActiveStyle);
                    // Debug.Log($"{(isActiveStyle ? "Activated" : "Deactivated")} object: {childName}");
                }

                if (child.childCount > 0)
                {
                    ApplyStyleToChildren(child.gameObject, allNameInModels, activeNameInModels);
                }
            }
        }
    }
}