using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmoEvolutivo
{
    /// <summary>
    /// A creature with attributes and behavior
    /// </summary>
    public class Creature : IEntity
    {
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
            // TODO: Put Fire in the states' actions
            // TODO: and yeet this outta here
            if (currState.name == StateID.Dead) return;
            Speed--;
            if (Speed <= 0)
            {
                Speed = 0;
                fsm.Fire(TriggerID.Dies);
            }
            else if (Speed % 2 == 0)
            {
                fsm.Fire(TriggerID.Moves);
            }
            else fsm.Fire(TriggerID.Stops);

            // Executes the action corresponding to the current state
            currState.action();
            Move();
        }

        /// <summary>
        /// Attempts to move the creature to an adjacent tile
        /// </summary>
        void Move()
        {
            int nX = x + r.Next(-1, 2),
                nY = y + r.Next(-1, 2);
            if (world.canMove(nX, nY)) { x = nX; y = nY; }
        }

        /// <summary>
        /// Returns the creature's current state
        /// </summary>W
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
                .SubstateOf(Alive)
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

        // TODO: Speed, for now only to test the FSM
        public float Speed = 10;
    }
}
