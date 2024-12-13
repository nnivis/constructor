using UnityEngine;
using CodeBase.Services.FurnitureConstructor.Data;

namespace CodeBase.Services.FurnitureConstructor.Modifier
{
    public class Modifier
    {
        private readonly StyleModifier _styleModifier;
        private readonly MaterialModifier _materialModifier;
        private readonly SizeModifier _sizeModifier;

        public Modifier()
        {
            _styleModifier = new StyleModifier();
            _sizeModifier = new SizeModifier();
            _materialModifier = new MaterialModifier();
        }

        public void SetStartModifier(FurnitureData data, GameObject prefab) => _styleModifier.SetStartStyle(data, prefab);

        public void SetStyleByKeyAndLabel(FurnitureData data, GameObject prefab, string styleKey, string styleLabel) =>
            _styleModifier.SetStyleByKeyAndLabel(data, prefab, styleKey, styleLabel);

        public void SetStartSize(FurnitureData data, GameObject prefab) => _sizeModifier.InitializeSize(data, prefab);

        public void SetSize(FurnitureData data, GameObject prefab, MorphType type, float newValue) => _sizeModifier.SetSize(data, prefab, type, newValue);
    }
}