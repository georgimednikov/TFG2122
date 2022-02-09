using System;
using EvolutionSimulation.FSM;
using EvolutionSimulation.FSM.Creature.States;
using EvolutionSimulation.FSM.Creature.Transitions;
using EvolutionSimulation.Genetics;

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
            //chromosome = new CreatureChromosome();
            //stats = new CreatureStats(chromosome);
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
        public CreatureStats stats { get; private set; }

        public int actionPoints;

        public int GetScavenger() { return chromosome.GetFeature(CreatureFeature.Scavenger); }
    }

    public class CreatureStats
    {
        //The percentage of an ability that has to be had in order to unlock it
        private float abilityUnlock = 0.4f;

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
        public int percepcion;
        public int camouflage;
        public int aggressiveness;
        public int intimidation;

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
        public float nightDebuff;

        private CreatureChromosome chromosome;

        public CreatureStats(CreatureChromosome chr)
        {
            chromosome = chr;
            SetStats();
        }

        private void SetStats()
        {
            gender = chromosome.GetGender();

            //The max value is divided in ranges based on the amount of diets and then a diet is assigned based on the range it fall in
            diet = (Diet)(chromosome.GetFeature(CreatureFeature.Diet) / (chromosome.GetFeatureMax(CreatureFeature.Diet) / (int)Diet.Count));
            if (diet >= Diet.Count) diet = Diet.Count;

            int min = 10; //Minimum amount of health
            int value = 2; //Health gained per point of constitution
            //Minimum health plus bonus health
            maxHealth = chromosome.GetFeature(CreatureFeature.Constitution) * value + min;
            currHealth = maxHealth;

            damage = chromosome.GetFeature(CreatureFeature.Strength);
            armor = chromosome.GetFeature(CreatureFeature.Fortitude);
            perforation = chromosome.GetFeature(CreatureFeature.Piercing);

            bool wings = HasAbility(CreatureFeature.Wings);
            bool arboreal = HasAbility(CreatureFeature.Arboreal);
            bool upright = HasAbility(CreatureFeature.Upright);
            int mobility = chromosome.GetFeature(CreatureFeature.Mobility);
            airReach = wings;
            treeReach = wings || arboreal || upright;
            //TODO rellenar entre todos en concordancia con el resto de acciones para no mover muy rapido/lento
            if (!wings) aerialSpeed = -1;
            else aerialSpeed = chromosome.GetFeature(CreatureFeature.Wings);
            if (!arboreal) arborealSpeed = -1;
            else arborealSpeed = chromosome.GetFeature(CreatureFeature.Arboreal);
            groundSpeed = mobility;

            //maxEnergy = 100;
            //currEnergy = maxEnergy;
            //energyExpense;

            //maxHydratation = 100;
            //currHydratation = maxHydratation;
            //hydratationExpense;

            //maxRest = 100;
            //currRest = maxRest;
            //restRecovery;
            //restExpense;

            ////Environment related stats
            //percepcion;
            //camouflage;
            //aggressiveness;
            //intimidation;

            ////Physique related stats
            size = chromosome.GetFeature(CreatureFeature.Size);
            //lifeSpan;

            //TODO mover de sitio el numero maximo de patas
            int maxMembers = 10;
            members = chromosome.GetFeature(CreatureFeature.Members) / (chromosome.GetFeatureMax(CreatureFeature.Members) / maxMembers);
            if (members >= maxMembers) members = maxMembers;

            //metabolism;
            idealTemperature = chromosome.GetFeature(CreatureFeature.IdealTemperature);
            minTemperature = idealTemperature - chromosome.GetFeature(CreatureFeature.TemperatureRange);
            maxTemperature = idealTemperature + chromosome.GetFeature(CreatureFeature.TemperatureRange);

            ////Behaviour related stats
            //knowledge;
            //paternity;

            ////Multipliers
            //healthRegeneration;
            //maxSpeed;
            //nightDebuff;


            if (!HasAbility(CreatureFeature.Scavenger)) scavenger = -1;
            else scavenger = chromosome.GetFeature(CreatureFeature.Scavenger) / chromosome.GetFeatureMax(CreatureFeature.Scavenger);
            if (!HasAbility(CreatureFeature.Venomous)) venom = -1;
            else venom = chromosome.GetFeature(CreatureFeature.Venomous);
            if (!HasAbility(CreatureFeature.Thorns)) counter = -1;
            else counter = chromosome.GetFeature(CreatureFeature.Thorns);
        }

        private bool HasAbility(CreatureFeature feat)
        {
            return abilityUnlock < chromosome.GetFeature(feat) / chromosome.GetFeatureMax(feat);
        }
    }
}
