using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class Unit : AStar
{
    public static Unit instance;
    public GameObject target;
    public float moveSpeed;

    private Rigidbody2D rb2D;
    private Coroutine MoveIE;

    private bool isTargetMoved = false;
    private Vector2 lastTargetPos;

    private LineRenderer lineRenderer;
    private Color lineColor = Color.red;

    List<Vector3> ListForDrawing = new List<Vector3>();

    protected override void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        Init();

        base.Start();
    }

    void Update()
    {
        // 타겟이 움직였을 경우 알고리즘의 생성과 패스파인딩을 다시 시행한다.
        isTargetMoved = checkingIsTargetMoved();

        if (isTargetMoved)
        {
            Init();
            DrawPath();
            VisualizePath(ListForDrawing);
            isTargetMoved = false;
        }

    }

    private void Init()
    {
        FindPath(truncatedPos(this.transform.position), truncatedPos(target.transform.position));
        lastTargetPos = target.transform.position;
        StartCoroutine(moveObject());
        finalPath.Reverse();
    }

    IEnumerator moveObject()
    {
        for (int i = 0; i < finalPath.Count; i++)
        {
            if (finalPath.Count <= i)
            {
                Debug.Log("FinalPath.Couunt보다 I가 커짐, 이동함수 강제 종료");
                break;
            }
            MoveIE = StartCoroutine(Moving(i));
            yield return MoveIE;
        }
    }

    IEnumerator Moving(int i)
    {
        while (i < finalPath.Count && truncatedPos(transform.position) != finalPath[i])
        {
            rb2D.MovePosition(Vector3.MoveTowards(transform.position, finalPath[i], moveSpeed * Time.deltaTime));
            yield return null;
        }
    }


    private bool checkingIsTargetMoved()
    {
        if (lastTargetPos != (Vector2)target.transform.position)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    protected void OnDrawGizmos()
    {
        if (cells == null)
        {
            return;
        }

        foreach (KeyValuePair<Vector2, Cell> kvp in cells)
        {

            if (!kvp.Value.isWall)
            {
                Gizmos.color = Color.white;
            }
            else
            {
                Gizmos.color = Color.black;
            }


            if (finalPath.Contains(kvp.Key))
            {
                Gizmos.color = Color.magenta;
            }

            Gizmos.DrawCube(kvp.Key, new Vector3(cellWidth, cellHeight));
        }
    }

    public void SwitchingTrackingStyle()
    {
        if (isSerching4Cells)
        {
            isSerching4Cells = false;
        }
        else
        {
            isSerching4Cells = true;
        }
    }
    // 경로를 숨기기 위한 함수
    private void HidePath()
    {
        lineRenderer.positionCount = 0;  // LineRenderer의 포인트 수를 0으로 설정하여 경로 숨기기
    }


    private void VisualizePath(List<Vector3> path)
    {
        if (path == null || path.Count == 0) return;

        lineRenderer.positionCount = path.Count;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        for (int i = 0; i < path.Count; i++)
        {
            lineRenderer.SetPosition(i, path[i]);
        }
    }

    public void DrawPath()
    {
        foreach (var cell in cells)
        {
            Vector2 data = cell.Key;
            ListForDrawing.Add(data);
        }
    }
}
