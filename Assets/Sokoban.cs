using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum CellType
{
    Wall,
    Box,
    Button,
    Player,
    Empty
}

[System.Serializable]
public class Cell
{
    [HideInInspector]
    public Vector2Int position;
    public CellType value;
    public Cell(Vector2Int position,CellType  value)
    {
        this.position = position;
        this.value = value;
    }
}

public class MapSokoban : DyProg<Cell, Vector2Int>
{
    public List<Cell> board;

    public MapSokoban(List<Cell> b)
    {
        this.board = b;
    }

    public override float Reward(Cell state)
    {
        throw new System.NotImplementedException();
    }

    public override (Cell, float) Simulate(Cell state, Vector2Int action)
    {
        throw new System.NotImplementedException();
    }

    public override List<Vector2Int> GetAllPossibleMoves(Cell state)
    {
        throw new System.NotImplementedException();
    }

    public override bool isEgal(Vector2Int a, Vector2Int b)
    {
        throw new System.NotImplementedException();
    }
}

public class Sokoban : MonoBehaviour
{
    public static Vector2Int[] random_direction = new Vector2Int[4] { Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up };

    public Vector2Int BoxSize;
    public float CellSize;
    public float gamma = .9f;

    [Header("Create Map")]
    public List<Cell> cells;
 
    MapSokoban map;

    public void Init()
    {
        //Debug.ClearDeveloperConsole();
        cells = new List<Cell>();
        for (int i = 0; i < BoxSize.x; i++)
        {
            for (int j = 0; j < BoxSize.y; j++)
            {
                Cell cell = new Cell(new Vector2Int(i,j),CellType.Empty);
                cells.Add(cell);
            }    
        }
        
        /*
        List<Cell> board = new List<Cell>();
        List<Mur> wp = new List<Mur>();
        List<Box> sbp = new List<Box>();
        List<Buton> bp = new List<Buton>();
            
        foreach(Vector2Int v in cells)
        {
            board.Add(new Cell(v, 0));
        }

        foreach (Vector2Int v in wallsPosition)
        {
            wp.Add(new Mur(v));
        }

        foreach (Vector2Int v in startBoxsPositions)
        {
            sbp.Add(new Box(v));
        }

        foreach (Vector2Int v in butonsPositions)
        {
            bp.Add(new Buton(v));
        }

        map = new MapSokoban(board, wp, sbp, bp, startPositionPlayer);
        */
    }

    void OnDrawGizmos()
    {
        
   

        for (int i = 0; i < cells.Count; i++)
        {

            switch (cells[i].value)
            {
                case CellType.Empty :
                    Gizmos.color = Color.white;
                    break;
                case CellType.Player :
                    Gizmos.color = Color.blue;
                    Gizmos.DrawCube(new Vector3(cells[i].position.x, 0, cells[i].position.y), Vector3.one *(CellSize*.5f));
                    break;
                case CellType.Wall :
                    Gizmos.color = Color.black;
                    break;
                case CellType.Button :
                    Gizmos.color = Color.green;
                    break;
                case CellType.Box :
                    Gizmos.color = Color.magenta;
                    break;
            }
            
            Vector3 position = new Vector3(cells[i].position.x, 0, cells[i].position.y);
            Gizmos.DrawWireCube(position, new Vector3(1, 0, 1) * CellSize);
            Gizmos.DrawCube(new Vector3(cells[i].position.x, 0, cells[i].position.y), Vector3.one * 0.1f);
        }


        
        
    }
}
