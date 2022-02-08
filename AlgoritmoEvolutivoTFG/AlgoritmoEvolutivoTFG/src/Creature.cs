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
            //this.fsm = new Stateless.StateMachine<State, TriggerID>(
            //    () => currState, 
            //    s => currState = s, 
            //    Stateless.FiringMode.Queued
            //);
            ConfigureStateMachine();
        }

        /// <summary>
        /// Simulation step
        /// </summary>
        public void Tick()
        {
            /*
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
                fsm.Fire(TriggerID.Dies);
            }
            else fsm.Fire(TriggerID.Stops);

            // Executes the action corresponding to the current state
            currState.action();
            /*/

            mfsm.Evaluate();
            mfsm.Execute();
            //*/
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
        /// </summary>
        public string GetState()
        {
            return "";// currState.name.ToString();
        }

        /// <summary>
        /// Configures the creature's state machine with the given states.
        /// TODO: We are forcefully cramming these states down the FSM's throat
        /// </summary>
        void ConfigureStateMachine()
        {

            // Initial state
            //currState = Moving;

            State idle = new Idle();
            State moving = new Moving();
            State dead = new Dead();
            State alive = new Alive();
            State eat = new Eating();
            mfsm = new StatelessFSM(idle);
            bool b = true;
            bool al = false;
            bool de = true;
            mfsm.AddSubstate(alive, idle);
            mfsm.AddSubstate(alive, moving);
            mfsm.AddSubstate(alive, eat);
            mfsm.AddTransition(idle, new BooleanTransition(ref de), eat);
            mfsm.AddTransition(idle, new BooleanTransition(ref b), moving);
            mfsm.AddTransition(moving, new BooleanTransition(ref b), idle);
            mfsm.AddTransition(alive, new BooleanTransition(ref al), dead);
        }

        // World tile position
        public int x, y;
        // World in which the creature resides
        World world;
        // Random number generator
        Random r;
        // State machine
        // Diagram: https://drive.google.com/file/d/1NLF4vdYOvJ5TqmnZLtRkrXJXqiRsnfrx/view?usp=sharing
        StatelessFSM mfsm;
        // Current state
        //State currState;

        // TODO: Speed, for now only to test the FSM
        public float Speed = 10;
    }
}
