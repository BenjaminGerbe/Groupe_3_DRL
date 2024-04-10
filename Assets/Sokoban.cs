using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[System.Serializable]
public enum CellType
{
    Wall,
    Box,
    Player,
    Empty
}

[System.Serializable]
public struct Box
{
    public Vector2Int position;
    public Box(Vector2Int position)
    {
        this.position = position;
    }
}

public class GameState
{
    public List<Box> boxes;
    public Vector2Int player;
    public GameState(List<Box> boxes,Vector2Int playerPosition)
    {
        player = playerPosition;
        this.boxes = boxes;
    }
}

public class MapSokoban : DyProg<GameState, Vector2Int>
{
    public static Vector2Int[] direction = new Vector2Int[4]{Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up};
    public Vector2Int boardSize;
    public List<Vector2Int> buttons;
    public MapSokoban(Vector2Int boardSize,List<Box> boxes,List<Vector2Int> buttons)
    {
        this.boardSize = boardSize;
        this.buttons = buttons;
        this.states = new List<(GameState, Vector2Int, float)>();
        
        for (int i = 0; i < boardSize.x; i++)
        {
            for (int j = 0; j < boardSize.y; j++)
            {
                Vector2Int position = new Vector2Int(i, j);
                List<Box> box = new List<Box>();
                GenerateState(box,position,boxes.Count);
            }    
        }
        
    }

    public bool IsContain(GameState state)
    {
        bool find = false;
        int i =-1;
        while (!find && i < this.states.Count-1)
        {
            i++; 
            if (state.player != states[i].Item1.player)
            {
                continue;
            }
            
            find = false;
            int similar = 0;
            for (int j = 0; j < states[i].Item1.boxes.Count; j++)
            {
                Box b = states[i].Item1.boxes[j];
                for (int k = 0; k < state.boxes.Count; k++)
                {
                    if (b.position == state.boxes[k].position)
                    {
                        similar++;
                    }
                }

                if (similar >= state.boxes.Count)
                {
                    find = true;
                    break;
                }
            }
        }

        return find;
    }
    
    public (GameState,Vector2Int,float) GetGameState(GameState state)
    {
        bool find = false;
        (GameState,Vector2Int,float) Rstate  = (states[0].Item1,Vector2Int.zero,-1);
        int i =-1;
        while (!find && i < this.states.Count-1)
        {
            i++; 
            if (state.player != states[i].Item1.player)
            {
                continue;
            }
            
            find = false;
            int similar = 0;
            for (int j = 0; j < states[i].Item1.boxes.Count; j++)
            {
                Box b = states[i].Item1.boxes[j];
                for (int k = 0; k < state.boxes.Count; k++)
                {
                    if (b.position == state.boxes[k].position)
                    {
                        similar++;
                    }
                }

                if (similar >= state.boxes.Count)
                {
                    find = true;
                    Rstate = states[i];
                    break;
                }
            }
        }

      
        return Rstate;
    }

    public void GenerateState(List<Box> box,Vector2Int PlayerPosition, int k)
    {
      
        for (int x = 0; x < boardSize.x; x++) {
            for (int y = 0; y < boardSize.y; y++) {
                Box b = new Box();
                b.position = new Vector2Int(x, y);
                if(b.position == PlayerPosition)
                    continue;
                
                bool finish = false;
                for (int i = 0; i < box.Count; i++) {
                    if (box[i].position == b.position){
                        finish = true;
                        break;
                    }
                }
                if (finish)
                    continue;
                
                box.Add(b);
                if (k > 1) {
                    GenerateState(box,PlayerPosition,k-1);
                }
                else {
                    
                    GameState state = new GameState(new List<Box>(box), PlayerPosition);

                    if (IsContain(state))
                    {
                        box.Remove(b);
                        continue;
                    }
                        
                    
                    List<Vector2Int> moves = GetAllPossibleMoves(state);
                    Vector2Int move = Vector2Int.zero;
                    
                    if (moves.Count > 0) {
                        move = moves[Random.Range(0, moves.Count)];
                    }
                    states.Add((state,move,0));
                }
                box.Remove(b);
            }
        }

        return;
    }


