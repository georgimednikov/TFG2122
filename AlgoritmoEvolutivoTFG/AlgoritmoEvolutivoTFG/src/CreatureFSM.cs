using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stateless;


namespace AlgoritmoEvolutivo
{
    /// <summary>
    /// IDs for the State Machine
    /// </summary>
    public enum StateID
    {
        Idle,
        Moving,
        Alive,
        Dead,
        Eat
    }
    public enum TriggerID
    {
        Moves,
        Stops,
        Dies,
        Eats
    }

    public abstract class State
    {
        protected StateID id;
        public abstract void Action();
    }

    public class Moving : State
    {
        public Moving()
        {
            id = StateID.Moving;
        }

        override public void Action()
        {
            Console.WriteLine("Moving");
        }
    }

    public class Idle : State
    {
        public Idle()
        {
            id = StateID.Idle;
        }

        override public void Action()
        {
            Console.WriteLine("Idle");
        }
    }

    public class Dead : State
    {
        public Dead()
        {
            id = StateID.Dead;
        }

        override public void Action()
        {
            Console.WriteLine("Dead");
        }
    }

    public class Alive : State
    {
        public Alive()
        {
            id = StateID.Alive;
        }

        override public void Action()
        {
            Console.WriteLine("Alive");
        }
    }

    public class Eating : State
    {
        public Eating()
        {
            id = StateID.Eat;
        }

        override public void Action()
        {
            Console.WriteLine("Eating");
        }
    }

    public abstract class Transition
    {
        /// <summary>
        /// Evaluates the transition and, if it returns true, the transition occurs
        /// </summary>
        public abstract bool Evaluate();
    }

    public class BooleanTransition : Transition
    {
        /// <summary>
        /// Constructor for the boolean transition, which evaluates true if the given bool is true
        /// </summary>
        public BooleanTransition(ref bool boolCondition)
        {
            b = boolCondition;
        }

        /// <summary>
        /// Evaluates true if the given bool is true
        /// </summary>
        /// <returns></returns>
        public override bool Evaluate()
        {
            return b;
        }

        bool b;
    }





    public interface FSM
    {
        void Execute();
        void Evaluate();
    }

    public class StatelessFSM : FSM
    {
        public StatelessFSM(State initalState)
        {
            fsm = new StateMachine<State, Transition>(initalState, FiringMode.Queued);
        }

        public void AddTransition(State og, Transition t, State dest)
        {
            fsm.Configure(og)
                .Permit(t, dest); //?
        }

        public void AddSubstate(State super, State sub)
        {
            fsm.Configure(sub)
                .SubstateOf(super);
        }

        public void Execute()
        {
            fsm.State.Action();
        }

        /// <summary>
        /// Triggers whichever transitions return true on its Evaluate, if there is an available transition
        /// </summary>
        public void Evaluate()
        {
            Transition[] currTransitions = fsm.GetPermittedTriggers().ToArray();
            
            foreach(Transition t in currTransitions)
            {
                if (fsm.CanFire(t) && t.Evaluate())
                {
                    fsm.Fire(t);
                    //break;
                }
            }
        }
        Stateless.StateMachine<State, Transition> fsm;
    }
}
