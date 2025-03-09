using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace MAFF
{
    public class PathManager : MonoBehaviour
    {
        private PathFinder pathFinder;

        [Header("Node List")]
        [Tooltip("배치할 Node들을 순서대로 드래그합니다.")]
        [SerializeField]
        private List<Node> nodes = new List<Node>();

        [Header("Segment Settings")]
        [Tooltip("LineRenderer의 선 두께")]
        [SerializeField]
        public float lineWidth = 0.2f;
        [Tooltip("Segment에 사용할 재질")]
        [SerializeField]
        public Material lineMaterial;

        // 생성된 Segment들을 저장하는 리스트
        [ReadOnly, SerializeField]
        private List<Segment> segments = new List<Segment>();

        public void Initialize()
        {
            if (nodes.Count <= 1 || segments.Count == 0)
            {
                Debug.LogError("Not enough nodes and segments to construct evironment");
                Application.Quit();
            }

            pathFinder = new(nodes, segments);
        }

        private Node GetRandomNode(Node node)
        {
            var idx = Random.Range(0, nodes.Count);
            var targetNode = nodes[idx];

            if (node == nodes[idx])
            {
                return nodes[(idx + 1) % nodes.Count];
            }

            return targetNode;
        }

        public Node FindNearestNode(Vector3 position)
        {
            Node targetNode = nodes[0];
            float minDist = float.PositiveInfinity;

            foreach (var node in nodes)
            {
                var dist = Vector3.Distance(position, node.position);
                if (dist < minDist)
                {
                    targetNode = node;
                    minDist = dist;
                }
            }

            return targetNode;
        }

        public List<Node> FindPath(Node startNode)
        {
            return pathFinder.FindPath(startNode, GetRandomNode(startNode), MLApp.Instance.PathAlgorithm);
        }

        [Button]
        public void CreateSegments()
        {
            // nodes 리스트의 순서대로 인접한 두 노드를 연결하는 Segment 생성
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Node startNode = nodes[i];
                Node endNode = nodes[i + 1];
                CreateSegment(startNode, endNode);
            }
        }

        private void CreateSegment(Node startNode, Node endNode)
        {
            // 새로운 Segment GameObject 생성
            GameObject segmentObj = new GameObject($"Segment_{startNode.name}_{endNode.name}");
            segmentObj.transform.parent = this.transform;

            // LineRenderer 추가 및 설정
            LineRenderer lr = segmentObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, startNode.position);
            lr.SetPosition(1, endNode.position);
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.useWorldSpace = true;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            if (lineMaterial != null)
                lr.material = lineMaterial;
            else
                lr.material = new Material(Shader.Find("Sprites/Default"));

            // SegmentComponent 추가해 두 노드 정보를 기록
            Segment segComp = segmentObj.AddComponent<Segment>();
            segComp.start = startNode;
            segComp.end = endNode;

            segments.Add(segComp);
        }

        [Button]
        public void UpdateSegmentsByChildren()
        {
            segments = GetComponentsInChildren<Segment>().ToList();
        }

        [Button]
        public void ClearSegments()
        {
            // 이전에 생성된 세그먼트들을 모두 삭제
            foreach (Segment seg in segments)
            {
                if (seg != null)
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.delayCall += () => { DestroyImmediate(seg.gameObject); };
#else
                Destroy(seg.gameObject);
#endif
                }
            }
            segments.Clear();
        }
    }
}