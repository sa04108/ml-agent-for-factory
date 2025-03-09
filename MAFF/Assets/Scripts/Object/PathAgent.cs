using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace MAFF
{
    public class PathAgent : Agent
    {
        [Header("Options")]
        [Tooltip("Agent가 이동할 속도 (초당 단위 거리)")]
        public float speed = 5f;
        [Tooltip("Agent가 Segment 위에 있다고 판단하는 허용 오차")]
        public float segmentTolerance = 0.1f;
        [Tooltip("목적지에 도착했다고 판단하는 임계값")]
        public float arrivalThreshold = 0.1f;

        [Header("Rewards")]
        [Tooltip("목적지에 도착할 때 받는 보상")]
        public float rewardForArrived = 1.0f;

        private PathManager pathManager;
        private Vector3 startPosition;
        private Vector3 targetRallyPoint;
        // 현재 경로(격자 노드들의 월드 좌표 목록)와 인덱스
        private List<Vector3> path;
        private int currentPathIndex = 0;

        private void Start()
        {
            pathManager = MLApp.Instance.PathManager;
            startPosition = transform.position;

            if (MLApp.Instance.PathAlgorithm != PathAlgorithm.None )
            {
                UpdateWithAlgorithm();
            }
        }

        private void UpdateWithAlgorithm()
        {
            path = pathManager.GetNewPath(transform.position);

            StartCoroutine(CoUpdateWithAlgorithm());
        }

        private IEnumerator CoUpdateWithAlgorithm()
        {
            if (path == null || path.Count == 0)
                yield break;

            while (MLApp.Instance.PathAlgorithm != PathAlgorithm.None)
            {
                Vector3 targetPoint = path[currentPathIndex];
                float step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPoint, step);

                if (Vector3.Distance(transform.position, targetPoint) <= arrivalThreshold)
                {
                    currentPathIndex++;
                    if (currentPathIndex >= path.Count)
                    {
                        // 전체 경로를 완료하면 새 경로 요청 (또는 다른 행동을 할 수 있음)
                        path = pathManager.GetNewPath(transform.position);
                        currentPathIndex = 0;
                    }
                }

                yield return null;
            }
        }

        public override void OnEpisodeBegin()
        {
            transform.position = startPosition;

            // 새로운 랠리 포인트를 요청
            RequestNextRallyPoint();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // Agent의 현재 위치
            sensor.AddObservation(transform.position);
            // 목표 랠리 포인트의 위치
            sensor.AddObservation(targetRallyPoint);
            // 목표로 향하는 방향 (정규화)
            sensor.AddObservation((targetRallyPoint - transform.position).normalized);
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            // 연속 액션: 두 개의 값(x, z)으로 이동 방향 결정
            float moveX = actionBuffers.ContinuousActions[0];
            float moveZ = actionBuffers.ContinuousActions[1];
            Vector3 moveDir = new Vector3(moveX, 0f, moveZ).normalized;

            // Agent 이동 (시간에 따른 보간)
            transform.position += moveDir * speed * Time.deltaTime;

            // 랠리 포인트에 도달하면 보상을 주고 다음 랠리 포인트를 요청
            if (Vector3.Distance(transform.position, targetRallyPoint) < segmentTolerance)
            {
                SetReward(rewardForArrived);
                RequestNextRallyPoint();
            }

            // 세그먼트 밖으로 벗어나면 에피소드 종료
            if (!pathManager.IsOnSegment(transform.position, segmentTolerance))
            {
                EndEpisode();
            }
        }

        // 휴리스틱: 테스트 시, 목표를 향해 직접 이동하도록 함
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            Vector3 direction = (targetRallyPoint - transform.position).normalized;
            continuousActionsOut[0] = direction.x;
            continuousActionsOut[1] = direction.z;
        }

        // 다른 Agent와 충돌 시 에피소드 종료
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Agent"))
            {
                EndEpisode();
            }
        }

        // PathManager의 교차점(waypoints) 중 무작위로 새로운 랠리 포인트를 선택
        private void RequestNextRallyPoint()
        {
            targetRallyPoint = pathManager.GetRandomWayPoint(targetRallyPoint);
        }
    }
}