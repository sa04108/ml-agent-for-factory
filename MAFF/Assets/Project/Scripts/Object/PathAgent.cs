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
        [SerializeField, Tooltip("Agent의 최고 이동 속도 (초당 단위 거리)")]
        private float speed = 1f;
        [SerializeField, Tooltip("Agent가 특정 위치에 있음을 판단하는 허용 오차")]
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

        // 현재 경로(격자 노드들의 월드 좌표 목록)와 인덱스
        private List<Node> path;
        private Node lastNode;
        private int nextNodeIndex;

        private void Start()
        {
            pathManager = MLApp.Instance.PathManager;

            var homeNode = pathManager.FindNearestNode(transform.position);
            lastNode = homeNode;
        }

        public override void OnEpisodeBegin()
        {
            path = pathManager.FindPath(lastNode);
            nextNodeIndex = 0;

            if (path.Count <= 1)
            {
                Debug.LogWarning("Path must include at least 2 nodes");
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
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
            Vector3 actionDir = new Vector3(dirX, 0, dirZ).normalized;

            Vector3 nextPoint = path[nextNodeIndex].position;
            Vector3 desiredDir = (nextPoint - transform.position).normalized;

            transform.position += actionDir * speed * Time.deltaTime;

            // Segment를 벗어나려고 시도하면 에피소드 종료
            if (Vector3.Dot(actionDir, desiredDir) < 0.8f)
            {
                AddReward(rewardForOffSegment);
                EndEpisode();

                return;
            }

            // 경유지에 도착하면 다음 경유지를 요청
            if (Vector3.Distance(transform.position, nextPoint) <= tolerance)
            {
                // 목적지에 도착시 보상을 주고 에피소드 종료
                if (nextNodeIndex == path.Count - 1)
                {
                    particle.Play();
                    AddReward(rewardForArrived);
                    EndEpisode();
                }
                else
                {
                    AddReward(rewardForNextPoint);
                }

                lastNode = path[nextNodeIndex++];
            }
        }

        // 휴리스틱: 테스트 시 현재 Path의 목적지로 이동
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            Vector3 direction = (path[nextNodeIndex].position - transform.position).normalized;
            continuousActionsOut[0] = direction.x;
            continuousActionsOut[1] = direction.z;
        }

        // 다른 Agent와 충돌 시 에피소드 종료
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Agent"))
            {
                AddReward(rewardForCollision);
                EndEpisode();
            }
        }
    }
}