    public override float Reward(GameState state)
    {
        if (GetAllPossibleMoves(state).Count <= 0)
        {
            return -1;
        }
        
        int reward = 0;
        for (int j = 0; j < state.boxes.Count; j++)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i] == state.boxes[j].position)
                {
                    reward++;
                }
           
            }
        }

        return reward;
    }

    public override (GameState, float) Simulate(GameState state, Vector2Int action)
    {
        GameState tmp = new GameState( new List<Box>(state.boxes),state.player);
        tmp.player += action;
        for (int i = 0; i < tmp.boxes.Count; i++)
        {
            if (tmp.player == tmp.boxes[i].position)
            {
                Box b = tmp.boxes[i];
                b.position += action;
                tmp.boxes[i] = b;
            }
        }

        (GameState, Vector2Int, float) tmpState = GetGameState(tmp);

        return (tmpState.Item1, tmpState.Item3);
    }
    
    public List<Vector2Int> GetAllPossibleMoves(GameState state,Vector2Int _position)
    {
        Vector2Int position = _position;
        List<Vector2Int> possiblesMoves = new List<Vector2Int>();
        List<Vector2Int> positionMoves = new List<Vector2Int>();
        for (int i = 0; i < direction.Length; i++)
        {
            Vector2Int move = direction[i];
            if(
                (position+move).x < 0 || (position+move).x > boardSize.x-1 ||
                (position+move).y < 0 || (position+move).y > boardSize.y-1 ){
                continue;
            }
            
            possiblesMoves.Add(move);
        }

        return possiblesMoves;
    }

    public bool Contain(GameState state, Vector2Int position)
    {
        for (int i = 0; i < state.boxes.Count ; i++)
        {
            if (state.boxes[i].position == position)
            {
                return true;
            }
        }

        return false;
    }
    
    public override List<Vector2Int> GetAllPossibleMoves(GameState state)
    {
        Vector2Int player = state.player;
        List<Vector2Int> possiblesMoves = GetAllPossibleMoves(state,player);
        List<Vector2Int> positionMoves = new List<Vector2Int>();
        
        for (int i = possiblesMoves.Count-1; i >= 0 ; i--)
        {
            bool find = false;
            for (int j = 0; j < state.boxes.Count; j++)
            {
                if (state.boxes[j].position == player + possiblesMoves[i] && !Contain(state,player+possiblesMoves[i]*2))
                {
                    if (GetAllPossibleMoves(state, state.boxes[j].position).Count <= 0)
                    {
                        possiblesMoves.RemoveAt(i);
                    }
                }
            }
        }

        return possiblesMoves;
    }
    
    public override bool isEgal(Vector2Int a, Vector2Int b)
    {
        return a == b;
    }
}

public class Sokoban : MonoBehaviour
{
    public static Vector2Int[] random_direction = new Vector2Int[4] { Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up };

    public Vector2Int BoxSize;
    public float CellSize;
    public float gamma = .9f;

    [Header("Create Map")] 
    public List<Box> box;
    public List<Vector2Int> buttons;
    public int Index;
    MapSokoban map;
    [HideInInspector]
    public string target;

    private GameState current;

    public void Init()
    {
        DateTime before = DateTime.Now;
        map = new MapSokoban(BoxSize,box,buttons);
        DateTime after = DateTime.Now; 
        TimeSpan duration = after.Subtract(before);
        Debug.Log(duration.Minutes + " :"+duration.Seconds + ": " + duration.Milliseconds);
        current = map.GetStates()[Index].Item1;
    }

    void DrawGameState(GameState state, Vector2Int offset)
    {
        this.target = state.ToString();
 
        if (box == null)
            return;
        
        for (int x = 0; x < BoxSize.x; x++)
        {
            for (int y = 0; y < BoxSize.y; y++)
            {
                Gizmos.color = Color.white;
                
                if (buttons.Contains(new Vector2Int(x,y)))
                {
                    Gizmos.color = Color.green;
                }
                
                for (int j = 0; j < box.Count; j++)
                {
                    if (state.boxes[j].position == new Vector2Int(x, y))
                    {
                        Gizmos.color = Color.magenta;
                        break;
                    }
                }
                if (new Vector2Int(x, y) == state.player)
                {
                    Gizmos.color = Color.blue;
                }
                Gizmos.DrawWireCube(new Vector3( offset.x + x*CellSize,0, offset.y  + y*CellSize), new Vector3(1, 0, 1) * CellSize*.9f);
            }
        }
    }

    public void Improvement()
    {
        DateTime before = DateTime.Now;
        map.ValueIteration();
        DateTime after = DateTime.Now; 
        TimeSpan duration = after.Subtract(before);
        Debug.Log(duration.Minutes + " :"+duration.Seconds + ": " + duration.Milliseconds);
        StartCoroutine(IA_Play());

    }
    
    void OnDrawGizmos()
    {
        if (map!= null){
            DrawGameState( current,new Vector2Int(0,0));

        }
        
    }

    IEnumerator IA_Play()
    {

        while (true)
        {
            List<Vector2Int> move = map.GetAllPossibleMoves(current);
            float Max = float.MinValue;
            int idx = 0;
            for (int i = 0; i < move.Count; i++)
            {
                var tmp = map.Simulate(current, move[i]);
                if (tmp.Item2 > Max)
                {
                    Max = tmp.Item2;
                    idx = i;
                }
            }

            current = map.Simulate(current, move[idx]).Item1;

            yield return new WaitForSeconds(1);
        }

    }
    
    private void Update()
    {
        if (map == null)
            return;
        
    }
}
