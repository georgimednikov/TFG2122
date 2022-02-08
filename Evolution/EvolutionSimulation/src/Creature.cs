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
            ConfigureStateMachine();
        }

        /// <summary>
        /// Simulation step
        /// </summary>
        public void Tick()
        {
            actionPoints += metabolism * 10;

            mfsm.Evaluate();
            mfsm.Execute();

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
            return mfsm.State.ToString();
        }

        /// <summary>
        /// Configures the creature's state machine with the given states.
        /// TODO: We are forcefully cramming these states down the FSM's throat
        /// </summary>
        void ConfigureStateMachine()
        {
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

        int actionPoints;

        // TODO: Speed, for now only to test the FSM
        public float Speed = 10;
    }
}
