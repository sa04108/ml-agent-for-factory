using UnityEngine;

namespace Merlin
{
    public class AssetPropertyMemberCreator : MonoBehaviour
    {
        [SerializeField] private AssetNumberPropertyMember numMemberPreset;
        [SerializeField] private AssetVectorPropertyMember vectorMemberPreset;
        [SerializeField] private AssetMatrixPropertyMember matrixMemberPreset;
        [SerializeField] private AssetGroupPropertyMember groupMemberPreset;
        [SerializeField] private Transform memberGroupPreset;

        private void Start()
        {
            numMemberPreset.gameObject.SetActive(false);
            //vectorMemberPreset.gameObject.SetActive(false);
            //matrixMemberPreset.gameObject.SetActive(false);
            groupMemberPreset.gameObject.SetActive(false);
            memberGroupPreset.gameObject.SetActive(false);
        }

        public Transform CreateGroupMember(Sprite sprite, string type, string name, Transform parent)
        {
            var member = Instantiate(groupMemberPreset, parent);
            member.Initialize(sprite, type, name);
            member.gameObject.SetActive(true);

            var memberGroup = Instantiate(memberGroupPreset, parent);
            memberGroup.gameObject.SetActive(true);

            member.Button.onClick.AddListener(() => memberGroup.gameObject.SetActive(!memberGroup.gameObject.activeSelf));

            return memberGroup;
        }

        public AssetNumberPropertyMember CreateNumberMember(Material mat, MaterialPropertyType type, string name, float value, float min, float max, Transform parent)
        {
            var member = Instantiate(numMemberPreset, parent);
            member.Initialize(mat, type, name, value, min, max);
            member.gameObject.SetActive(true);

            return member;
        }

        public AssetNumberPropertyMember CreateNumberMember(Material mat, MaterialPropertyType type, string name, float value, Transform parent)
        {
            var member = Instantiate(numMemberPreset, parent);
            member.Initialize(mat, type, name, value);
            member.gameObject.SetActive(true);

            return member;
        }

        public AssetNumberPropertyMember CreateFloatMember(Material mat, string name, float value, float min, float max, Transform parent)
        {
            return CreateNumberMember(mat, MaterialPropertyType.Float, name, value, min, max, parent);
        }

        public AssetNumberPropertyMember CreateFloatMember(Material mat, string name, float value, Transform parent)
        {
            return CreateNumberMember(mat, MaterialPropertyType.Float, name, value, parent);
        }

        public AssetNumberPropertyMember CreateIntMember(Material mat, string name, int value, int min, int max, Transform parent)
        {
            return CreateNumberMember(mat, MaterialPropertyType.Int, name, value, min, max, parent);
        }

        public AssetNumberPropertyMember CreateIntMember(Material mat, string name, int value, Transform parent)
        {
            return CreateNumberMember(mat, MaterialPropertyType.Int, name, value, parent);
        }
    }
}