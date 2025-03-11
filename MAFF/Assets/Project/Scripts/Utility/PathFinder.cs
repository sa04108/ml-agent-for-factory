using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace MAFF
{
    public enum PathAlgorithm { AStar, Dijkstra }

    public class PathFinder
    {
        private List<Node> nodes;
        private List<Segment> segments;

        public PathFinder(List<Node> nodes, List<Segment> segments)
        {
            this.nodes = nodes;
            this.segments = segments;
        }

        public List<Node> FindPath(Node start, Node goal, PathAlgorithm algorithm)
        {
            // 맵 빌더의 노드와 세그먼트를 기반으로 그래프를 생성합니다.
            Dictionary<Node, List<Node>> graph = BuildGraph();

            // A* 알고리즘 또는 Dijkstra 알고리즘을 실행합니다.
            return SearchPath(graph, start, goal, algorithm);
        }

        private Dictionary<Node, List<Node>> BuildGraph()
        {
            Dictionary<Node, List<Node>> graph = new Dictionary<Node, List<Node>>();

            // MapBuilder의 노드를 그래프의 노드로 초기화
            foreach (Node node in nodes)
            {
                if (!graph.ContainsKey(node))
                    graph[node] = new List<Node>();
            }

            // 각 세그먼트의 연결 정보를 그래프에 추가 (양방향)
            foreach (Segment seg in segments)
            {
                if (seg == null) continue;
                Node a = seg.start;
                Node b = seg.end;
                if (a != null && b != null)
                {
                    if (!graph[a].Contains(b))
                        graph[a].Add(b);
                    if (!graph[b].Contains(a))
                        graph[b].Add(a);
                }
            }
            return graph;
        }

        // A* 또는 Dijkstra 알고리즘으로 경로 탐색
        private List<Node> SearchPath(Dictionary<Node, List<Node>> graph, Node start, Node goal, PathAlgorithm algorithm)
        {
            // openSet: 평가할 노드 목록
            List<Node> openSet = new List<Node> { start };

            // 각 노드에 대한 최단 경로 비용 및 경로 재구성을 위한 딕셔너리
            Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
            Dictionary<Node, float> gScore = new Dictionary<Node, float>();  // 시작부터 현재까지의 실제 비용
            Dictionary<Node, float> fScore = new Dictionary<Node, float>();  // gScore + 휴리스틱(추정 비용)

            foreach (Node node in graph.Keys)
            {
                gScore[node] = Mathf.Infinity;
                fScore[node] = Mathf.Infinity;
            }
            gScore[start] = 0;
            fScore[start] = Heuristic(start, goal, algorithm);

            while (openSet.Count > 0)
            {
                // fScore가 가장 낮은 노드를 current로 선택
                Node current = openSet[0];
                foreach (Node node in openSet)
                {
                    if (fScore[node] < fScore[current])
                        current = node;
                }

                if (current == goal)
                {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);

                foreach (Node neighbor in graph[current])
                {
                    float tentative_gScore = gScore[current] + Vector3.Distance(current.position, neighbor.position);
                    if (tentative_gScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentative_gScore;
                        fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, goal, algorithm);
                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            Debug.LogWarning("경로를 찾지 못했습니다.");
            return null;
        }

        private float Heuristic(Node a, Node b, PathAlgorithm algorithm)
        {
            // A*일 경우 Euclidean distance를 휴리스틱으로 사용하고,
            // Dijkstra(=휴리스틱 0)일 경우 0을 반환합니다.

            if (algorithm == PathAlgorithm.AStar)
                return Vector3.Distance(a.position, b.position);
            else
                return 0f;
        }

        private List<Node> ReconstructPath(Dictionary<Node, Node> cameFrom, Node current)
        {
            List<Node> totalPath = new List<Node> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Add(current);
            }
            totalPath.Reverse();
            return totalPath;
        }
    }
}