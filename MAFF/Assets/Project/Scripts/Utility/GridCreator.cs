using UnityEngine;
using Sirenix.OdinInspector;

public class GridCreator : MonoBehaviour
{
    [Header("격자 설정")]
    [Tooltip("가로(열)의 셀 개수")]
    public int columns = 3; // M
    [Tooltip("세로(행)의 셀 개수")]
    public int rows = 3;    // N
    [Tooltip("한 칸의 길이")]
    public float cellSize = 10f;
    [Tooltip("격자 굵기")]
    public float lineWidth = 0.2f;

    // 생성된 자식 오브젝트들을 구분하기 위한 접두어
    private const string gridLinePrefix = "GridLine_";

    // 컨텍스트 메뉴에서 실행할 수 있는 메서드
    [Button("Generate Grid")]
    public void GenerateGrid()
    {
        // 기존의 격자 라인이 있다면 모두 삭제
        ClearGridLines();

        // 수직 세그먼트 생성
        // 격자의 x좌표 경계: 0 ~ columns (총 columns+1)
        // 각 경계는 rows 개의 세그먼트로 나뉨
        for (int i = 0; i <= columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Vector3 start = new Vector3(i * cellSize, 0f, j * cellSize);
                Vector3 end = new Vector3(i * cellSize, 0f, (j + 1) * cellSize);
                CreateEdge($"Vertical_{i}_{j}", start, end);
            }
        }

        // 수평 세그먼트 생성
        // 격자의 z좌표 경계: 0 ~ rows (총 rows+1)
        // 각 경계는 columns 개의 세그먼트로 나뉨
        for (int j = 0; j <= rows; j++)
        {
            for (int i = 0; i < columns; i++)
            {
                Vector3 start = new Vector3(i * cellSize, 0f, j * cellSize);
                Vector3 end = new Vector3((i + 1) * cellSize, 0f, j * cellSize);
                CreateEdge($"Horizontal_{j}_{i}", start, end);
            }
        }
    }

    // 주어진 이름과 시작, 끝 좌표로 Line Renderer 생성
    private void CreateEdge(string name, Vector3 start, Vector3 end)
    {
        GameObject edgeObj = new GameObject(gridLinePrefix + name);
        edgeObj.transform.SetParent(transform);
        LineRenderer lr = edgeObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = false;  // 부모의 로컬 좌표 사용
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.widthMultiplier = lineWidth;
        // 필요에 따라 재질(Material) 등을 추가 설정 가능
    }

    // 기존의 격자 라인을 삭제하는 함수
    [Button("Clear Grid")]
    public void ClearGridLines()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith(gridLinePrefix))
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}