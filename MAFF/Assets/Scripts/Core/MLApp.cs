using UnityEngine;

namespace MAFF
{
    public class MLApp : Singleton<MLApp>
    {
        [Header("Agent")]

        [Header("Path")]
        [SerializeField] private PathAlgorithm pathAlgorithm;
        [SerializeField] private PathManager pathManager;

        public PathAlgorithm PathAlgorithm => pathAlgorithm;
        public PathManager PathManager => pathManager;

        protected override void Awake()
        {
            base.Awake();
            pathManager.Initialize();
        }

        public static void Play()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = true;
#endif
        }
    }
}