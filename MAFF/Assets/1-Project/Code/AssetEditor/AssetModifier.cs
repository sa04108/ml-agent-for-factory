using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Merlin
{
    // Shader Name -> Property Name -> Property Type -> Property Value
    using ShaderProperty = Dictionary<string, Dictionary<string, Dictionary<string, object>>>;

    public class AssetModifier : MonoBehaviour
    {
        [SerializeField]
        private Transform memberParent;

        [SerializeField]
        private AssetPropertyMemberCreator memberCreator;

        private int presetCount;
        private GameObject assetInstance;

        [SerializeField]
        private TextAsset shaderPropJson;
        private ShaderProperty shaderProps;

        private void Start()
        {
            presetCount = memberParent.transform.childCount;
            shaderProps = JsonConvert.DeserializeObject<ShaderProperty>(shaderPropJson.text);
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
                foreach (Material mat in renderer.sharedMaterials)
                {
                    var matHash = mat.GetHashCode();

                    if (materialSet.Contains(matHash))
                        continue;
                    materialSet.Add(matHash);

                    var group = memberCreator.CreateGroupMember(mat.mainTexture, "Material", mat.name, memberParent);

                    var textureProps = mat.GetTexturePropertyNames();
                    foreach (string prop in textureProps)
                    {
                        var tex = mat.GetTexture(prop);
                        if (tex == null)
                            continue;

                        memberCreator.CreateGroupMember(tex, "Texture", prop, group);
                    }

                    var floatProps = mat.GetPropertyNames(MaterialPropertyType.Float);
                    foreach (string prop in floatProps)
                    {
                        var value = mat.GetFloat(prop);
                        if (shaderProps.ContainsKey(mat.shader.name) &&
                            shaderProps[mat.shader.name].ContainsKey(prop) &&
                            shaderProps[mat.shader.name][prop].ContainsKey("Range"))
                        {
                            Dictionary<string, float> rangeDict = GetShaderPropertyValue<float>(mat.shader.name, prop, "Range");
                            float min = rangeDict["Min"];
                            float max = rangeDict["Max"];
                            memberCreator.CreateFloatMember(mat, prop, value, min, max, group);
                        }
                        else
                        {
                            memberCreator.CreateFloatMember(mat, prop, value, group);
                        }
                    }

                    var intProps = mat.GetPropertyNames(MaterialPropertyType.Int);
                    foreach (string prop in intProps)
                    {
                        var value = mat.GetInteger(prop);
                        memberCreator.CreateIntMember(mat, prop, value, group);
                    }

                    var vecProps = mat.GetPropertyNames(MaterialPropertyType.Vector);
                    foreach (string prop in vecProps)
                    {
                        var value = mat.GetVector(prop);
                        if (shaderProps.ContainsKey(mat.shader.name) &&
                            shaderProps[mat.shader.name].ContainsKey(prop) &&
                            shaderProps[mat.shader.name][prop].ContainsKey("Color"))
                        {
                            memberCreator.CreateVectorMember(mat, prop, value, true, group);
                        }
                        else
                        {
                            memberCreator.CreateVectorMember(mat, prop, value, false, group);
                        }
                    }

                    var matrixProps = mat.GetPropertyNames(MaterialPropertyType.Matrix);
                    foreach (string prop in matrixProps)
                    {
                        var value = mat.GetMatrix(prop);
                        memberCreator.CreateMatrixMember(mat, prop, value, group);
                    }
                }
            }
        }

        private void ClearMembers()
        {
            for (int i = memberParent.childCount - 1; i >= presetCount; i--)
            {
                Destroy(memberParent.GetChild(i).gameObject);
            }
        }

        private Dictionary<string, T> GetShaderPropertyValue<T>(string shaderName, string propName, string propType)
        {
            return ((JObject)shaderProps[shaderName][propName][propType]).ToObject<Dictionary<string, T>>();
        }
    }
}