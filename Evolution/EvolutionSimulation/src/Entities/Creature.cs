using System;
using System.Collections.Generic;
using EvolutionSimulation.FSM;
using EvolutionSimulation.FSM.Creature.States;
using EvolutionSimulation.FSM.Creature.Transitions;
using EvolutionSimulation.Genetics;

namespace EvolutionSimulation.Entities
{
    /// <summary>
    /// A creature with attributes and behavior
    /// </summary>
    public abstract class Creature : IEntity
    {
        BooleanWrapper toMove, toIdle, toDie, toMunch,
            toSleep, toWake;

        /// <summary>
        /// Constructor for factories
        /// </summary>
        public Creature()
        {
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
            toDie.value = (stats.currAge++ >= stats.lifeSpan);
            stats.currRest -= stats.restExpense;
            if (stats.currRest < 0) stats.currRest = 0;
            toSleep.value = (stats.currRest <= 0.1 * stats.maxRest);
            toWake.value = (stats.currRest >= stats.maxRest);
            mfsm.obtainActionPoints(stats.metabolism);
            
            seenEntities = Percieve();

            do { mfsm.Evaluate(); } // While the creature can keep performing actions
            while (mfsm.Execute());// Maintains the evaluation - execution action
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
            IState idle = new Idle(this);
            IState moving = new Moving(this);
            IState dead = new Dead(this);
            IState alive = new Alive(this);
            IState eat = new Eat(this);
            IState sleep = new Sleeping(this);

            mfsm = new Fsm(idle);
            toMove = new BooleanWrapper(false);
            toIdle = new BooleanWrapper(false);
            toDie = new BooleanWrapper(false);
            toMunch = new BooleanWrapper(false);
            toSleep = new BooleanWrapper(false);
            toWake = new BooleanWrapper(false);

            // Substates
            mfsm.AddSubstate(alive, idle);
            mfsm.AddSubstate(alive, moving);
            mfsm.AddSubstate(alive, eat);
            mfsm.AddSubstate(alive, sleep);

            // Transitions
            mfsm.AddTransition(idle, new BooleanTransition(toMove), moving);
            mfsm.AddTransition(idle, new BooleanTransition(toMunch), eat);
            mfsm.AddTransition(moving, new BooleanTransition(toIdle), idle);
            mfsm.AddTransition(moving, new BooleanTransition(toSleep), sleep);
            mfsm.AddTransition(idle, new BooleanTransition(toSleep), sleep);
            mfsm.AddTransition(sleep, new BooleanTransition(toWake), idle);
            mfsm.AddTransition(alive, new BooleanTransition(toDie), dead);
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

        /// <summary>
        /// Checks the perception area around this entity for other entities
        /// </summary>
        /// <returns>A list of every other entity in the area</returns>
        List<IEntity> Percieve()
        {
            int perceptionRadius = 4; // TODO: calculate this using the Perception stat
            List<IEntity> list = new List<IEntity>();

            foreach (IEntity e in world.Creatures) // TODO: use this?
            {
                if (e == this) continue; // Reference comparison
                if (Math.Abs(e.x - x) <= perceptionRadius && Math.Abs(e.y - y) <= perceptionRadius) // Square vision
                {
                    list.Add(e);
                    Console.WriteLine("Seeing " + e.ToString()); // TODO: Remove
                }
            }
            Console.WriteLine(); // TODO: Remove

            return list;
        }

        /// <summary>
        /// Modifies the given stat based on age
        /// </summary>
        float ModifyStatByAge(float stat)
        {
            return stat * Math.Min(1.0f, (1 - startMultiplier) / (stats.lifeSpan * adulthoodThreshold) * stats.currAge + startMultiplier);
        }

        /// <summary>
        /// Sets the stats of the creature.
        /// </summary>
        abstract public void SetStats();

        // World tile position
        public int x { get; private set; }
        public int y { get; private set; }
        // World in which the creature resides
        public World world { get; private set; } 

        // Genetic
        public CreatureChromosome chromosome { get; private set; }
        public CreatureStats stats { get; private set; }
        
        // List of entities seen at this moment by this creature
        public List<IEntity> seenEntities { get; private set; }

        public int actionPoints;

        // State machine
        // Diagram: https://drive.google.com/file/d/1NLF4vdYOvJ5TqmnZLtRkrXJXqiRsnfrx/view?usp=sharing
        private Fsm mfsm;
        private float startMultiplier = 0.33f; //Starting multiplier of newborns
        private float adulthoodThreshold = 0.25f; //After which percentage of lifespan the creature has his stats not dimished by age
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
        public int currAge;
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
