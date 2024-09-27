using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Unit : AStar
{
    public GameObject target;
    public float moveSpeed;

    private Rigidbody2D rb2D;
    private Coroutine MoveIE;

    private bool isTargetMoved = false;
    private Vector2 lastTargetPos;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        Init();
    }

    void Update()
    {

        // 타겟이 움직였을 경우 알고리즘의 생성과 패스파인딩을 다시 시행한다.
        isTargetMoved = checkingIsTargetMoved();

        if (isTargetMoved)
        {
            Init();
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
            MoveIE = StartCoroutine(Moving(i));
            yield return MoveIE;
        }
    }

    IEnumerator Moving(int i)
    {
        while (truncatedPos(transform.position) != finalPath[i])
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
}
