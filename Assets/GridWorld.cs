using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor.Experimental.GraphView;
using UnityEngine;



public class GridWorld : MonoBehaviour
{
    public static Vector2Int[] random_direction = new Vector2Int[4]{Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up};
    public Vector2 EndPosition;
    public Vector2 boardSize;
    public float BoxSize;
    public float gamma = .9f;
    public class Grid{
        public Vector2Int position;
        public float value;
        public Vector2Int policy;
        public Vector2 boardSize;

        public int IsPossibleMove(Vector2Int move){
            if((this.position+move).x < 0 || (this.position+move).x > boardSize.x-1 
            || (this.position+move).y < 0 || (this.position+move).y > boardSize.y-1 ){
                return -1;
            }

            return 1;
        }

        public  Grid(Vector2Int position,float value,Vector2 boardSize){
            this.boardSize = boardSize;
           
            this.position = position;
            this.value = value;
            int possibleMove = -1;
            do{
                int random = Random.Range(0,4);
                Grid grid = this;
                this.policy = random_direction[random];
                possibleMove = IsPossibleMove(this.policy);
         
            }while(possibleMove < 0);
        }
    }
    
    List<Grid> board;

    public float Reward(Grid grid){
        if(grid.position == EndPosition){
            return 1f;
        }
        return 0f;
    }

    public int GetGrid(Grid grid,Vector2Int Policy)
    {
        for (int i = 0; i < board.Count; i++)
        {
            if(board[i].position == grid.position + Policy){
                return i;
            }
        }
        return -1;
    }

    void OnDrawGizmos()
    {
        if(board == null){
            return;
        }

        // Draw a yellow sphere at the transform's position
        for (int i = 0; i < board.Count; i++)
        {
    
            Gizmos.color = Color.white;
            Vector3 position = new Vector3(board[i].position.x,0,board[i].position.y);
            Gizmos.DrawWireCube(position,new Vector3(1,0,1) * BoxSize);
        
            if (board[i].policy == Vector2Int.down)
            {
                Gizmos.color = Color.blue;
            }
            
            if (board[i].policy == Vector2Int.up)
            {
                Gizmos.color = Color.yellow;
            }

            if (board[i].policy == Vector2Int.right)
            {
                Gizmos.color = Color.red;
            }
            
            if (board[i].policy == Vector2Int.left)
            {
                Gizmos.color = Color.green;
            }
            
            if (board[i].position == EndPosition)
            {
                Gizmos.color = Color.magenta;
            }

            Gizmos.DrawCube(new Vector3(board[i].position.x,0,board[i].position.y) ,Vector3.one*0.1f);
        }

    }

    public void Init(){

        board = new List<Grid>();
        for (int i = 0;i<boardSize.x;i++){
            for (int j = 0;j<boardSize.x;j++){
                int val = 0;
                if(i == EndPosition.x && j == EndPosition.y){
                    val = 1;
                }
                Grid grid = new Grid(new Vector2Int(i,j),val,boardSize);
                board.Add(grid);
                if(Reward(grid) == 1){
                    grid.policy = new Vector2Int();
                }
            }   
        }

    }

    public void Evaluation(){

        float delta=1;
        int gdf =0;
        while(delta > float.Epsilon){
            gdf++;
            delta = 0;
            for (int i = 0; i < board.Count; i++)
            {
                Grid grid = board[i];
                int idx = GetGrid(grid,grid.policy);
                if (idx < 0)
                    continue;
                Grid nxtGrid= board[idx];
                float v = grid.value;
                grid.value = Reward(nxtGrid) + gamma*nxtGrid.value;
                delta = Mathf.Max(delta,Mathf.Abs(grid.value - v));
            }
        }
        return;
    }

    public void Improvement()
    {
        bool PolicyStable = false;
        while (!PolicyStable)
        {
            Evaluation();
            PolicyStable = true;
            for (int i = 0; i < board.Count; i++)
            {
                if (board[i].position == EndPosition)
                    continue;
                Grid grid = board[i];
                Vector2Int tmp = grid.policy;
                float max = float.MinValue;
                Vector2Int Policy = grid.policy;
                for (int j = 0; j < random_direction.Length; j++)
                {
                    if (grid.IsPossibleMove(random_direction[j]) > 0)
                    {
                        int idx = GetGrid(grid, random_direction[j]);
                        if (idx < 0)
                            continue;
                        Grid g = board[idx];
                        if (g.value > max) {
                            max = g.value;
                            Policy = random_direction[j];
                        }
                    }
                }

                grid.policy = Policy;
                if (tmp != Policy) {
                    PolicyStable = false;
                }
            }
        }
    }

    public Grid checkVMax(Grid grid)
    {
        Grid GVmax = grid;
        if (grid.position == EndPosition)
        {
            Debug.Log("caseFinal");
            return null;
        }

        if (grid.IsPossibleMove(Vector2Int.right) > 0 && board[GetGrid(grid, Vector2Int.right)].value >= GVmax.value)
        {
            GVmax = board[GetGrid(grid, Vector2Int.right)];
            grid.value = /*Reward(GVmax) +*/ gamma * GVmax.value;
            Debug.Log(grid.value);
            grid.policy = Vector2Int.right;
        }

        if (grid.IsPossibleMove(Vector2Int.left) > 0 && board[GetGrid(grid, Vector2Int.left)].value >= GVmax.value)
        {
            GVmax = board[GetGrid(grid, Vector2Int.left)];
            grid.value = /*Reward(GVmax) +*/ gamma * GVmax.value;
            Debug.Log(grid.value);
            grid.policy = Vector2Int.left;
        }

        if (grid.IsPossibleMove(Vector2Int.down) > 0 && board[GetGrid(grid, Vector2Int.down)].value >= GVmax.value)
        {
            GVmax = board[GetGrid(grid, Vector2Int.down)];
            grid.value = /*Reward(GVmax) +*/ gamma * GVmax.value;
            Debug.Log(grid.value);
            grid.policy = Vector2Int.down;
        }

        if (grid.IsPossibleMove(Vector2Int.up) > 0 && board[GetGrid(grid, Vector2Int.up)].value >= GVmax.value)
        {
            GVmax = board[GetGrid(grid, Vector2Int.up)];
            grid.value = /*Reward(GVmax) +*/ gamma * GVmax.value;
            Debug.Log(grid.value);
            grid.policy = Vector2Int.up;
        }
        return GVmax;
    }

    public void valueIteration()
    {
        bool PolicyStable = false;
        while (!PolicyStable)
        {
            PolicyStable = true;
            for (int i = 0; i < board.Count; i++)
            {
                if (board[i].position == EndPosition)
                    continue;
                Grid grid = board[i];
                Vector2Int tmp = grid.policy;
                float max = float.MinValue;
                Vector2Int Policy = grid.policy;
                Grid nxtG = checkVMax(grid);

                Policy = grid.policy;


                if (tmp != Policy)
                {
                    PolicyStable = false;
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
