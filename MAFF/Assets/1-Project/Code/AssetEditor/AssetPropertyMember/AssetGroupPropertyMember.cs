using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class AssetGroupPropertyMember : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        public Button Button => button;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private TMP_Text desc;

        public void Initialize(Sprite sprite, string name)
        {
            icon.sprite = sprite;
            desc.text = name;
        }

        public void Initialize(Sprite sprite, string type, string name)
        {
            icon.sprite = sprite;
            desc.text = $"Type: {type}\n" +
                        $"{name}";
        }
    }
}