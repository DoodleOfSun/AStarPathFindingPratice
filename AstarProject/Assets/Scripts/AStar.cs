using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStar : MonoBehaviour
{
    private class Cell
    {
        public Vector2 position;
        public int fCost = int.MaxValue;
        public int gCost = int.MaxValue;
        public int hCost = int.MaxValue;
        public Vector2 connection;
        public bool isWall;

        public Cell(Vector2 pos, bool wall)
        {
            position = pos;
            isWall = wall;
        }
    }


    public Tilemap blockingTile;
    public int cellWidth;
    public int cellHeight;
    public GameObject target;
    public GameObject unit;
    

    private List<Vector2> cellsToSearch;        // 앞으로 찾아내야 할 셀
    private List<Vector2> searchedCells;        // 이미 조사한 셀 위치
    private List<Vector2> finalPath;            // 찾아낸 최적화된 길

    private Dictionary<Vector2, Cell> cells;

    void Start()
    {
        Initialize(truncatedPos(unit.transform.position), truncatedPos(target.transform.position));
    }

    void Update()
    {
        FindPath(truncatedPos(unit.transform.position), truncatedPos(target.transform.position));
    }

    private Vector2 truncatedPos(Vector3 pos)
    {
        float truncatedX = (int)pos.x;
        float truncatedY = (int)pos.y;

        if (truncatedX >= 0)
        {
            truncatedX += 0.5f;
        }
        else
        {
            truncatedX -= 0.5f;
        }

        if (truncatedY >= 0)
        {
            truncatedY += 0.5f;
        }
        else
        {
            truncatedY -= 0.5f;
        }
        
        return new Vector2(truncatedX, truncatedY);
    }

    private void Initialize(Vector2 startPos, Vector2 endPos)
    {
        searchedCells = new List<Vector2>();
        cellsToSearch = new List<Vector2> { startPos };
        finalPath = new List<Vector2>();

        cells = new Dictionary<Vector2, Cell>();

        foreach (var pos in blockingTile.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            Vector3 place = blockingTile.CellToWorld(localPlace) + new Vector3(0.5f, 0.5f, 0f);
            if (blockingTile.HasTile(localPlace))
            {
                cells.Add(place, new Cell(place, true));
            }
            else
            {
                cells.Add(place, new Cell(place, false));
            }
        }
    }

    private void FindPath(Vector2 startPos, Vector2 endPos)
    {

        Cell startCell = cells[startPos];
        startCell.gCost = 0;
        startCell.hCost = GetDistance(startPos, endPos);
        startCell.fCost = GetDistance(startPos, endPos);

        while (cellsToSearch.Count > 0)
        {
            Vector2 cellToSearch = cellsToSearch[0];

            foreach (Vector2 pos in cellsToSearch)
            {
                Cell c = cells[pos];
                if (c.fCost < cells[cellToSearch].fCost ||
                    c.fCost == cells[cellToSearch].fCost && c.hCost == cells[cellToSearch].hCost)
                {
                    cellToSearch = pos;
                }
            }


            cellsToSearch.Remove(cellToSearch);
            searchedCells.Add(cellToSearch);

            if (cellToSearch == endPos)
            {
                Cell pathCell = cells[endPos];

                while (pathCell.position != startPos)
                {
                    finalPath.Add(pathCell.position);
                    pathCell = cells[pathCell.connection];
                }

                finalPath.Add(startPos);
                return;
            }

            SearchCellNeighbors(cellToSearch, endPos);
        }
        
    }

    private int GetDistance(Vector2 pos1, Vector2 pos2)
    {
        Vector2Int dist = new Vector2Int(Mathf.Abs((int)pos1.x - (int)pos2.x), Mathf.Abs((int)pos1.y - (int)pos2.y));

        int lowest = Mathf.Min(dist.x, dist.y);
        int highest = Mathf.Max(dist.x, dist.y);

        int horizontalMoveRequired = highest - lowest;

        return lowest * 14 + horizontalMoveRequired * 10;
    }

    private void SearchCellNeighbors(Vector2 cellPos, Vector2 endPos)
    {
        for (float x = cellPos.x - cellWidth; x <= cellWidth + cellPos.x; x += cellHeight)
        {
            for (float y = cellPos.y - cellHeight; y <= cellHeight + cellPos.y; y += cellHeight)
            {
                Vector2 neighborPos = new Vector2(x, y);
                if (cells.TryGetValue(neighborPos, out Cell c) && !searchedCells.Contains(neighborPos) && !cells[neighborPos].isWall)
                {
                    int GcostToNeighbor = cells[cellPos].gCost + GetDistance(cellPos, neighborPos);
                    if (GcostToNeighbor < cells[neighborPos].gCost)
                    {
                        Cell neighborNode = cells[neighborPos];

                        neighborNode.connection = cellPos;
                        neighborNode.gCost = GcostToNeighbor;
                        neighborNode.hCost = GetDistance(neighborPos, endPos);
                        neighborNode.fCost = neighborNode.gCost + neighborNode.hCost;

                        if (!cellsToSearch.Contains(neighborPos))
                        {
                            cellsToSearch.Add(neighborPos);
                        }
                    }
                }
            }
        }
    }

    // 알고리즘 가시화
    private void OnDrawGizmos()
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

            Gizmos.DrawCube(kvp.Key + (Vector2)transform.position, new Vector3(cellWidth, cellHeight));
        }


    }
}
