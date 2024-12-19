using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.Services.FurnitureConstructor.View
{
    public class FurnitureVisibilityPanel : MonoBehaviour
    {
        [SerializeField] private Button sizeButton;
        [SerializeField] private Button styleButton;
        [SerializeField] private Button materialButton;

        private SliderSectionView _sizeSection;
        private DropDownSectionView _materialSection;
        private DropDownSectionView _stylesSection;

        private GameObject _activeSection;

        public void Initialize(SliderSectionView sizeSection, DropDownSectionView materialSection,
            DropDownSectionView styleSection)
        {
            _sizeSection = sizeSection;
            _materialSection = materialSection;
            _stylesSection = styleSection;

            DisableSectionPanel();
        }

        private void OnEnable()
        {
            sizeButton.onClick.AddListener(OnClickSizeButton);
            styleButton.onClick.AddListener(OnClickStyleButton);
            materialButton.onClick.AddListener(OnClickMaterial);
        }

        private void OnDisable()
        {
            sizeButton.onClick.RemoveListener(OnClickSizeButton);
            styleButton.onClick.RemoveListener(OnClickStyleButton);
            materialButton.onClick.RemoveListener(OnClickMaterial);
        }

        private void OnClickSizeButton() => ToggleSection(_sizeSection.gameObject);

        private void OnClickStyleButton() => ToggleSection(_stylesSection.gameObject);

        private void OnClickMaterial() => ToggleSection(_materialSection.gameObject);

        private void ToggleSection(GameObject sectionToToggle)
        {
            if (_activeSection == sectionToToggle)
            {
                sectionToToggle.SetActive(false);
                _activeSection = null;
            }
            else
            {
                if (_activeSection != null)
                    _activeSection.SetActive(false);


                sectionToToggle.SetActive(true);
                _activeSection = sectionToToggle;
            }
        }

        private void DisableSectionPanel()
        {
            _sizeSection.gameObject.SetActive(false);
            _materialSection.gameObject.SetActive(false);
            _stylesSection.gameObject.SetActive(false);
        }
    }
}