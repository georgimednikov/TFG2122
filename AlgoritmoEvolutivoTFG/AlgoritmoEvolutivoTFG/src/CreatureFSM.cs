using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stateless;


namespace AlgoritmoEvolutivo
{
    /// <summary>
    /// IDs para la maquina de estados
    /// </summary>
    public enum StateID
    {
        Idle,
        Moving,
        Alive,
        Dead
    }
    public enum TriggerID
    {
        Moves,
        Stops,
        Dies
    }

    public abstract class _State
    {
        protected StateID id;
        public abstract void Action();
    }

    public class Moving : _State
    {
        public Moving ()
        {
            id = StateID.Moving;
        }

        override public void Action()
        {
            Console.WriteLine("Moving");
        }
    }

    public abstract class Transition
    {
        protected TriggerID id;
        public abstract bool Evaluate();
    }


    public interface FSM
    {
        void Execute();
        void Evaluate();
    }

    public class StatelessFSM : FSM
    {
        public StatelessFSM(_State initalState)
        {
            fsm = new StateMachine<_State, Transition>(initalState);
        }

        public void AddTransition(_State og, Transition t, _State dest)
        {
            fsm.Configure(og)
                .Permit(t, dest); //?
        }

        public void Execute()
        {
            fsm.State.Action();
        }
        public void Evaluate()
        {
            Transition[] currTransitions = fsm.GetPermittedTriggers().ToArray();

            foreach(Transition t in currTransitions)
            {
                if (t.Evaluate())
                {
                    fsm.Fire(t);
                    break;
                }
            }
        }
        Stateless.StateMachine<_State, Transition> fsm;
    }
}
