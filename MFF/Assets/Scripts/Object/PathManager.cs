using System.Collections.Generic;
using UnityEngine;

namespace MlAgent
{
    public struct GridInfo
    {
        public int Columns;
        public int Rows;
        public float CellSize;
    }

    public class PathManager : MonoBehaviour, Service
    {
        private GridPathFinder pathFinder;
        // 모든 Segment 데이터를 저장할 리스트
        private List<SegmentData> segments = new();
        // 거점(waypoint) 목록: 각 LineRenderer의 시작점
        private List<Vector3> waypoints = new();

        private GridInfo grid = new();
        public GridInfo Grid => grid;
        private PathAlgorithm algorithm;

        // 계산된 격자 정보
        public int Columns => grid.Columns;
        public int Rows => grid.Rows;
        public float CellSize => grid.CellSize;

        public void OnInit()
        {
            UpdatePathData();

            pathFinder = new(grid, algorithm);
        }

        public void SetPathAlgorithm(PathAlgorithm algorithm)
        {
            this.algorithm = algorithm;
        }

        public void UpdatePathData()
        {
            segments.Clear();
            waypoints.Clear();

            LineRenderer[] lrList = GetComponentsInChildren<LineRenderer>();
            // 수직 Edge의 x좌표, 수평 Edge의 z좌표 (중복 없이 저장)
            List<float> verticalXCoords = new List<float>();
            List<float> horizontalZCoords = new List<float>();

            foreach (LineRenderer lr in lrList)
            {
                if (lr.positionCount < 2)
                    continue;

                Vector3 start = lr.GetPosition(0);
                Vector3 end = lr.GetPosition(1);

                // Segment 데이터 등록
                segments.Add(new SegmentData(start, end));

                // 수직 Edge: x 좌표 차이가 거의 없으면
                if (Mathf.Abs(start.x - end.x) < 0.001f)
                {
                    if (!ContainsApproximately(verticalXCoords, start.x))
                    {
                        verticalXCoords.Add(start.x);
                    }
                }
                // 수평 Edge: z 좌표 차이가 거의 없으면
                if (Mathf.Abs(start.z - end.z) < 0.001f)
                {
                    if (!ContainsApproximately(horizontalZCoords, start.z))
                    {
                        horizontalZCoords.Add(start.z);
                    }
                }
            }

            // 정렬 (오름차순)
            verticalXCoords.Sort();
            horizontalZCoords.Sort();

            // 열(columns): 수직선 개수 - 1
            if (verticalXCoords.Count > 1)
            {
                grid.Columns = verticalXCoords.Count - 1;
                // 인접한 x 좌표 차이를 cellSize로 사용
                grid.CellSize = verticalXCoords[1] - verticalXCoords[0];
            }

            // 행(rows): 수평선 개수 - 1
            if (horizontalZCoords.Count > 1)
            {
                grid.Rows = horizontalZCoords.Count - 1;
                float horizontalCellSize = horizontalZCoords[1] - horizontalZCoords[0];
                // cellSize가 아직 0이면 설정, 아니라면 평균 처리(격자가 정사각형이라면 두 값는 같을 것)
                if (Mathf.Approximately(CellSize, 0f))
                    grid.CellSize = horizontalCellSize;
                else
                    grid.CellSize = (CellSize + horizontalCellSize) * 0.5f;
            }

            // 격자 교차점(노드)를 생성: 각 수직선과 수평선의 교차점
            for (int i = 0; i < verticalXCoords.Count; i++)
            {
                for (int j = 0; j < horizontalZCoords.Count; j++)
                {
                    Vector3 node = new Vector3(verticalXCoords[i], 0f, horizontalZCoords[j]);
                    waypoints.Add(node);
                }
            }
        }

        // 리스트에 이미 value와 거의 동일한 값이 존재하는지 확인 (tolerance 사용)
        private bool ContainsApproximately(List<float> list, float value)
        {
            foreach (float f in list)
            {
                if (Mathf.Abs(f - value) < 0.001f)
                    return true;
            }
            return false;
        }

        public Vector3 GetRandomWayPoint()
        {
            if (waypoints.Count == 0)
            {
                Debug.LogWarning("WayPoints are not set");
                return Vector3.zero;
            }

            return waypoints[Random.Range(0, waypoints.Count)];
        }

        public List<Vector3> RequestNewPath(Vector3 position)
        {
            // 격자 내 무작위 노드를 목적지로 선택 (노드는 0~columns, 0~rows)
            int targetX = Random.Range(0, grid.Columns + 1);
            int targetY = Random.Range(0, grid.Rows + 1);
            Vector3 destination = new Vector3(targetX * grid.CellSize, position.y, targetY * grid.CellSize);

            // 현재 Agent의 위치와 목적지 사이의 경로를 탐색
            return pathFinder.FindPath(position, destination);
        }
    }

    // 하나의 선분(Segment) 정보를 담는 클래스
    public class SegmentData
    {
        public Vector3 start;
        public Vector3 end;

        public SegmentData(Vector3 start, Vector3 end)
        {
            this.start = start;
            this.end = end;
        }

        // 주어진 point가 이 선분 위에 있는지 (tolerance 범위 내) 확인하는 메서드
        public bool IsPointOnSegment(Vector3 point, float tolerance = 0.1f)
        {
            Vector3 segDir = (end - start).normalized;
            float segLength = Vector3.Distance(start, end);
            Vector3 projected = Vector3.Project(point - start, segDir);
            float projectedLength = projected.magnitude;
            // 프로젝션 길이가 선분의 길이 범위 내에 있는지 체크
            if (projectedLength < 0 || projectedLength > segLength)
                return false;
            Vector3 closestPoint = start + segDir * projectedLength;
            // 선분에서의 최단 거리와 tolerance 비교
            return Vector3.Distance(point, closestPoint) <= tolerance;
        }
    }
}