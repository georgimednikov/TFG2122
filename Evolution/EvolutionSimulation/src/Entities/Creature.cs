using System;
using System.Collections.Generic;
using EvolutionSimulation.FSM;
using EvolutionSimulation.FSM.Creature.States;
using EvolutionSimulation.FSM.Creature.Transitions;
using EvolutionSimulation.Genetics;
using System.Numerics;

namespace EvolutionSimulation.Entities
{
    /// <summary>
    /// A creature with attributes and behavior
    /// </summary>
    public abstract class Creature : IEntity
    {
        /// <summary>
        /// 
        /// </summary>
        // TODO: Puede ser que queramos aniadir mas de una interaccion
        public Dictionary<Interactions, Action<Creature>> InteractionsDict { get; private set; }

        /// <summary>
        /// Constructor for factories
        /// </summary>
        public Creature()
        {
            chromosome = new CreatureChromosome();
            stats = new CreatureStats();
            seenCreatures = new List<Creature>();
            seenEntities = new List<StableEntity>();
            InteractionsDict = new Dictionary<Interactions, Action<Creature>>();
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
            InteractionsDict.Add(Interactions.attack, OnAttack);
        }

        /// <summary>
        /// Simulation step
        /// </summary>
        public void Tick()
        {
            //toDie.value = (stats.currAge++ >= stats.lifeSpan);
            stats.currRest -= stats.restExpense;
            if (stats.currRest < 0) stats.currRest = 0;
            //toSleep.value = (stats.currRest <= 0.1 * stats.maxRest);
            //toWake.value = (stats.currRest >= stats.maxRest);
            mfsm.obtainActionPoints(stats.metabolism);

            seenCreatures.Clear();
            seenEntities.Clear();
            Perceive();

            MakeDecision();
            // TomarDecision(); (Asignar Criatura Objetivo) -> Trigger Transicion -> Cambio de estado
            do { mfsm.Evaluate(); } // While the creature can keep performing actions
            while (mfsm.Execute());// Maintains the evaluation - execution action
                                   // Creatura 1 ->  Ataca -> Creatura 2       Desde Creatura1 : Creatura2.Interact(Creatura 1, attack);
                                   //                                          Desde Creatura2 : Creatura1.Interact(Creatura2, attack);
            hasBeenHit = false; // TODO: Reset flags en general
        }

        /// <summary>
        /// Affects the transitions of the FSM based on perceived entities
        /// </summary>
        void MakeDecision()
        {
            if (seenCreatures.Count > 0 || hasBeenHit)    // TODO: hacer esto bien, 
            {                                             // ahora para atacar, muy WIP
                objective = seenCreatures[0];
                objectivePos = new Vector2(objective.x, objective.y);
            }
            else objective = null;
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
            IState attack = new Attacking(this);

            mfsm = new Fsm(idle);

            // Substates
            mfsm.AddSubstate(alive, idle);
            mfsm.AddSubstate(alive, moving);
            mfsm.AddSubstate(alive, eat);
            mfsm.AddSubstate(alive, sleep);
            mfsm.AddSubstate(alive, attack);

            // Transitions
            ITransition moveTransition = new MoveTransition(this);
            ITransition hungerTransition = new HungerTransition(this);
            ITransition sleepyTransition = new SleepyTransition(this);
            ITransition attackTransition = new AttackTransition(this);
            ITransition idleTransition = new IdleTransition(this);
            ITransition wakeTransition = new WakeTransition(this);
            ITransition dieTransition = new DieTransition(this);

            mfsm.AddTransition(idle, moveTransition, moving);
            mfsm.AddTransition(idle, hungerTransition, eat);
            mfsm.AddTransition(idle, sleepyTransition, sleep);
            mfsm.AddTransition(idle, attackTransition, attack);

            mfsm.AddTransition(moving, sleepyTransition, sleep);
            mfsm.AddTransition(moving, attackTransition, attack);
            mfsm.AddTransition(moving, idleTransition, idle);

            mfsm.AddTransition(attack, idleTransition, idle);

            mfsm.AddTransition(sleep, wakeTransition, idle);

            mfsm.AddTransition(alive, dieTransition, dead);
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
        void Perceive()
        {
            int perceptionRadius = 4; // TODO: calculate this using the Perception stat
            seenCreatures = world.PerceiveCreatures(this, x, y, perceptionRadius);
            seenEntities = world.PerceiveEntities(this, x, y, perceptionRadius);
        }

        /// <summary>
        /// Modifies the given stat based on age
        /// </summary>
        float ModifyStatByAge(float stat)
        {
            return stat * Math.Min(1.0f, (1 - startMultiplier) / (stats.lifeSpan * adulthoodThreshold) * stats.currAge + startMultiplier);
        }

        /// <summary>
        /// Manages an interaction between creatures
        /// </summary>
        public void ReceiveInteraction(Creature interacter, Interactions type)
        {
            if (InteractionsDict.ContainsKey(type))
                InteractionsDict[type](interacter);
        }

        private void OnAttack(Creature interacter)
        {
            stats.currHealth -= computeDamage(interacter.stats.damage, interacter.stats.perforation);
            objective = interacter;
            objectivePos = new Vector2(interacter.x, interacter.y);
            hasBeenHit = true; //O MEJOR INCLUSO forzar el cambio al igual que TomarDecision()
        }

        /// <summary>
        /// Returns the taken damage
        /// </summary>
        /// <param name="dmg">Incoming damage</param>
        /// <param name="pen">Damage penetratione</param>
        public float computeDamage(float dmg, float pen)
        {
            float amount = 0;
            amount = (dmg) - (stats.armor - pen);
            amount = Math.Max(0, amount);
            amount = Math.Min(amount, stats.currHealth);
            return amount;
        }

        /// <summary>
        /// Sets the stats of the creature.
        /// </summary>
        abstract public void SetStats();

        #region Attributes
        // World tile position
        public int x { get; private set; }
        public int y { get; private set; }
        // World in which the creature resides
        public World world { get; private set; }

        // Genetic
        public Species species;
        public CreatureChromosome chromosome { get; private set; }
        public CreatureStats stats { get; private set; }

        // List of creatures seen at this moment by this creature
        public List<Creature> seenCreatures { get; private set; }
        // List of entities seen at this moment by this creature
        public List<StableEntity> seenEntities { get; private set; }

        public int actionPoints;

        // State machine
        // Diagram: https://drive.google.com/file/d/1NLF4vdYOvJ5TqmnZLtRkrXJXqiRsnfrx/view?usp=sharing
        private Fsm mfsm;
        private float startMultiplier = 0.33f; //Starting multiplier of newborns
        private float adulthoodThreshold = 0.25f; //After which percentage of lifespan the creature has his stats not dimished by age

        public IEntity objective;
        Vector2 objectivePos;

        public bool hasBeenHit;

        #endregion
    }

    public class CreatureStats
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

    public enum Interactions { attack, initmidate, mate }
}
