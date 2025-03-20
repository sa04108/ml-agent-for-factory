using UnityEngine;

namespace MAFF
{
    public class App : Singleton<App>
    {
        [Header("Links")]
        [SerializeField] private PathAlgorithm pathAlgorithm;
        [SerializeField] private PathManager pathManager;

        public PathAlgorithm PathAlgorithm => pathAlgorithm;
        public PathManager PathManager => pathManager;

        protected override void Awake()
        {
            base.Awake();
            pathManager.Initialize();
        }
    }
}