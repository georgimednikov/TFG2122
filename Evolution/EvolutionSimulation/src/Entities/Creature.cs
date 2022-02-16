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
            toDie.value = (stats.CurrAge++ >= stats.LifeSpan);
            stats.CurrRest -= stats.RestExpense;
            toSleep.value = (stats.CurrRest <= 0.1 * stats.MaxRest);
            toWake.value = (stats.CurrRest >= stats.MaxRest);
            mfsm.obtainActionPoints(stats.Metabolism);
            
            seenEntities = Percieve();
            Console.WriteLine(stats.MaxEnergy);
            Console.WriteLine(stats.CurrEnergy);
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
                }
            }

            return list;
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
    }

    public class CreatureStats
    {
        private float startMultiplier = 0.33f; //Starting multiplier of newborns
        private float adulthoodThreshold = 0.25f; //After which percentage of lifespan the creature has his stats not dimished by age

        /// <summary>
        /// Modifies the given stat based on age
        /// </summary>
        float ModifyStatByAge(float stat)
        {
            return stat * Math.Min(1.0f, (1 - startMultiplier) / (LifeSpan * adulthoodThreshold) * currAge + startMultiplier);
        }

        public Gender Gender { get; set; }

        //Nutrition related stats
        public Diet Diet { get; set; }
        public float Scavenger { get; set; } //From 0 (normal chance of getting poisoned) to 1 (cannot get poisoned)

        //Health and damage related stats
        float maxHealth;
        public float MaxHealth { get { return ModifyStatByAge(maxHealth); }
            set { maxHealth = value; /* If maxHealth changes, currHealth changes the difference */ CurrHealth += MaxHealth - CurrHealth; } }
        public float CurrHealth { get; private set; }
        int damage;
        public int Damage { get { /* Minimum damage is 1 */ return (int)Math.Ceiling(ModifyStatByAge(damage)); } set { damage = value; } }
        int armor;
        public int Armor { get { return (int)ModifyStatByAge(armor); } set { armor = value; } }
        int perforation;
        public int Perforation { get { return (int)ModifyStatByAge(perforation); } set { perforation = value; } }
        float venom;
        public float Venom { get { return ModifyStatByAge(venom); } set { venom = value; } }
        float counter; // Puas
        public float Counter { get { return ModifyStatByAge(counter); } set { counter = value; } }

        //Mobility related stats
        public int AerialSpeed { get; set; }
        public int ArborealSpeed { get; set; }
        public int GroundSpeed { get; set; }

        //Reaches
        public bool AirReach { get; set; } // TODO: que afecte la edad?
        public bool TreeReach { get; set; }

        //Energy related stats
        float maxEnergy;
        public float MaxEnergy { get { return maxEnergy; }
            set { maxEnergy = value; } }
        public float CurrEnergy { get; set; }
        public float EnergyExpense { get; set; }

        //Hydration related stats
        public float MaxHydration { get; set; }
        public float CurrHydration { get; set; }
        public float HydrationExpense { get; set; }

        //Rest related stats
        public float MaxRest { get; set; }
        float currRest;
        public float CurrRest { get { return currRest; } set { currRest = value; if (currRest < 0) currRest = 0; } }
        public float RestRecovery { get; set; }
        public float RestExpense { get; set; }

        //Environment related stats
        public int Camouflage { get; set; }
        int aggressiveness;
        public int Aggressiveness { get { return (int)ModifyStatByAge(aggressiveness); } set { aggressiveness = value; } }
        int intimidation;
        public int Intimidation { get { return (int)ModifyStatByAge(intimidation); } set { intimidation = value; } }
        public int Perception { get; set; }
        public float NightDebuff { get; set; }

        //Physique related stats
        int size;
        public int Size { get { return (int)ModifyStatByAge(size); } set { size = value; } }
        public int LifeSpan { get; set; }
        int currAge;
        public int CurrAge { get { return currAge; } 
            set { float oldMaxH = MaxHealth; currAge = value; CurrHealth += MaxHealth - oldMaxH; } }
        public int Members { get; set; }//limbs
        public int Metabolism { get; set; }
        public float MinTemperature { get; set; }
        public float MaxTemperature { get; set; }
        public float IdealTemperature { get; set; }

        //Behaviour related stats
        public int Knowledge { get; set; }
        public int Paternity { get; set; }

        //Multipliers
        public float HealthRegeneration { get; set; }
        public float MaxSpeed { get; set; }
    }
}
