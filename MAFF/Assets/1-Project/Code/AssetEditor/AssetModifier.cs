using System.Collections.Generic;
using UnityEngine;

namespace Merlin
{
    public class AssetModifier : MonoBehaviour
    {
        [SerializeField]
        private Transform memberParent;

        [SerializeField]
        private AssetPropertyMemberCreator memberCreator;

        private int presetCount;
        private GameObject assetInstance;

        private void Start()
        {
            presetCount = memberParent.transform.childCount;
        }

        public void SetFbxInstance(GameObject go)
        {
            assetInstance = go;

            ClearMembers();
            LoadMembers();
        }

        private void LoadMembers()
        {
            Renderer[] renderers = assetInstance.GetComponentsInChildren<Renderer>();
            HashSet<int> materialSet = new();

            foreach (Renderer renderer in renderers)
            {
                var mat = renderer.sharedMaterial;
                var matHash = mat.GetHashCode();

                if (materialSet.Contains(matHash))
                    continue;
                materialSet.Add(matHash);

#if UNITY_EDITOR
                Dictionary<string, int> shaderProps = new();
                int propCount = UnityEditor.ShaderUtil.GetPropertyCount(mat.shader);
                for (int i = 0; i < propCount; i++)
                {
                    shaderProps.Add(UnityEditor.ShaderUtil.GetPropertyName(mat.shader, i), i);
                }
#endif

                var mainTex = mat.mainTexture as Texture2D;
                if (mainTex == null)
                    continue;

                var mainSprite = TextureToSprite(mainTex);
                var group = memberCreator.CreateGroupMember(mainSprite, "Material", mat.name, memberParent);

                var floatProps = mat.GetPropertyNames(MaterialPropertyType.Float);
                foreach (string prop in floatProps)
                {
                    var value = mat.GetFloat(prop);
#if UNITY_EDITOR
                    if (UnityEditor.ShaderUtil.GetPropertyType(mat.shader, shaderProps[prop]) == UnityEditor.ShaderUtil.ShaderPropertyType.Range)
                    {
                        float min = UnityEditor.ShaderUtil.GetRangeLimits(mat.shader, shaderProps[prop], 1);
                        float max = UnityEditor.ShaderUtil.GetRangeLimits(mat.shader, shaderProps[prop], 2);
                        memberCreator.CreateFloatMember(mat, prop, value, min, max, group);
                    }
                    else
                    {
                        memberCreator.CreateFloatMember(mat, prop, value, group);
                    }
#else
                    memberCreator.CreateFloatMember(mat, prop, value, -2, 2, group);
#endif
                }

                var textureProps = mat.GetTexturePropertyNames();
                foreach (string prop in textureProps)
                {
                    var texture = mat.GetTexture(prop) as Texture2D;
                    if (texture == null)
                        continue;

                    var sprite = TextureToSprite(texture);
                    memberCreator.CreateGroupMember(sprite, "Texture", renderer.name, group);
                }
            }
        }

        private Sprite TextureToSprite(Texture2D texture)
        {
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            return sprite;
        }

        private void ClearMembers()
        {
            for (int i = memberParent.childCount - 1; i >= presetCount; i--)
            {
                Destroy(memberParent.GetChild(i).gameObject);
            }
        }
    }
}