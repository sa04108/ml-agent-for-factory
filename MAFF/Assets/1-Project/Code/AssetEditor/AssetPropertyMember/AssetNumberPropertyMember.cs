using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class AssetNumberPropertyMember : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Slider slider;

        private Material mat;
        private string propertyName;
        private MaterialPropertyType type;
        private float currentValue;

        private float CurrentValue
        {
            get => currentValue;
            set
            {
                currentValue = value;

                if (type == MaterialPropertyType.Float)
                {
                    mat.SetFloat(propertyName, value);
                }
                else if (type == MaterialPropertyType.Int)
                {
                    mat.SetInteger(propertyName, (int)value);
                }
            }
        }

        private void Start()
        {
            inputField.onEndEdit.AddListener(OnInputValueChanged);
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        public void Initialize(Material mat, MaterialPropertyType type, string name, float value, float min, float max)
        {
            this.mat = mat;
            this.type = type;
            propertyName = name;
            title.text = $"{name}, [{type}]";
            currentValue = value;
            inputField.SetTextWithoutNotify(value.ToString());

            if (type == MaterialPropertyType.Int)
                slider.wholeNumbers = true;

            slider.value = value;
            slider.minValue = min;
            slider.maxValue = max;
        }

        public void Initialize(Material mat, MaterialPropertyType type, string name, float value)
        {
            this.mat = mat;
            this.type = type;
            propertyName = name;
            title.text = $"{name}, [{type}]";
            currentValue = value;
            inputField.SetTextWithoutNotify(value.ToString());

            slider.gameObject.SetActive(false);
        }

        private void OnInputValueChanged(string value)
        {
            if (type == MaterialPropertyType.Float &&
                float.TryParse(value, out float fResult))
            {
                fResult = Mathf.Clamp(fResult, slider.minValue, slider.maxValue);
                CurrentValue = fResult;
                inputField.SetTextWithoutNotify(CurrentValue.ToString());
                slider.SetValueWithoutNotify(CurrentValue);
            }
            else if (type == MaterialPropertyType.Int &&
                int.TryParse(value, out int iResult))
            {
                iResult = Mathf.Clamp(iResult, (int)slider.minValue, (int)slider.maxValue);
                CurrentValue = iResult;
                inputField.SetTextWithoutNotify(CurrentValue.ToString());
                slider.SetValueWithoutNotify(CurrentValue);
            }
            else // 빈 값 입력 포함
            {
                inputField.SetTextWithoutNotify(inputField.text);
            }
        }

        public void OnSliderValueChanged(float value)
        {
            CurrentValue = value;
            inputField.SetTextWithoutNotify(CurrentValue.ToString());
        }
    }
}