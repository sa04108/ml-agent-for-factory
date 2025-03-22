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

        public void Initialize(Texture tex, string name)
        {
            icon.sprite = TextureToSprite(tex);
            desc.text = name;
        }

        public void Initialize(Texture tex, string type, string name)
        {
            icon.sprite = TextureToSprite(tex);
            desc.text = $"Type: {type}\n" +
                        $"{name}";
        }

        private Sprite TextureToSprite(Texture texture)
        {
            if (texture == null)
                return null;

            Texture2D tex2D = texture as Texture2D;
            Sprite sprite = Sprite.Create(
                tex2D,
                new Rect(0, 0, tex2D.width, tex2D.height),
                new Vector2(0.5f, 0.5f)
            );

            return sprite;
        }
    }
}