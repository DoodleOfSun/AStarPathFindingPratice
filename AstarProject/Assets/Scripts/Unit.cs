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
        FindPath(truncatedPos(this.transform.position), truncatedPos(target.transform.position));
        lastTargetPos = target.transform.position;
        StartCoroutine(moveObject());
        finalPath.Reverse();
    }

    void Update()
    {

        // Ÿ���� �������� ��� �˰����� ������ �н����ε��� �ٽ� �����Ѵ�.
        /*
        if (checkingIsTargetMoved())
        {
            FindPath(truncatedPos(this.transform.position), truncatedPos(target.transform.position));
            lastTargetPos = target.transform.position;
        }*/

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
            // �����̴� ��ġ�� �밢���� ��� ó��
            /*
            if ()
            {

            }
            else
            {
                rb2D.MovePosition(Vector3.MoveTowards(transform.position, finalPath[i], moveSpeed * Time.deltaTime));
                yield return null;
            }
            */

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
