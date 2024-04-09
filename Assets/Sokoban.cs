using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sokoban : MonoBehaviour
{
    public static Vector2Int[] random_direction = new Vector2Int[4] { Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up };

    public float CellSize;
    public float gamma = .9f;

    public List<Vector2Int> cells;
    public List<Vector2Int> wallsPosition;
    public List<Vector2Int> startBoxsPositions;
    public List<Vector2Int> butonsPositions;

    public Vector2Int startPositionPlayer;

    public class MapSokoban
    {
        public List<Cell> board;
        public List<Mur> wallsPosition;
        public List<Box> startBoxsPositions;
        public List<Buton> butonsPositions;
        public Vector2Int player_position;

        public MapSokoban(List<Cell> b, List<Mur> w, List<Box> boxs, List<Buton> btn, Vector2Int player_position)
        {
            this.board = b;
            this.wallsPosition = w;
            this.startBoxsPositions = boxs;
            this.butonsPositions = btn;
            this.player_position = player_position;
        }
    }

    public class Cell
    {
        public Vector2Int position;
        public float value;
        public Cell(Vector2Int position, float value)
        {
            this.position = position;
            this.value = value;
        }
    }

    public class Mur
    {
        public Vector2Int position;

        public Mur (Vector2Int position)
        {
            this.position = position;
        }
    }

    public class Box
    {
        public Vector2Int position;
        public bool placed;

        public Box(Vector2Int position)
        {
            this.position = position;
            this.placed = false;
        }
        
    }

    public class Buton
    {
        public Vector2Int position;

        public Buton(Vector2Int position)
        {
            this.position = position;
        }
    }

    MapSokoban map;

    public void Init()
    {
        Debug.ClearDeveloperConsole();

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
        
    }

    void OnDrawGizmos()
    {
        if (map == null)
        {
            return;
        }

        for (int i = 0; i < map.board.Count; i++)
        {
            Gizmos.color = Color.white;
            Vector3 position = new Vector3(map.board[i].position.x, 0, map.board[i].position.y);
            Gizmos.DrawWireCube(position, new Vector3(1, 0, 1) * CellSize);
            Gizmos.DrawCube(new Vector3(map.board[i].position.x, 0, map.board[i].position.y), Vector3.one * 0.1f);
        }

        for (int j = 0; j < map.wallsPosition.Count; j++)
        {
            Gizmos.color = Color.black;
            Vector3 position = new Vector3(map.wallsPosition[j].position.x, 0, map.wallsPosition[j].position.y);
            Gizmos.DrawCube(position, new Vector3(1, 0, 1) * CellSize);
            //Gizmos.DrawCube(new Vector3(map.wallsPosition[j].position.x, 0, map.wallsPosition[j].position.y), Vector3.one * 0.1f);
        }

        for (int k = 0; k < map.startBoxsPositions.Count; k++)
        {
            Gizmos.color = Color.magenta;
            Vector3 position = new Vector3(map.startBoxsPositions[k].position.x, 0, map.startBoxsPositions[k].position.y);
            Gizmos.DrawWireCube(position, new Vector3(1, 0, 1) * CellSize);
            Gizmos.DrawCube(new Vector3(map.startBoxsPositions[k].position.x, 0, map.startBoxsPositions[k].position.y), Vector3.one * 0.1f);
        }

        for (int l = 0; l < map.butonsPositions.Count; l++)
        {
            Gizmos.color = Color.green;
            Vector3 position = new Vector3(map.butonsPositions[l].position.x, 0, map.butonsPositions[l].position.y);
            Gizmos.DrawWireCube(position, new Vector3(1, 0, 1) * CellSize);
            Gizmos.DrawCube(new Vector3(map.butonsPositions[l].position.x, 0, map.butonsPositions[l].position.y), Vector3.one * 0.1f);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawCube(new Vector3(map.player_position.x, 0, map.player_position.y), Vector3.one * (CellSize / 2));
    }
}
