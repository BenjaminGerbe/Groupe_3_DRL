using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class MCTSnode<T,T2>
{
    public MCTSnode<T,T2> parent;
    public List<MCTSnode<T,T2>> childrens;
    public T GameState;
    public T2 action;
    public float numberWins;
    public int numberSimulations;
    public bool FullyExpanded;

    public MCTSnode()
    {
        this.childrens = new List<MCTSnode<T,T2>>();
        
    }
    
}

public abstract class GameManagerMCTS<T, T2>
{
    public abstract List<T2> GetAllPossibleMove(T state);
    public abstract bool IsFinish(T state);
    public abstract float GetReward(T state);
    public abstract bool IsEgal(T2 a,T2 b);

    public abstract T Play(T state, T2 action);
    public abstract T DefaultState();
    public abstract T Copy(T state);

    public abstract T2[] GetAllMove();
}

public enum MCTSType
{
    Random,
    Follow    
}

public class MCTSManager <T,T2>
{
    private const int NUMBER_TEST = 200; // 2000
    private const int NUMBER_SIMULATION = 50;

    private MCTSnode<T,T2>[] lstNodes; // ma liste de node
    private GameManagerMCTS<T,T2> GameManager;
    private int currentidx; // pour savoir le max de mon lstnode
    public MCTSManager(GameManagerMCTS<T,T2> state)
    {
        lstNodes = new MCTSnode<T,T2>[NUMBER_TEST + 500] ;
        
        GameManager = state;
        
        for (int i = 0; i < lstNodes.Length; i++)
        {
            lstNodes[i] = new MCTSnode<T,T2>();
        }
    }
    
    // clear les nodes que j'ai utilisé dans le pulling

    public void ClearNode()
    {
        for (int i = 0; i < lstNodes.Length; i++)
        {
            lstNodes[i].GameState = GameManager.DefaultState();
            lstNodes[i].childrens = new List<MCTSnode<T,T2>>();
            lstNodes[i].FullyExpanded = false;
            lstNodes[i].parent = null;
            lstNodes[i].numberWins = 0;
            lstNodes[i].numberSimulations = 0;
            T2[] val = GameManager.GetAllMove();
            lstNodes[i].action = val[Random.Range(0, val.Length)];
        }
    }
    
    

    public T2 ComputeMCTS(T gameState)
    {
        
        ClearNode();
        Assert.AreNotEqual(lstNodes[0],null);

        lstNodes[0].GameState = gameState;
        
        currentidx = 0;
        for (int i = 0; i < NUMBER_TEST; i++)
        {
            if (lstNodes[0].FullyExpanded) break;
            
            MCTSnode<T,T2> selectedNode = Selection(); // <==== la selection
            
            Assert.AreNotEqual(selectedNode.FullyExpanded,true);
            MCTSnode<T,T2> newNode = Expland(selectedNode,currentidx); // <= Expand
            
            float victoire = SimulateGame(newNode, newNode.action); // <== la el famoso simulation 
            Backpropagation(newNode,victoire); // la back propagation
            currentidx++;
        }
    
        
        // ca me permet de récupérer le noeuf avec le plus grand ratio de victoire
        float ratio = float.MinValue;
        MCTSnode<T,T2> n = null;
        for (int i = 0; i < lstNodes[0].childrens.Count; i++)
        {
            MCTSnode<T,T2> node = lstNodes[0].childrens[i];
            
            if ( (node.numberWins / (float) node.numberSimulations) >= ratio)
            {
                ratio = node.numberWins / (float) node.numberSimulations;
                n = node;
            }
        }

        Assert.IsNotNull(n);

        return n.action;
        }
    
    public MCTSnode<T,T2> Selection()
    {
        float ex=   Random.Range(0f,1f);
        MCTSnode<T,T2> result = null;


        if (ex <= 0.85 || currentidx <= 1)
        {
            int c = 0;
            
            // cherche une node qui ne soit pas Fully expanded et qui n'ai pas déjà fait tout les coup possibles
            do
            {
                c++;
                if (c >= (currentidx+1)*20)
                {
                    Debug.LogError("garde de fou");
                    break;
                    
                }
                
                Assert.AreNotEqual(c,(currentidx+1)*20);
                result = lstNodes[Random.Range(0, currentidx)];
            } while (result.FullyExpanded || result.childrens.Count >= GameManager.GetAllPossibleMove(result.GameState).Count);
        }
        else
        {
            
            // cherche le meilleur neud
            float maxRatio = float.MinValue;
            float c = 0;
            for (int i = 0; i < currentidx; i++)
            {
                MCTSnode<T,T2> Node = lstNodes[i];
                
                if (Node.numberWins / (float) Node.numberSimulations >= maxRatio && !Node.FullyExpanded && Node.childrens.Count < GameManager.GetAllPossibleMove(Node.GameState).Count )
                {
                    
                    maxRatio = Node.numberWins / (float) Node.numberSimulations;
                    result = Node;
              
                }
            }
        }
        
        
        return result;
    }


