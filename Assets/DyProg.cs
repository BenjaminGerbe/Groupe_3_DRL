using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;


public abstract class DyProg<T,T2>
{
   
   private const float GAMMA = .9f; 
   private const float ALPHA = .2f; 
   protected List<(T,T2,float)> SarsaList;
   protected List<(T,T2,float)> states;
   public abstract float Reward(T state);
   public abstract bool IsFinish(T state);
   public abstract (T,float) Simulate(T state,T2 action);
   public abstract int SimulateInt(T state,T2 action);
   public abstract List<T2> GetAllPossibleMoves(T state);
   
   public DyProg(){}

   public List<(T,T2,float)> GetStates(){ return this.states;}

   private void Evaluation() {
      float delta=1;
      
      while(delta > 0.01f){
         delta = 0;
         for (int i = 0; i < states.Count; i++) {
            T state,nState;
            T2 action;
            float v,v1;
            (state,action,v) = states[i];
            (nState,v1) = Simulate(state, action);
            float nV  = Reward(nState) + GAMMA*v1;
            states[i] = (state, action, nV);
            delta = Mathf.Max(delta,Mathf.Abs(nV - v));
         }
      }
   }
   public  abstract  bool isEgal(T2 a,T2 b);
   
   public void ValueIteration() {
       bool PolicyStable = false;
       while (!PolicyStable)
       {
           PolicyStable = true;
           for (int i = 0; i < states.Count; i++)
           {
               (T,T2,float) state = states[i];
            
               T2 tmp = state.Item2;
               float max = state.Item3;
               float reward = Reward(state.Item1);
               T2 action = state.Item2;
               List<T2> possibleMoves = GetAllPossibleMoves(state.Item1);
              
               for (int j = 0; j < possibleMoves.Count; j++)
               {
                   T2 move = possibleMoves[j];
                   (T,float) val = Simulate(state.Item1,move);
               
                   if (val.Item2 > max) {
                       max = val.Item2;
                       action = move;
                       reward = Reward(val.Item1);
                   }
               }
               float value = reward + GAMMA * max;
               states[i] = (states[i].Item1, action, value);
               if (!isEgal(tmp,action)) {
                   PolicyStable = false;
               }
           }
       }
   }

   public T2 GetMove(T game)
   {
      List<T2> possibleMoves = GetAllPossibleMoves(game);
      T2 action = possibleMoves[Random.Range(0,possibleMoves.Count)];
      if (Random.Range(0f, 1f) < 0.5f)
      {
         float max = float.MinValue;
         for (int j = 0; j < possibleMoves.Count; j++)
         {
            T2 move = possibleMoves[j];
            (T, float) val = Simulate(game, move);

            if (val.Item2 > max)
            {
               max = val.Item2;
               action = move;
            }
         }
      }

      return action;
   }

   public List<(T, T2, float)> GetSarsaList()
   {
      return SarsaList;
   }

   public void Sarsa(T g,T2 pol)
   {
      this.SarsaList = new List<(T, T2, float)>();
      this.SarsaList.Add((g,pol,0));
      for (int i = 0; i < 100; i++)
      {
         T game = this.SarsaList[0].Item1;
         T2 action = GetMove(game);
         int c = 0;
         int current = 0;
         while (c < 100 || IsFinish(game))
         {
            c++;
            int nxt =  SimulateInt(game, action);
            T2 ac = GetMove(SarsaList[nxt].Item1);
            T gameState = SarsaList[nxt].Item1;
            float val = SarsaList[current].Item3 + ALPHA * (Reward(gameState)+GAMMA*(this.SarsaList[nxt].Item3 - this.SarsaList[current].Item3 ) );
            this.SarsaList[current] = (game, action, val);
            
            current = nxt;
            game = gameState;
            action = ac;
         }
         
      }
   }
   
   public void Improvement() {
      bool PolicyStable = false;
      while (!PolicyStable)
      {
         Evaluation();
         PolicyStable = true;
         for (int i = 0; i < states.Count; i++)
         {
            (T,T2,float) state = states[i];
            
            T2 tmp = state.Item2;
            float max = float.MinValue;
            T2 action = state.Item2;
            List<T2> possibleMoves = GetAllPossibleMoves(state.Item1);

            for (int j = 0; j < possibleMoves.Count; j++)
            {
               T2 move = possibleMoves[j];
               (T,float) val = Simulate(state.Item1,move);
               
               if (val.Item2 > max) {
                  max = val.Item2;
                  action = move;
               }
            }

            states[i] = (states[i].Item1, action, states[i].Item3);
            if (!isEgal(tmp,action)) {
               PolicyStable = false;
            }
         }
      }
   }
}



