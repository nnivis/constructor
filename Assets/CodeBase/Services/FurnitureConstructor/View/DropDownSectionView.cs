﻿using System.Collections.Generic;
using UnityEngine;

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
            UnityEngine.Events.UnityAction<string> onValueChangedCallback)
        {
            var dropDownView = Instantiate(dropDownViewPrefab, container);
            dropDownView.SetLabel(labelText);
            dropDownView.SetOptions(options);

            dropDownView.AddOnValueChangedListener(index =>
            {
                var selectedOption = options[index];
                onValueChangedCallback?.Invoke(selectedOption);
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
            else
            {
                Debug.LogWarning($"DropDownView at index {dropDownIndex} does not exist.");
            }
        }
    }
}