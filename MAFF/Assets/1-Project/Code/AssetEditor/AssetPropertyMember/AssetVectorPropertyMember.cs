using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class AssetVectorPropertyMember : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private Image colorIcon;
        [SerializeField] private TMP_Text[] inputFieldLabels;
        [SerializeField] private TMP_InputField[] inputFields;

        private Material mat;
        private Vector4 currentValue;
        private bool isColor;

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

        public void Initialize(Material mat, string name, bool isColor, Vector4 value)
        {
            this.mat = mat;
            this.isColor = isColor;
            colorIcon.gameObject.SetActive(isColor);
            colorIcon.color = value;

            title.text = $"{name}, [{(isColor ? "Color" : "Vector")}]";
            currentValue = value;

            string[] colorChanels = { "R", "G", "B", "A" };
            string[] vectorChanels = { "X", "Y", "Z", "W" };
            for (int i = 0; i < 4; i++)
            {
                if (isColor)
                {
                    inputFieldLabels[i].text = colorChanels[i];

                    var color = Color.black;
                    color[i] = 1f;
                    inputFieldLabels[i].color = color;
                }
                else
                {
                    inputFieldLabels[i].text = vectorChanels[i];
                }

                inputFields[i].text = value[i].ToString();
            }
        }

        private void OnInputValueChanged(TMP_InputField inputField, string value, int idx)
        {
            if (float.TryParse(value, out float fResult))
            {
                currentValue[idx] = fResult;
                inputField.SetTextWithoutNotify(fResult.ToString());

                if (isColor)
                {
                    var color = colorIcon.color;
                    color[idx] = fResult;
                    colorIcon.color = color;

                    mat.SetColor(title.text, color);
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
    }
}