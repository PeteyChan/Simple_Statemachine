# Simple Statemachine
Simple statemachine to use in games. To use just drop the Statemachine.cs C# file into your project.

## Basic Usage
```C#
using Godot;
public partial class Example_Enums_Usage : Node
{
   enum States
   {
      // enums must have a state set to zero to represent a null state.
      // the initial state will be the null state and cannot be transitioned to.
      // the null state is the perfect state for initializiation or setup
      None = 0,
      Idle,
      Run
   }

   Statemachine<States> state = new();

   public override void _Process(double delta)
   {
      switch (state.current)
      {
         case States.None:
            // TryGoTo returns true if the state provided
            // is set to the next state.
            // Can only set next state if the next state
            // is not already set.
            state.TryGoTo(States.Idle);
            break;

         case States.Idle:
            // entered is true if this is the state's first
            // update after transitioning
            if (state.entered)
               GD.Print("Entered Idle");

            if (Input.IsKeyPressed(Key.W))
               state.TryGoTo(States.Run);

            // exiting is true when there a state that will
            // be transitiong to next state update
            if (state.exiting)
               GD.Print("Exiting Idle");
            break;

         case States.Run:
            // OnTimeElapased is called on the update that
            // the state's current time passed the target time
            if (state.OnTimeElapsed(.5f))
               state.TryGoTo(States.Idle);
            break;
      }

      // updates the internal state of the statemachine
      // as well as applying any transitions.
      // It's important to call this directly after logic
      // otherwise state.exiting may be missed
      state.Update((float)delta);
   }
}

public partial class Example_Interface_Usage : Node
{
   public interface IState
   {
      void OnEnter() { }
      void OnUpdate(Statemachine<IState> state) { }
      void OnExit() { }
   }

   /// <summary>
   /// When creating a statemachine using objects you can
   /// choose whether to store and reuse the states created
   /// with TryGet<T>() or just make a new state each time.
   /// By default the statemachine reuses states.   
   /// </summary>
   Statemachine<IState> state = new(cache_new_states: true);
   public override void _Process(double delta)
   {
      // using c# pattern matching you can handle states directly
      // similar to the enum example above.

      // this is a simple example of how you can do a more oop apporach
      switch (state.current)
      {
         // you can put any initializing code here
         case null:
            state.TryGoTo<Idle_State>();
            break;

         case IState current:
            if (state.entered)
               current.OnEnter();

            current.OnUpdate(state);

            if (state.exiting)
               current.OnExit();
            break;
      }
      // it's important to update the statemachine directly
      // after the logic so that state.exiting is not missed
      state.Update((float)delta);
   }

   class Idle_State : IState
   {
      void IState.OnEnter()
      {
         GD.Print("Entered Idle State");
      }

      void IState.OnUpdate(Statemachine<IState> state)
      {
         if (Input.IsKeyPressed(Key.W))
         {
            // if you don't supply the state to transition to
            // the statemachine will make a new state or return 
            // a cached state depending on cache_new_states's value
            // at statemachine creation
            state.TryGoTo<Run_State>();
         }
      }
   }

   class Run_State : IState
   {
      void IState.OnEnter()
      {
         GD.Print("Entered Run State");
      }

      void IState.OnUpdate(Statemachine<Example_Interface_Usage.IState> state)
      {
         if (!Input.IsKeyPressed(Key.W))
         {
            // you can use previous to transition to the previous state
            state.TryGoTo(state.previous);
         }
      }
   }
}
```
