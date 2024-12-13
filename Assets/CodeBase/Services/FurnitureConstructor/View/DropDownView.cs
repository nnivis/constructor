using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CodeBase.Services.FurnitureConstructor.View
{
    public class DropDownView : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private TextMeshProUGUI label;

        public void SetLabel(string text)
        {
            if (label != null)
                label.text = text;
        }

        public void SetOptions(List<string> options)
        {
            if (dropdown != null)
            {
                dropdown.ClearOptions();
                dropdown.AddOptions(options);
            }
        }

        public void AddOnValueChangedListener(UnityEngine.Events.UnityAction<int> callback)
        {
            if (dropdown != null)
                dropdown.onValueChanged.AddListener(callback);
        }

        public void SetSelectedValue(string value)
        {
            if (dropdown != null)
            {
                var index = dropdown.options.FindIndex(option => option.text == value);
                if (index >= 0)
                {
                    dropdown.SetValueWithoutNotify(index);
                }
            }
        }
    }
}