﻿using System;
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
    public abstract class Creature : IEntity, IInteractable<Creature>
    {
        /// <summary>
        /// Constructor for factories
        /// </summary>
        public Creature()
        {
            chromosome = new CreatureChromosome();
            stats = new CreatureStats();
            seenCreatures = new List<Creature>();
            seenEntities = new List<StableEntity>();
            InteractionsDict = new Dictionary<Interactions, List<Action<Creature>>>();
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
            AddInteraction(Interactions.attack, OnAttack);

        }

        /// <summary>
        /// Simulation step
        /// </summary>
        public void Tick()
        {
            //toDie.value = (stats.CurrAge++ >= stats.LifeSpan);
            stats.CurrRest -= stats.RestExpense;
            //toSleep.value = (stats.CurrRest <= 0.1 * stats.MaxRest);
            //toWake.value = (stats.CurrRest >= stats.MaxRest);
            mfsm.obtainActionPoints(stats.Metabolism);
            
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
        /// Executes every response that this creature has to an interaction with other creature
        /// </summary>
        public void ReceiveInteraction(Creature interacter, Interactions type)
        {
            if (InteractionsDict.ContainsKey(type))
                foreach(Action<Creature> response in InteractionsDict[type])
                    response(interacter);
        }

        /// <summary>
        /// Adds a response to a interaction type, given 
        /// the creature that interacts with this.
        /// </summary>
        public void AddInteraction(Interactions type, Action<Creature> response)
        {
            if (!InteractionsDict.ContainsKey(type))
                InteractionsDict[type] = new List<Action<Creature>>();
            InteractionsDict[type].Add(response);
        }
        /// <summary>
        /// Removes a response to an interaction type, given the
        /// creature that interacts with this. 
        /// If the interaction type or the response is not registered, it does nothing.
        /// If no responses remain after the removal, it removes the interaction entry
        /// </summary>
        // TODO: Quitamos la entrada del diccionario si no quedan reacciones?
        public void RemoveInteraction(Interactions type, Action<Creature> response)
        {
            if (!InteractionsDict.ContainsKey(type)) return;

            InteractionsDict[type].Remove(response);
            if (InteractionsDict[type].Count == 0)
                InteractionsDict.Remove(type);
        }

        private void OnAttack(Creature interacter)
        {
            stats.CurrHealth -= computeDamage(interacter.stats.Damage, interacter.stats.Perforation);

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
            amount = (dmg) - (stats.Armor - pen);
            amount = Math.Max(0, amount);
            amount = Math.Min(amount, stats.CurrHealth);
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

        public IEntity objective;
        Vector2 objectivePos;

        public bool hasBeenHit;

        // Interactions that the creature can react to. Keys are the Interaction type
        // and values are the actions that the creature performs when something interacts with it.
        Dictionary<Interactions, List<Action<Creature>>> InteractionsDict;
        #endregion
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
        public float CurrHealth { get; set; }
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
