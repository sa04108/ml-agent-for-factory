using System;
using TMPro;
using UnityEngine;

namespace Merlin
{
    public class AssetMatrixPropertyMember : MonoBehaviour
    {
        [Serializable]
        public class InputFieldsRow
        {
            public TMP_InputField[] inputFieldsRow;
        }

        [SerializeField] private TMP_Text title;
        [SerializeField] private InputFieldsRow[] inputFields;

        private Material mat;
        private string propertyName;
        private Matrix4x4 currentValue;

        private void Start()
        {
            for (int i = 0; i < inputFields.Length; i++)
            {
                for (int j = 0; j < inputFields[i].inputFieldsRow.Length; j++)
                {
                    inputFields[i].inputFieldsRow[j].onEndEdit.AddListener(value => OnInputValueChanged(inputFields[i].inputFieldsRow[j], value, i, j));
                }
            }
        }

        public void Initialize(Material mat, string name, Matrix4x4 value)
        {
            this.mat = mat;
            propertyName = name;
            title.text = $"{name}, [Vector]";
            currentValue = value;

            for (int i = 0; i < inputFields.Length; i++)
            {
                for (int j = 0; j < inputFields[i].inputFieldsRow.Length; j++)
                {
                    inputFields[i].inputFieldsRow[j].SetTextWithoutNotify(value[i, j].ToString());
                }
            }
        }

        private void OnInputValueChanged(TMP_InputField inputField, string value, int row, int column)
        {
            if (float.TryParse(value, out float fResult))
            {
                currentValue[row, column] = fResult;
                inputField.SetTextWithoutNotify(fResult.ToString());

                // Unity Matrix4x4는 Column Major를 따르지만 그건 내부에서 알아서 해주는 것이고 외부에서는 알 필요 없다
                mat.SetMatrix(propertyName, currentValue);
            }
            else // 빈 값 입력 포함
            {
                inputField.SetTextWithoutNotify(currentValue[row, column].ToString());
            }
        }
    }
}