using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class MCTSnode
{
    public MCTSnode parent;
    public List<MCTSnode> childrens;
    public GameState GameState;
    public Action action;
    public int numberWins;
    public int numberSimulations;
    public bool FullyExpanded;

    public MCTSnode()
    {
        this.childrens = new List<MCTSnode>();
    }
    
}

public class MCTSManager<T,T2> : DyProg<T,T2>
{
    private const int NUMBER_TEST = 1500; // 2000
    private const int NUMBER_SIMULATION = 30;

    private MCTSnode[] lstNodes; // ma liste de node
    private T GameManager;
    private int currentidx; // pour savoir le max de mon lstnode
    public MCTSManager(T state)
    {
        lstNodes = new MCTSnode[NUMBER_TEST + 500] ;
        
        GameManager = state; // je récupère le gameManager
        
        for (int i = 0; i < lstNodes.Length; i++)
        {
            lstNodes[i] = new MCTSnode();
        }
        
    }
    
    // clear les nodes que j'ai utilisé dans le pulling

    public void ClearNode()
    {
        
        for (int i = 0; i < lstNodes.Length; i++)
        {
            lstNodes[i].GameState = null;
            lstNodes[i].childrens = new List<MCTSnode>();
            lstNodes[i].FullyExpanded = false;
            lstNodes[i].parent = null;
            lstNodes[i].numberWins = 0;
            lstNodes[i].numberSimulations = 0;
       
        }
    }
    

    
    public Action ComputeMCTS(GameState gameState)
    {
        
        ClearNode();
        GameManager.initializeGame(gameState,true);
        gameState.simulation = true;

        Assert.AreNotEqual(lstNodes[0],null);

        lstNodes[0].GameState = gameState;
        
        currentidx = 0;
        for (int i = 0; i < NUMBER_TEST; i++)
        {
            if (lstNodes[0].FullyExpanded) break;
            
            MCTSnode selectedNode = Selection(); // <==== la selection
            
            Assert.AreNotEqual(selectedNode.FullyExpanded,true);
            MCTSnode newNode = Expland(selectedNode,currentidx); // <= Expand
            
            int victoire = SimulateGame(newNode, newNode.action); // <== la el famoso simulation 
            Backpropagation(newNode,victoire); // la back propagation
            currentidx++;
        }

        
        // ca me permet de récupérer le noeuf avec le plus grand ratio de victoire
        float ratio = float.MinValue;
        MCTSnode n = null;
        for (int i = 0; i < lstNodes[0].childrens.Count; i++)
        {
            MCTSnode node = lstNodes[0].childrens[i];
            
            if ( (node.numberWins / (float) node.numberSimulations) >= ratio)
            {
                ratio = node.numberWins / (float) node.numberSimulations;
                n = node;
            }
        }

        Assert.IsNotNull(n);

        return n.action;
        }
    
