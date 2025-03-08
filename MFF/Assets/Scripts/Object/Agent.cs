using UnityEngine;

namespace MlAgent
{
    public class Agent : MonoBehaviour
    {
        //// 위치 체크 시 허용 오차
        //[SerializeField] private float checkTolerance = 0.1f;
        // 이동 속도 (초당 단위 거리)
        public float speed = 5f;
        // 목적지 도달 판단 임계값
        public float arrivalThreshold = 0.1f;
        // 현재 이동 목적지
        private Vector3 currentDestination;

        private PathManager pathManager;

        public void OnCreate()
        {
            pathManager = MLApp.GetService<PathManager>();
        }

        void Update()
        {
            MoveTowardsDestination();

            // 목적지에 도달하면 다음 거점을 요청
            if (Vector3.Distance(transform.position, currentDestination) <= arrivalThreshold)
            {
                currentDestination = pathManager.GetRandomWayPoint();
            }

            //bool onPath = false;
            //// PathManager의 모든 선분을 순회하며 현재 위치가 경로 상에 있는지 확인
            //foreach (SegmentData segment in pathManager.segments)
            //{
            //    if (segment.IsPointOnSegment(transform.position, checkTolerance))
            //    {
            //        onPath = true;
            //        break;
            //    }
            //}

            //// 만약 Agent의 위치가 경로 위에 있지 않으면, 가장 가까운 경로 점으로 보정
            //if (!onPath)
            //{
            //    transform.position = pathManager.GetNearestPointOnPath(transform.position);
            //}
        }

        // 현재 목적지를 향해 Agent 이동
        void MoveTowardsDestination()
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, currentDestination, step);
        }
    }

}