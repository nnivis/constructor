using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CodeBase.Services.FurnitureConstructor.View
{
    public class DropDownSectionView : MonoBehaviour
    {
        public IReadOnlyList<DropDownView> DropDownViews => _dropDownViews;
        public int DropDownCount => _dropDownViews.Count;

        [SerializeField] private RectTransform container;
        private List<DropDownView> _dropDownViews = new List<DropDownView>();

        public void AddDropDownView(
            DropDownView dropDownViewPrefab,
            string labelText,
            List<string> options,
            UnityAction<string> onValueChangedCallback)
        {
            var dropDownView = Instantiate(dropDownViewPrefab, container);
            dropDownView.SetLabel(labelText);
            dropDownView.SetOptions(options);

            dropDownView.AddOnValueChangedListener(index =>
            {
                onValueChangedCallback?.Invoke(options[index]);
            });

            _dropDownViews.Add(dropDownView);
        }

        public void Clear()
        {
            foreach (var dropDownView in _dropDownViews)
            {
                Destroy(dropDownView.gameObject);
            }

            _dropDownViews.Clear();
        }

        public void SetSelectedValue(int dropDownIndex, string value)
        {
            if (dropDownIndex >= 0 && dropDownIndex < _dropDownViews.Count)
            {
                _dropDownViews[dropDownIndex].SetSelectedValue(value);
            }
        }
    }
}