using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class AssetVectorPropertyMember : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private Button colorIconButton;
        [SerializeField] private TMP_Text[] inputFieldLabels;
        [SerializeField] private TMP_InputField[] inputFields;

        private Material mat;
        private string propertyName;
        private Vector4 currentValue;
        private bool isColor;
        private FlexibleColorPicker colorPicker;

        private void Start()
        {
            for (int i = 0; i < 4; i++)
            {
                int c_i = i;
                inputFields[i].onEndEdit.AddListener(value =>
                {
                    OnInputValueChanged(inputFields[c_i], value, c_i);
                });
            }
        }

        public void Initialize(Material mat, FlexibleColorPicker fcp, string name, bool isColor, Vector4 value)
        {
            this.mat = mat;
            this.isColor = isColor;

            colorIconButton.gameObject.SetActive(isColor);
            propertyName = name;
            title.text = $"{name}, [{(isColor ? "Color" : "Vector")}]";
            currentValue = value;

            if (isColor)
            {
                SetColorEditor(fcp, value);

                string[] colorChanels = { "R", "G", "B", "A" };
                for (int i = 0; i < 4; i++)
                {
                    var color = Color.black;
                    color[i] = 1f;
                    inputFieldLabels[i].color = color;

                    inputFieldLabels[i].text = colorChanels[i];
                    inputFields[i].text = value[i].ToString();
                }
            }
            else
            {
                string[] vectorChanels = { "X", "Y", "Z", "W" };
                for (int i = 0; i < 4; i++)
                {
                    inputFieldLabels[i].text = vectorChanels[i];
                    inputFields[i].text = value[i].ToString();
                }
            }
        }

        private void SetColorEditor(FlexibleColorPicker fcp, Vector4 value)
        {
            colorPicker = fcp;
            colorPicker.color = value;
            colorPicker.onColorChange.AddListener(OnColorPick);
            colorIconButton.onClick.AddListener(ToggleColorPick);

            SetColor(value);
        }

        private void SetColor(Color color)
        {
            currentValue = color;
            colorIconButton.image.color = color;

            mat.SetColor(propertyName, color);
        }

        private void OnInputValueChanged(TMP_InputField inputField, string value, int idx)
        {
            if (float.TryParse(value, out float fResult))
            {
                currentValue[idx] = fResult;
                inputField.SetTextWithoutNotify(fResult.ToString());

                if (isColor)
                {
                    var color = colorIconButton.image.color;
                    color[idx] = fResult;
                    colorPicker.color = color;

                    SetColor(color);
                }
                else
                {
                    mat.SetVector(title.text, currentValue);
                }
            }
            else // 빈 값 입력 포함
            {
                inputField.SetTextWithoutNotify(currentValue[idx].ToString());
            }
        }

        private void OnColorPick(Color color)
        {
            for (int i = 0; i < 4; i++)
            {
                inputFields[i].SetTextWithoutNotify(color[i].ToString());
            }

            SetColor(color);
        }

        private void ToggleColorPick()
        {
            colorPicker.gameObject.SetActive(!colorPicker.gameObject.activeSelf);
        }
    }
}