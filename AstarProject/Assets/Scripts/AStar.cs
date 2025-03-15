using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStar : MonoBehaviour
{
    public class Cell
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

    public bool isSerching4Cells;

    public Tilemap blockingTile;
    public int cellWidth;
    public int cellHeight;

    private List<Vector2> cellsToSearch;        // 앞으로 찾아내야 할 셀
    private List<Vector2> searchedCells;        // 이미 조사한 셀 위치
    protected List<Vector2> finalPath;            // 찾아낸 최적화된 길

    protected Dictionary<Vector2, Cell> cells;    // 타일맵을 딕셔너리로 관리

    protected virtual void Start()
    {
        //isSerching4Cells = false;
    }

    // 0.5씩 더하거나 빼서 중간으로 맞춰주는 함수
    protected Vector2 truncatedPos(Vector3 pos)
    {
        float truncatedX = (int)pos.x;
        float truncatedY = (int)pos.y;

        if (truncatedX > 0)
        {
            truncatedX += 0.5f;
        }
        else if(truncatedX < 0)
        {
            truncatedX -= 0.5f;
        }
        else if (truncatedX == 0 && pos.x > 0)
        {
            truncatedX += 0.5f;
        }
        else
        {
            truncatedX -= 0.5f;
        }

        if (truncatedY > 0)
        {
            truncatedY += 0.5f;
        }
        else if (truncatedY < 0)
        {
            truncatedY -= 0.5f;
        }
        else if (truncatedY == 0 && pos.y > 0)
        {
            truncatedY += 0.5f;
        }
        else
        {
            truncatedY -= 0.5f;
        }

        return new Vector2(truncatedX, truncatedY);
    }


    protected void FindPath(Vector2 startPos, Vector2 endPos)
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

    protected int GetDistance(Vector2 pos1, Vector2 pos2)
    {
        // 휴리스틱 추정을 위한 거리 계산
        Vector2Int dist = new Vector2Int(Mathf.Abs((int)pos1.x - (int)pos2.x), Mathf.Abs((int)pos1.y - (int)pos2.y));

        int lowest = Mathf.Min(dist.x, dist.y);
        int highest = Mathf.Max(dist.x, dist.y);

        int horizontalMoveRequired = highest - lowest;

        return lowest * 14 + horizontalMoveRequired * 10;
    }

    protected void SearchCellNeighbors(Vector2 cellPos, Vector2 endPos)
    {
        // Grid 내에서 상하좌우 4칸만 검색하는 경우
        if (isSerching4Cells)
        {
            Vector2[] serchingDirections = new Vector2[]
            {
            new Vector2(cellWidth, 0),
            new Vector2(-cellWidth, 0),
            new Vector2(0, cellHeight),
            new Vector2(0, -cellHeight)
            };

            foreach (var dir in serchingDirections)
            {
                Vector2 neighborPos = cellPos + dir;

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


        // 8칸을 전부 검색하는 경우
        else if(!isSerching4Cells)
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
    }
}
