using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class DyProg<T,T2>
{
   private const float GAMMA = .9f; 
   protected List<(T,T2,float)> states;
   public abstract float Reward(T state);
   public abstract (T,float) Simulate(T state,T2 action);
   public abstract List<T2> GetAllPossibleMoves(T state);
   
   public DyProg(){}

   public List<(T,T2,float)> GetStates(){ return this.states;}

   private void Evaluation() {
      float delta=1;
      
      while(delta > float.Epsilon){
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
   
   public void Improvement() {
      bool PolicyStable = false;
      int gdf = 0;
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

