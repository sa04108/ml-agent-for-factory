using UnityEngine;

namespace MAFF
{
    public class ObjectColor : MonoBehaviour
    {
        [SerializeField] public Color color = Color.white;

        void Start()
        {
            // Renderer 컴포넌트를 가져옵니다.
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                // GetComponent<Renderer>().material은 인스턴스화된 Material을 반환합니다.
                // 이렇게 하면 여러 오브젝트가 동일한 원본 Material을 공유하더라도, 
                // 각 오브젝트는 개별적으로 색상을 변경할 수 있습니다.
                Material materialInstance = rend.material;
                materialInstance.SetColor("_Color", color);
            }
        }
    }
}