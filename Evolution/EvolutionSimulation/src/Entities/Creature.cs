﻿using System;
using EvolutionSimulation.FSM;
using EvolutionSimulation.FSM.Creature.States;
using EvolutionSimulation.FSM.Creature.Transitions;
using EvolutionSimulation.Genetics;

namespace EvolutionSimulation.src.Entities
{
    /// <summary>
    /// A creature with attributes and behavior
    /// </summary>
    public abstract class Creature : IEntity
    {
        /// <summary>
        /// Constructor for factories
        /// </summary>
        public Creature()
        {
            r = new Random();
            chromosome = new CreatureChromosome();
            stats = new CreatureStats();
            SetStats();
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
            actionPoints += stats.metabolism * 10;

            mfsm.Evaluate();
            mfsm.Execute();
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
            IState moving = new Moving(this);
            IState dead = new Dead();
            IState alive = new Alive();
            IState eat = new Eat();

            mfsm = new Fsm(idle);
            bool toMove = true;
            bool toIdle = false;
            bool toDie = false;
            bool toMunch = false;

            // Substates
            mfsm.AddSubstate(alive, idle);
            mfsm.AddSubstate(alive, moving);
            mfsm.AddSubstate(alive, eat);

            // Transitions
            mfsm.AddTransition(idle, new BooleanTransition(ref toMove), moving);
            mfsm.AddTransition(idle, new BooleanTransition(ref toMunch), eat);
            mfsm.AddTransition(moving, new BooleanTransition(ref toIdle), idle);
            mfsm.AddTransition(alive, new BooleanTransition(ref toDie), dead);
        }

        /// <summary>
        /// Moves a creature a specified amount
        /// </summary>
        public void Move(int x, int y)
        {
            this.x += x;
            this.y += y;
        }

        /// <summary>
        /// Places a creature in the designated coordinates
        /// </summary>
        public void Place(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        // World tile position
        public int x { get; private set; }
        public int y { get; private set; }
        // World in which the creature resides
        public World world { get; private set; }
        // Random number generator
        public Random r { get; private set; }
        // State machine
        // Diagram: https://drive.google.com/file/d/1NLF4vdYOvJ5TqmnZLtRkrXJXqiRsnfrx/view?usp=sharing
        private Fsm mfsm;

        // Genetic
        public CreatureChromosome chromosome { get; private set; }
        public CreatureStats stats;

        public int actionPoints;

        /// <summary>
        /// Sets the stats of the creature.
        /// </summary>
        abstract public void SetStats();
    }

    public struct CreatureStats
    {
        public Gender gender;

        //Nutrition related stats
        public Diet diet;
        public float scavenger; //From 0 (normal chance of getting poisoned) to 1 (cannot get poisoned)

        //Health and damage related stats
        public float maxHealth;
        public float currHealth;
        public int damage;
        public int armor;
        public int perforation;
        public float venom;
        public float counter;

        //Mobility related stats
        public int aerialSpeed;
        public int arborealSpeed;
        public int groundSpeed;

        //Reaches
        public bool airReach;
        public bool treeReach;

        //Energy related stats
        public float maxEnergy;
        public float currEnergy;
        public float energyExpense;

        //Hydratation related stats
        public float maxHydratation;
        public float currHydratation;
        public float hydratationExpense;

        //Rest related stats
        public float maxRest;
        public float currRest;
        public float restRecovery;
        public float restExpense;

        //Environment related stats
        public int camouflage;
        public int aggressiveness;
        public int intimidation;
        public int perception;
        public float nightDebuff;

        //Physique related stats
        public int size;
        public int lifeSpan;
        public int members;
        public int metabolism;
        public float minTemperature;
        public float maxTemperature;
        public float idealTemperature;

        //Behaviour related stats
        public int knowledge;
        public int paternity;

        //Multipliers
        public float healthRegeneration;
        public float maxSpeed;
    }
}