    public MCTSnode Selection()
    {
        float ex=   Random.Range(0f,1f);
        MCTSnode result = null;


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
            } while (result.FullyExpanded || result.childrens.Count >= GameManager.actions.Count);

           
        }
        else
        {
            
            // cherche le meilleur neud
            float maxRatio = float.MinValue;
            float c = 0;
            for (int i = 0; i < currentidx; i++)
            {
                MCTSnode Node = lstNodes[i];
                
                if (Node.numberWins / (float) Node.numberSimulations >= maxRatio && !Node.FullyExpanded && Node.childrens.Count < GameManager.actions.Count )
                {
                    
                    maxRatio = Node.numberWins / (float) Node.numberSimulations;
                    result = Node;
              
                }
            }
        }
        
        
        return result;
    }

    public MCTSnode Expland(MCTSnode node,int current)
    {
        
        Assert.IsNotNull(node);
        Assert.AreNotEqual(node.FullyExpanded,true);
        Action ActionToplay;
        
        // cherche un coup que tu n'as pas déjà joué
        int c = 0;
        do
        {
            c++;

            ActionToplay = GameManager.actions[Random.Range(0,GameManager.actions.Count)];
            
            if (c >= 10000)
            {
                Debug.Log("");
                Debug.LogError("c'est ca");
               // Debug.LogError("garde fou");
                break;
            }
            
        } while (findSameInput(node,ActionToplay));
        
        
        lstNodes[current+1].parent = node;
        lstNodes[current+1].GameState = InterfaceGameState.instance.CreateInstance(node.GameState);
        lstNodes[current+1].action = ActionToplay;
        node.childrens.Add(lstNodes[current+1]);
        MCTSnode n = lstNodes[current + 1];
        
        
        SimulateAction(n,ActionToplay); // Pour jouer un coup 
        

        return  lstNodes[current+1];
    }
    
    bool findSameInput(MCTSnode root,Action action)
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
                
                if ( (root.childrens[i].action.direction == action.direction) && (Math.Abs(root.childrens[i].action.time - action.time) < float.Epsilon) )
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
    
    public void SimulateAction(MCTSnode node,Action action)
    {
     
        
        GameState gameState = node.GameState;
        
        
        // ca me permet de trouver quel agent je dois bouger et de quel façon
        Joueur jmcts = node.GameState.j1.PlayerTag == this.joueur.PlayerTag
            ? node.GameState.j1
            : node.GameState.j2;
        
        Joueur Jplayer = node.GameState.j1.PlayerTag != this.joueur.PlayerTag
            ? node.GameState.j1
            : node.GameState.j2;
        
        GameManager.SetAction(jmcts, action); 
        GameManager.SetRandomAction(Jplayer,GameManager.actions);
        int c = 0;
        
        Assert.AreEqual(gameState.simulation,true);

        // tant que le joueur agent n'a pas fini de bouger, on déroule la simulation
        while (jmcts.counter > 0)
        {
            c++;
            
            if (c >= 500)
            {
                Debug.LogError("garde fou");
                break;
            }
          
        
            GameManager.RunFrame(gameState);
            GameManager.EmulateOneMove(gameState);
        }
        
    }
 
    
    public int SimulateGame(MCTSnode node,Action action)
    {
        int numberVictoire = 0;
        string j = "";

        GameState gameState = InterfaceGameState.instance.CreateInstance(node.GameState);
        
        Assert.AreNotEqual(this.joueur,null);
        
        for (int i = 0; i < NUMBER_SIMULATION; i++)
        {
            
            float c = 0;
            bool finish = false;
            

            while ( !finish)
            {
                c++;
                
                if (c >= 1000)
                {
                    Debug.LogError("garde fou");
                    break;
                }
                // simule le jeu
                GameManager.RunFrame(gameState);
                GameManager.RunSimulatedMovement(gameState);
                 
                (finish,j) = InterfaceGameState.instance.getGameManager().Goal(gameState);
                
                
               // Assert.AreNotEqual(c,15000);
                
            }

            if (j == this.joueur.PlayerTag)
            {
                numberVictoire++;
            }
            else if(j != "Timer")
            {
                numberVictoire--;
            }
            
        
        }

        node.numberSimulations = NUMBER_SIMULATION;
        node.numberWins = numberVictoire;
        
        return numberVictoire;
    }
    
    
    public void Backpropagation(MCTSnode node,int victory)
    {
        
        //back propagation
        
        MCTSnode n = node.parent;
        
        
        // si après le coup jouer dans l'expand, le partie est fini, alors c'est fully expanded
        if (GameManager.Goal(node.GameState).Item1)
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
            
            
            
            if (n.childrens.Count >= GameManager.actions.Count )
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


    public override float Reward(T state)
    {
        throw new NotImplementedException();
    }

    public override (T, float) Simulate(T state, T2 action)
    {
        throw new NotImplementedException();
    }

    public override List<T2> GetAllPossibleMoves(T state)
    {
        throw new NotImplementedException();
    }

    public override bool isEgal(T2 a, T2 b)
    {
        throw new NotImplementedException();
    }
}