using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MlAgent
{
    public class PathManager : MonoBehaviour, Service
    {
        // 모든 Segment 데이터를 저장할 리스트
        private List<SegmentData> segments = new();
        // 거점(waypoint) 목록: 각 LineRenderer의 시작점
        private List<Vector3> waypoints = new();

        public void OnInit()
        {
            UpdatePathData();
        }

        public void UpdatePathData()
        {
            segments.Clear();
            waypoints.Clear();

            LineRenderer[] lrList = GetComponentsInChildren<LineRenderer>();
            foreach (LineRenderer lr in lrList)
            {
                // LineRenderer가 최소 2개의 포인트를 가지고 있다고 가정
                if (lr.positionCount >= 2)
                {
                    Vector3 start = lr.GetPosition(0);
                    Vector3 end = lr.GetPosition(1);
                    segments.Add(new SegmentData(start, end));
                    // 중복되지 않는 거점만 등록
                    if (!waypoints.Contains(start))
                    {
                        waypoints.Add(start);
                    }
                    // 중복되지 않는 거점만 등록
                    if (!waypoints.Contains(end))
                    {
                        waypoints.Add(end);
                    }
                }
            }
        }

        public void GetGrid(out Vector3 min, out Vector3 max)
        {
            min = Vector3.negativeInfinity;
            max = Vector3.positiveInfinity;

            foreach (SegmentData segment in segments)
            {
                var segMin = Vector3.Min(segment.start, segment.end);
                var segMax = Vector3.Max(segment.start, segment.end);

                min = Vector3.Min(min, segMin);
                max = Vector3.Max(max, segMax);
            }
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

        // 현재 위치에서 가장 가까운 경로상의 점을 찾는 함수
        public Vector3 GetNearestPointOnPath(Vector3 position)
        {
            Vector3 nearestPoint = position;
            float minDistance = Mathf.Infinity;
            foreach (SegmentData segment in segments)
            {
                Vector3 candidate = ClosestPointOnSegment(segment.start, segment.end, position);
                float distance = Vector3.Distance(position, candidate);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPoint = candidate;
                }
            }
            return nearestPoint;
        }

        // 주어진 선분(start~end) 위에서 point와 가장 가까운 점 계산
        Vector3 ClosestPointOnSegment(Vector3 start, Vector3 end, Vector3 point)
        {
            Vector3 segment = end - start;
            float t = Vector3.Dot(point - start, segment) / segment.sqrMagnitude;
            t = Mathf.Clamp01(t);
            return start + t * segment;
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