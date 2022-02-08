﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvolutionSimulation.FSM;
using EvolutionSimulation.FSM.Creature.States;
using EvolutionSimulation.FSM.Creature.Transitions;


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
        public int health { get; set; }

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
            return mfsm.GetState().ToString();
        }

        /// <summary>
        /// Configures the creature's state machine with the given states.
        /// TODO: We are forcefully cramming these states down the FSM's throat
        /// </summary>
        void ConfigureStateMachine()
        {
            // States
            IState idle = new Idle();
            IState moving = new Moving();
            IState dead = new Dead();
            IState alive = new Alive();
            IState eat = new Eat();

            mfsm = new Fsm(idle);
            bool b = false;
            bool al = false;
            bool de = true;

            // Substates
            mfsm.AddSubstate(alive, idle);
            mfsm.AddSubstate(alive, moving);
            mfsm.AddSubstate(alive, eat);

            // Transitions
            mfsm.AddTransition(idle, new BooleanTransition(ref b), moving);
            mfsm.AddTransition(idle, new BooleanTransition(ref de), eat);
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
        Fsm mfsm;

        int actionPoints;

        // TODO: Speed, for now only to test the FSM
        public float Speed = 10;
    }
}
