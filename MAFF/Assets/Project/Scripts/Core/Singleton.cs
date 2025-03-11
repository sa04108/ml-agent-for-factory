using UnityEngine;

namespace MAFF
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        /// <summary>
        /// 싱글턴 인스턴스를 반환합니다. 없으면 씬에서 찾거나 새로 생성합니다.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 이미 씬에 존재하는 인스턴스 찾기
                    _instance = FindAnyObjectByType<T>();

                    // 없으면 새 GameObject에 컴포넌트로 추가
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = nameof(T);
                        // 필요하다면 씬 전환 시 삭제되지 않도록
                        DontDestroyOnLoad(singletonObject);
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            // 이미 인스턴스가 존재한다면, 중복 객체는 제거
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

}