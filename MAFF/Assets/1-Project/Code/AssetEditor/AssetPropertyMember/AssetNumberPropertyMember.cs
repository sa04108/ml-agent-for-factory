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
                    mat.SetFloat(title.text, value);
                }
                else if (type == MaterialPropertyType.Int)
                {
                    mat.SetInt(title.text, (int)value);
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
            title.text = name;
            currentValue = value;
            inputField.text = value.ToString();

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
            title.text = name;
            currentValue = value;
            inputField.text = value.ToString();

            slider.gameObject.SetActive(false);
        }

        private void OnInputValueChanged(string value)
        {
            if (type == MaterialPropertyType.Float &&
                float.TryParse(value, out float fResult))
            {
                CurrentValue = fResult;
                slider.SetValueWithoutNotify(currentValue);
            }
            else if (type == MaterialPropertyType.Int &&
                int.TryParse(value, out int iResult))
            {
                CurrentValue = iResult;
                slider.SetValueWithoutNotify(currentValue);
            }
            else // 빈 값 입력 포함
            {
                inputField.SetTextWithoutNotify(inputField.text);
            }
        }

        public void OnSliderValueChanged(float value)
        {
            CurrentValue = slider.value;
            inputField.SetTextWithoutNotify(currentValue.ToString());
        }
    }
}