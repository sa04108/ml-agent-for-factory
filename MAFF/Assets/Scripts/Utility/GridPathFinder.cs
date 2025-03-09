using System.Collections.Generic;
using UnityEngine;

namespace MAFF
{
    public enum PathAlgorithm { None, AStar, Dijkstra }

    public class GridPathFinder
    {
        private GridInfo gridInfo;

        public GridPathFinder(GridInfo grid, PathAlgorithm algorithm)
        {
            gridInfo = grid;
            this.algorithm = algorithm;
        }

        [Tooltip("길찾기 알고리즘 선택 (Dijkstra는 휴리스틱을 사용하지 않음)")]
        private PathAlgorithm algorithm = PathAlgorithm.AStar;

        /// <summary>
        /// 주어진 월드 좌표(startPos, targetPos)를 가장 가까운 격자 노드로 변환한 후
        /// 해당 노드들 사이의 경로(노드 리스트)를 찾아 반환합니다.
        /// </summary>
        public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
        {
            // 노드의 인덱스로 변환 (노드의 좌표는 (i*cellSize, 0, j*cellSize))
            int startX = Mathf.RoundToInt(startPos.x / gridInfo.CellSize);
            int startY = Mathf.RoundToInt(startPos.z / gridInfo.CellSize);
            int targetX = Mathf.RoundToInt(targetPos.x / gridInfo.CellSize);
            int targetY = Mathf.RoundToInt(targetPos.z / gridInfo.CellSize);

            // 격자 범위를 벗어나지 않도록 클램프
            startX = Mathf.Clamp(startX, 0, gridInfo.Columns);
            startY = Mathf.Clamp(startY, 0, gridInfo.Rows);
            targetX = Mathf.Clamp(targetX, 0, gridInfo.Columns);
            targetY = Mathf.Clamp(targetY, 0, gridInfo.Rows);

            // 2차원 배열로 노드 생성 (총 (columns+1) x (rows+1) 노드)
            Node[,] grid = new Node[gridInfo.Columns + 1, gridInfo.Rows + 1];
            for (int i = 0; i <= gridInfo.Columns; i++)
            {
                for (int j = 0; j <= gridInfo.Rows; j++)
                {
                    Vector3 worldPos = new Vector3(i * gridInfo.CellSize, 0f, j * gridInfo.CellSize);
                    grid[i, j] = new Node(i, j, worldPos);
                }
            }

            Node startNode = grid[startX, startY];
            Node targetNode = grid[targetX, targetY];

            // A* 혹은 Dijkstra (휴리스틱 0)로 경로 탐색
            List<Node> pathNodes = FindPathNodes(startNode, targetNode, grid);
            List<Vector3> path = new List<Vector3>();
            if (pathNodes != null)
            {
                foreach (Node node in pathNodes)
                {
                    path.Add(node.worldPosition);
                }
            }
            return path;
        }

        List<Node> FindPathNodes(Node startNode, Node targetNode, Node[,] grid)
        {
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            startNode.gCost = 0;
            startNode.hCost = GetHeuristic(startNode, targetNode);
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                // openSet에서 fCost(= gCost + hCost)가 가장 낮은 노드를 선택
                Node currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost ||
                        (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    return RetracePath(startNode, targetNode);
                }

                foreach (Node neighbor in GetNeighbors(currentNode, grid))
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    float tentativeGCost = currentNode.gCost + Distance(currentNode, neighbor);
                    if (tentativeGCost < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = tentativeGCost;
                        // Dijkstra는 휴리스틱 없이 진행
                        neighbor.hCost = (algorithm == PathAlgorithm.AStar) ? GetHeuristic(neighbor, targetNode) : 0;
                        neighbor.parent = currentNode;

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }
            return null;
        }

        float GetHeuristic(Node a, Node b)
        {
            // 맨해튼 거리 (노드 간 이동 비용은 1로 가정)
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        float Distance(Node a, Node b)
        {
            return 1f;
        }

        List<Node> GetNeighbors(Node node, Node[,] grid)
        {
            List<Node> neighbors = new List<Node>();
            int x = node.x;
            int y = node.y;

            // 4방향 (상하좌우) 노드 추가
            if (x - 1 >= 0) neighbors.Add(grid[x - 1, y]);
            if (x + 1 <= gridInfo.Columns) neighbors.Add(grid[x + 1, y]);
            if (y - 1 >= 0) neighbors.Add(grid[x, y - 1]);
            if (y + 1 <= gridInfo.Rows) neighbors.Add(grid[x, y + 1]);

            return neighbors;
        }

        List<Node> RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;
            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            path.Add(startNode);
            path.Reverse();
            return path;
        }

        public class Node
        {
            public int x;
            public int y;
            public Vector3 worldPosition;
            public float gCost = Mathf.Infinity;
            public float hCost = Mathf.Infinity;
            public Node parent;

            public Node(int _x, int _y, Vector3 _worldPos)
            {
                x = _x;
                y = _y;
                worldPosition = _worldPos;
            }

            public float fCost { get { return gCost + hCost; } }
        }
    }
}