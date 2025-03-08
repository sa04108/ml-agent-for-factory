using System.Collections.Generic;
using UnityEngine;

namespace MlAgent
{
    public class Agent : MonoBehaviour
    {
        [Tooltip("Agent가 이동할 속도 (초당 단위 거리)")]
        public float speed = 5f;
        [Tooltip("목표 rally point에 도착했다고 판단하는 임계값")]
        public float arrivalThreshold = 0.1f;

        private PathManager pathManager;
        // 현재 경로(격자 노드들의 월드 좌표 목록)와 인덱스
        private List<Vector3> path;
        private int currentPathIndex = 0;

        // 새로운 경로의 목적지를 지정하는 방법은 Commander나 다른 매니저에서 결정할 수 있습니다.
        // 여기서는 예시로 격자 내 무작위 노드를 목표로 선택합니다.
        public void OnCreate()
        {
            pathManager = MLApp.GetService<PathManager>();
            RequestNewPath();
        }

        private void Update()
        {
            if (path == null || path.Count == 0)
                return;

            Vector3 targetPoint = path[currentPathIndex];
            MoveTowards(targetPoint);

            if (Vector3.Distance(transform.position, targetPoint) <= arrivalThreshold)
            {
                currentPathIndex++;
                if (currentPathIndex >= path.Count)
                {
                    // 전체 경로를 완료하면 새 경로 요청 (또는 다른 행동을 할 수 있음)
                    RequestNewPath();
                }
            }
        }

        private void RequestNewPath()
        {
            path = pathManager.RequestNewPath(transform.position);
            currentPathIndex = 0;
        }

        private void MoveTowards(Vector3 target)
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, step);
        }
    }
}