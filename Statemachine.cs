using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class Statemachine<State>
{
   public Statemachine(bool cache_new_states = true) { if (cache_new_states) cached_state_data = new(); }
   public State previous { get; private set; }
   public int previous_updates { get; private set; }
   public float previous_time { get; private set; }
   public State current { get; private set; }
   public float current_time { get; private set; }
   public int current_updates { get; private set; }
   public float delta_time { get; private set; }
   public State next { get; private set; }
   public bool entered => current_updates == 0;
   public bool exiting => next != null;
   public float total_time { get; private set; }
   public int total_updates { get; private set; }
   public int total_transitions { get; private set; }
   Dictionary<int, object> cached_state_data;
   static int type_id;
   class Cache<T> where T : new() { public T data = new(); public static readonly int ID = type_id++; }

   /// <summary>
   /// updates the internal state of the state machine.
   /// should be called directly after state logic
   /// </summary>
   public void Update(float delta_time)
   {
      this.delta_time = delta_time;
      total_time += delta_time;
      total_updates++;
      if (EqualityComparer<State>.Default.Equals(next, default))
      {
         current_updates++;
         current_time += delta_time;
      }
      else
      {
         total_transitions++;
         previous_time = current_time;
         previous_updates = current_updates;
         previous = current;
         current = next;
         next = default;
         current_time = default;
         current_updates = default;
      }
   }

   /// <summary>
   /// Returns true only if the next state was successfully set.
   /// If cache_new_states was true on creation, will always return internally stored state,
   /// otherwise will always output a new state.
   /// </summary>
   /// <param name="state">the state we are transitioning to</param>
   public bool TryGoTo<T>(out T state) where T : State, new()
   {
      state = default;
      if (!EqualityComparer<State>.Default.Equals(next, default)) return false;
      if (cached_state_data == null) state = new();
      else
      {
         if (!cached_state_data.TryGetValue(Cache<T>.ID, out var cached))
            cached = new Cache<T>();
         state = ((Cache<T>)cached).data;
      }
      next = state;
      return true;
   }

   /// <summary>
   /// Returns true only if the next state was successfully set.
   /// If cache_new_states was true on creation, will transition to internally stored state,
   /// otherwise will always transition to a new state.
   /// </summary>
   public bool TryGoTo<T>() where T : State, new() => TryGoTo(out T _);

   /// <summary>
   /// Tries to transition to the provided state.
   /// Returns true if successful.
   /// </summary>
   public bool TryGoTo<T>(T state) where T : State
   {
      if (!EqualityComparer<State>.Default.Equals(next, default)) return false;
      if (EqualityComparer<State>.Default.Equals(state, default)) return false;
      next = state;
      return true;
   }

   static MethodInfo method =
       typeof(Statemachine<State>).GetMethods().First(method =>
       {
          return method.Name == nameof(TryGoTo)
            && method.GetGenericArguments().Length == 1
            && method.GetParameters().Length == 0;
       });

   /// <summary>
   /// Tries to transition to the state of type provided.
   /// </summary>
   public bool TryGoTo(Type type)
   {
      if (next != null) return false;
      try
      {
         return (bool)method.MakeGenericMethod(type).Invoke(this, Array.Empty<object>());
      }
      catch// (Exception e)
      {
         //Debug.LogError(type, "is not a valid state type", e.Message);
         return false;
      }
   }

   /// <summary>
   /// returns true only on the update that the current state time surpassed target time
   /// </summary>
   public bool OnTimeElapsed(float time) => current_time >= time && (current_time - delta_time) < time;
}

public class Statemachine : Statemachine<object>
{
   public Statemachine(bool cache_states = true) : base(cache_states) { }
}