    public MCTSnode<T,T2> Expland(MCTSnode<T,T2> node,int current)
    {
        
        Assert.IsNotNull(node);
        Assert.AreNotEqual(node.FullyExpanded,true);
        T2 ActionToplay;
        
        // cherche un coup que tu n'as pas déjà joué
        int c = 0;
        do
        {
            c++;
            List<T2> actions = GameManager.GetAllPossibleMove(node.GameState);
            ActionToplay = actions[Random.Range(0,actions.Count)];
            
            if (c >= 100)
            {
                Debug.Log("");
                break;
            }
            
        } while (findSameInput(node,ActionToplay));
        
        lstNodes[current+1].parent = node;
        lstNodes[current+1].GameState = GameManager.Play(node.GameState,ActionToplay);
        lstNodes[current+1].action = ActionToplay;
        node.childrens.Add(lstNodes[current+1]);
        MCTSnode<T,T2> n = lstNodes[current + 1];

        return  lstNodes[current+1];
    }
    
    bool findSameInput(MCTSnode<T,T2> root,T2 action)
    {
        // cette fonction permet de trouver une action
        bool trouver = false;
        int c = 0;
        if (root != null)
        {
            int i = 0;
            while (!trouver && i < root.childrens.Count)
            {
                c++;
                
                if (c >= 500)
                {
                    Debug.LogError("garde fou");
                    break;
                }
                
                if ( ( GameManager.IsEgal(root.childrens[i].action,action)))
                {
                    trouver = true;
                }
                else
                {
                    i++;
                }
            }
        }
        
        return trouver;
    }

    public float SimulateGame(MCTSnode<T,T2> node,T2 action)
    {
        float numberVictoire = 0;
        string j = "";

        T gameState = GameManager.Copy(node.GameState);
        
        for (int i = 0; i < NUMBER_SIMULATION; i++)
        {
            gameState = GameManager.Copy(node.GameState);
            float c = 0;
            bool finish = false;
            int coup = 0;
            while ( !finish)
            {
                c++;
                
                if (c >= 20)
                {
                    break;
                }
                // simule le jeu
                List<T2> moves = GameManager.GetAllPossibleMove(gameState);
                gameState = GameManager.Play(gameState, moves[Random.Range(0, moves.Count)]);
                coup++;
                finish = GameManager.IsFinish(gameState);
            } 
            numberVictoire+= GameManager.GetReward(gameState)/coup;
        }

        node.numberSimulations = NUMBER_SIMULATION;
        node.numberWins = numberVictoire;
        
        return numberVictoire;
    }
    
    
    public void Backpropagation(MCTSnode<T,T2> node,float victory)
    {
        
        //back propagation
        MCTSnode<T,T2> n = node.parent;
        
        // si après le coup jouer dans l'expand, le partie est fini, alors c'est fully expanded
        if (GameManager.IsFinish(node.GameState))
        {
            node.FullyExpanded = true;
        }

    
        // on fait remonter le nombre de victoire et de simulation
        int c = 0;
        while (n != null)
        {
          
            bool trouver = false;
            int i = 0;
            int j = 0;
            
            
            
            if (n.childrens.Count >= GameManager.GetAllPossibleMove(node.GameState).Count )
            {
                
                bool v = true;
                for (int k = 0; k < n.childrens.Count; k++)
                {
                    v = v && n.childrens[k].FullyExpanded;
                }

                n.FullyExpanded = v;
            }
            
            if (c >= (currentidx+1)*2)
            {
                Debug.LogError("garde fou");
                break;
            }
            
            c++;
            

            n.numberSimulations += NUMBER_SIMULATION;
            n.numberWins += victory;
            n = n.parent;
        }
       
    }
}