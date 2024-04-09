using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class Grid{
    public Vector2Int position;
}

public class GridWordDyProg : DyProg<Grid, Vector2Int>
{
    public static Vector2Int[] random_direction = new Vector2Int[4]{Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up};
    public Vector2Int[] HolesPosition;
    public Vector2Int boardSize;
    public Vector2Int EndPosition;
    public override List<Vector2Int> GetAllPossibleMoves(Grid state)
    {
        List<Vector2Int> possiblesMoves = new List<Vector2Int>();
        if (state.position == EndPosition || HolesPosition.Contains(state.position))
            return possiblesMoves;
        
            
        for (int i = 0; i < random_direction.Length; i++)
        {
            Vector2Int move = random_direction[i];
            if((state.position+move).x < 0 || (state.position+move).x > boardSize.x-1 
            || (state.position+move).y < 0 || (state.position+move).y > boardSize.y-1 ){
                continue;
            }
            possiblesMoves.Add(move);
        }
        return possiblesMoves;
    }

    public GridWordDyProg(Vector2Int boardSize,Vector2Int EndPosition,Vector2Int[] HolesPositions): base()
    {
        this.boardSize = boardSize;
        this.EndPosition = EndPosition;
        HolesPosition = HolesPositions;
        this.states = new List<(Grid,Vector2Int,float)>();
        for (int i = 0;i<boardSize.x;i++){
            for (int j = 0;j<boardSize.y;j++){
                Grid grid = new Grid();
                grid.position = new Vector2Int(i,j);
                List<Vector2Int> actions = GetAllPossibleMoves(grid);
                
                if(Reward(grid) == 1 || actions.Count == 0){
                    states.Add((grid,Vector2Int.zero,0));
                }else{
                    Vector2Int action = actions[Random.Range(0,actions.Count)];
                    states.Add((grid,action,0));
                }
            }   
        }
    }

    public override bool isEgal(Vector2Int a, Vector2Int b)
    {
        return a == b;
    }

    public override float Reward(Grid state)
    {
        if (HolesPosition.Contains(state.position))
        {
            return -1;
        }
        
        if(state.position == EndPosition){
            return 1f;
        }
        return 0f;
    }

    public override (Grid, float) Simulate(Grid state, Vector2Int action)
    {
        for (int i = 0; i < this.states.Count ; i++)
        {
            if(states[i].Item1.position == state.position + action){
                return (states[i].Item1,states[i].Item3);
            }
        }

        Debug.Assert(false,"the state has not been find in the list of states");
        return (null,-1);
    }
}

public class GridWorld : MonoBehaviour
{
 
    public Vector2Int EndPosition;
    public Vector2Int boardSize;
    public float BoxSize;
    GridWordDyProg  DyProg;
    public GameObject arrow;
    public Vector2Int[] HolesPosition;
    private List<Transform> Arrows;

    void OnDrawGizmos()
    {
        if(DyProg == null)
            return;
        // Draw a yellow sphere at the transform's position
        List<(Grid,Vector2Int,float)> states = DyProg.GetStates();
        if (states.Count != Arrows.Count)
            return;
        for (int i = 0; i < DyProg.GetStates().Count; i++)
        {
            Gizmos.color = Color.white;
            Vector3 position = new Vector3(states[i].Item1.position.x,0,states[i].Item1.position.y);
            Gizmos.DrawWireCube(position,new Vector3(1,0,1) * BoxSize);
        
            if (states[i].Item2 == Vector2Int.down)
            {
                Arrows[i].GetComponentInChildren<SpriteRenderer>().color = Color.blue;
                Arrows[i].rotation = Quaternion.LookRotation(new Vector3(1, 0, 0), Vector3.up);
            }
            
            if (states[i].Item2 == Vector2Int.up)
            {
                Arrows[i].GetComponentInChildren<SpriteRenderer>().color = Color.yellow;
                Arrows[i].rotation = Quaternion.LookRotation(new Vector3(-1, 0, 0), Vector3.up);
            }

            if (states[i].Item2 == Vector2Int.right)
            {
                Arrows[i].GetComponentInChildren<SpriteRenderer>().color = Color.red;
                Arrows[i].rotation = Quaternion.LookRotation(new Vector3(0, 0, 1), Vector3.up);
            }
            
            if (states[i].Item2 == Vector2Int.left)
            {
                Gizmos.color = Color.green;
                Arrows[i].rotation = Quaternion.LookRotation(new Vector3(0, 0, -1), Vector3.up);
                Arrows[i].GetComponentInChildren<SpriteRenderer>().color = Color.green;
            }
            
            if (states[i].Item1.position == EndPosition)
            {
                Gizmos.color = Color.magenta;
                Arrows[i].GetComponentInChildren<SpriteRenderer>().enabled=false;
                Gizmos.DrawCube(new Vector3(states[i].Item1.position.x,0,states[i].Item1.position.y), Vector3.one*(BoxSize-0.01f));
            }

            if (HolesPosition.Contains(states[i].Item1.position))
            {
                Arrows[i].GetComponentInChildren<SpriteRenderer>().enabled=false;
                
            }
           
        }

    }
    
     

    public void Init(){

       
        DyProg = new GridWordDyProg(boardSize,EndPosition,HolesPosition);
        if (Arrows != null)
        {
            for (int i = 0; i < Arrows.Count; i++)
            {
                DestroyImmediate(Arrows[i].gameObject);
            }
        }

        Arrows = new List<Transform>();
        for (int i = 0; i < DyProg.GetStates().Count; i++)
        {
            Vector2Int position = DyProg.GetStates()[i].Item1.position;
            GameObject go = Instantiate(arrow, this.transform);
           go.transform.position = new Vector3(position.x, 0, position.y);
           Arrows.Add(go.transform);
        }
        
    }

    public void ClearArrow()
    {
        foreach (Transform child in this.transform) {
            GameObject.DestroyImmediate(child.gameObject);
        }

        Arrows = new List<Transform>();
        }

    public void Improvement()
    {
       DyProg.Improvement();
    }

    public void ValueIteration(){
        DyProg.ValueIteration();
    }

}
