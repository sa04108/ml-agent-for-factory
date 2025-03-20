using Sirenix.OdinInspector;
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
        [SerializeField, Tooltip("Agent의 최고 이동 속도 (초당 단위 거리)")]
        private float maxSpeed = 5f;
        [SerializeField, Tooltip("Agent가 포인트에 있음을 판단하는 허용 오차")]
        private float tolerance = 0.1f;

        [Header("Rewards")]
        [SerializeField, Tooltip("다음 포인트에 도착할 때 받는 보상")]
        private float rewardForNextPoint = 3.0f;
        [SerializeField, Tooltip("목적지에 도착할 때 받는 보상")]
        private float rewardForArrived = 10.0f;
        [SerializeField, Tooltip("Segment를 벗어날 때 받는 보상")]
        private float rewardForOffSegment = -15.0f;
        [SerializeField, Tooltip("다른 Agent와 충돌할 때 받는 보상")]
        private float rewardForCollision = -20.0f;

        [Header("Unity Links")]
        [SerializeField]
        private ParticleSystem particle;

        private PathManager pathManager;

        private float speed;
        // 현재 경로(격자 노드들의 월드 좌표 목록)와 인덱스
        private List<Node> path;
        private Node lastNode;
        private Node lastGoalNode;
        private int nextNodeIndex;

        private void Start()
        {
            pathManager = App.Instance.PathManager;

            var homeNode = pathManager.FindNearestNode(transform.position);
            lastNode = homeNode;
        }

        public override void OnEpisodeBegin()
        {
            path = pathManager.FindPath(lastNode, lastGoalNode);
            lastGoalNode = path[path.Count - 1];
            nextNodeIndex = 1;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // Agent의 현재 속도
            sensor.AddObservation(speed);
            // Agent의 현재 위치
            sensor.AddObservation(transform.position);
            // Path의 다음 위치
            sensor.AddObservation(path[nextNodeIndex].position);
            // Path의 다음 위치로의 방향
            sensor.AddObservation((path[nextNodeIndex].position - transform.position).normalized);
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            // 연속 액션: 두 개의 값(x, z)으로 이동 방향 결정
            float dirX = actionBuffers.ContinuousActions[0];
            float dirZ = actionBuffers.ContinuousActions[1];
            speed = Mathf.Clamp(0, actionBuffers.ContinuousActions[2], maxSpeed);
            Vector3 actionDir = new Vector3(dirX, 0, dirZ).normalized;

            transform.position += actionDir * speed * Time.deltaTime;

            // Segment를 벗어나려고 시도하면 에피소드 종료
            Vector3 nextPoint = path[nextNodeIndex].position;
            Vector3 desiredDir = (nextPoint - transform.position).normalized;
            if (Vector3.Dot(actionDir, desiredDir) < 0.8f)
            {
                AddReward(rewardForOffSegment);
                EndEpisode();

                return;
            }

            // 경유지에 도착하면 다음 경유지를 요청
            if (Vector3.Distance(transform.position, nextPoint) <= tolerance)
            {
                lastNode = path[nextNodeIndex];

                // 목적지에 도착시 보상을 주고 에피소드 종료
                if (nextNodeIndex == path.Count - 1)
                {
                    lastGoalNode = null;
                    particle.Play();
                    AddReward(rewardForArrived);
                    EndEpisode();
                }
                else
                {
                    AddReward(rewardForNextPoint);
                }

                nextNodeIndex = Mathf.Clamp(nextNodeIndex + 1, 0, path.Count - 1);
            }
        }

        // 휴리스틱: 테스트 시 현재 Path의 목적지로 이동
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            Vector3 direction = (path[nextNodeIndex].position - transform.position).normalized;
            continuousActionsOut[0] = direction.x;
            continuousActionsOut[1] = direction.z;
            continuousActionsOut[2] = maxSpeed;
        }

        // 다른 Agent의 충돌 위험 포착시 경로 수정
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Agent"))
            {
                AddReward(rewardForCollision);
                EndEpisode();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (path == null || path.Count == 0)
                return;

            UnityEditor.Handles.color = GetComponent<ObjectColor>().color;
            Vector3[] points = new Vector3[path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                if (path[i] != null)
                    points[i] = path[i].position;
            }
            UnityEditor.Handles.DrawAAPolyLine(8f, points);
        }
#endif

        private bool IsAgentOnSegment(Vector3 start, Vector3 end, float tolerance)
        {
            // 선분의 방향과 길이 계산
            Vector3 segmentDirection = (end - start).normalized;
            float segmentLength = Vector3.Distance(start, end);

            // 에이전트 위치를 선분의 시작점으로부터의 벡터로 만들고, 선분 방향으로 투영
            Vector3 agentVector = transform.position - start;
            float projectedLength = Vector3.Dot(agentVector, segmentDirection);

            // 투영 길이가 선분의 길이 범위 내에 있는지 확인합니다.
            if (projectedLength < 0 || projectedLength > segmentLength)
                return false;

            // 투영한 점(선분 위의 가장 가까운 점) 계산
            Vector3 closestPoint = start + segmentDirection * projectedLength;
            float distanceFromSegment = Vector3.Distance(transform.position, closestPoint);

            // 에이전트가 선으로부터 tolerance 이내에 있으면 true 반환
            return distanceFromSegment <= tolerance;
        }
    }
}