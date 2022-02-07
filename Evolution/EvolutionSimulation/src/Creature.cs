using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvolutionSimulation.FSM;

namespace EvolutionSimulation
{
    /// <summary>
    /// A creature with attributes and behavior
    /// </summary>
    public class Creature : IEntity
    {
        //TODO: Al que le toque, que seguramente sea yo, que lo quite.
        public int metabolism { get; set; }
        public int mobility { get; private set; }

        /// <summary>
        /// Private class representing states.
        /// Each state has an ID and an associated action
        /// </summary>
        private class State
        {
            public StateID name;
            public Action action;

            public State(StateID n, Action a)
            {
                name = n; action = a;
            }
        }

        /// <summary>
        /// Constructor for factories
        /// </summary>
        public Creature()
        {
            r = new Random();
            metabolism = r.Next(1, 201);
            mobility = 100;
        }

        /// <summary>
        /// Initializes a creature in a world and position
        /// </summary>
        /// <param name="w">World in which it'll reside</param>
        public void Init(World w, int x, int y)
        {
            world = w;
            this.x = x;
            this.y = y;
            this.fsm = new Stateless.StateMachine<State, TriggerID>(
                () => currState,
                s => currState = s,
                Stateless.FiringMode.Queued
            );
            ConfigureStateMachine();
        }

        /// <summary>
        /// Simulation step
        /// </summary>
        public void Tick()
        {
            actionPoints += metabolism * 10;

            // TODO: Put Fire in the states' actions
            // TODO: and yeet this outta here
            //if (currState.name == StateID.Dead) return;
            //Speed--;
            //if (Speed <= 0)
            //{
            //    Speed = 0;
            //    fsm.Fire(TriggerID.Dies);
            //}
            //else if (Speed % 2 == 0)
            //{
            //    fsm.Fire(TriggerID.Moves);
            //}
            //else fsm.Fire(TriggerID.Stops);

            // Calculates next action according to the state machine
            //currState.action();
            // evaluateTransitions();
            // Checks if there are enough action points to perform said action
            // If not, "skips" its turn
            // If yes, performs said action and attempts to use the FSM again
            while(Move());
        }

        /// <summary>
        /// Attempts to move the creature to an adjacent tile
        /// </summary>
        bool Move()
        {
            int nX = 0, nY = 0;
            do
            {
                nX = x + r.Next(-1, 2);
                nY = y + r.Next(-1, 2);

            } while (nX != x && nY != y);
            if (world.canMove(nX, nY))
            {
                if (actionPoints < 1000 * ((200f - mobility) / 100f)) return false;
                actionPoints -= 1000 * (int)((200f - mobility) / 100f);
                x = nX; y = nY;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the creature's current state
        /// </summary>
        public string GetState()
        {
            return currState.name.ToString();
        }

        /// <summary>
        /// Configures the creature's state machine with the given states.
        /// TODO: We are forcefully cramming these states down the FSM's throat
        /// </summary>
        void ConfigureStateMachine()
        {
            // State setup
            State Alive = new State(
               StateID.Alive,
               () => { /*TODO: Yeet*/Console.WriteLine("Idle"); }
           );
            State Dead = new State(
                 StateID.Dead,
                () => { /*TODO: Yeet*/Console.WriteLine("Dead"); }
            );

            State Idle = new State(
                StateID.Idle,
                () => { /*TODO: Yeet*/Console.WriteLine("Idle"); }
            );


            State Moving = new State(
                 StateID.Moving,
                () => { /*TODO: Yeet*/Console.WriteLine("Moving"); }
            );

            // Initial state
            currState = Moving;

            // Establishes each state in the FSM and the transition triggers
            fsm.Configure(Alive)
                .Permit(TriggerID.Dies, Dead)
                .InitialTransition(Moving);

            fsm.Configure(Dead);

            fsm.Configure(Idle)
                .SubstateOf(Alive);
            //.Permit(TriggerID.Moves, Moving);

            fsm.Configure(Idle)
                //.SubstateOf(Alive)
                .Permit(TriggerID.Moves, Moving);

            fsm.Configure(Moving)
                .SubstateOf(Alive)
                .Permit(TriggerID.Stops, Idle);
        }

        // World tile position
        public int x, y;
        // World in which the creature resides
        World world;
        // Random number generator
        Random r;
        // State machine
        // Diagram: https://drive.google.com/file/d/1NLF4vdYOvJ5TqmnZLtRkrXJXqiRsnfrx/view?usp=sharing
        Stateless.StateMachine<State, TriggerID> fsm;
        // Current state
        State currState;

        int actionPoints;

        // TODO: Speed, for now only to test the FSM
        public float Speed = 10;
    }
}
