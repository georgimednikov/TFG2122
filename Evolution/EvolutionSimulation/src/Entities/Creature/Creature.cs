using System;
using System.Collections.Generic;
using EvolutionSimulation.FSM;
using EvolutionSimulation.FSM.Creature.States;
using EvolutionSimulation.FSM.Creature.Transitions;
using EvolutionSimulation.Genetics;
using EvolutionSimulation.Entities.Status;
using System.Numerics;

using Telemetry;
using Telemetry.Events;

namespace EvolutionSimulation.Entities
{
    public enum CauseOfDeath
    {
        Temperature,
        Attack,
        Retalliation,
        Starved,
        Dehydration,
        Exhaustion,
        Poison,
        Longevity,
        NONE
    }

    /// <summary>
    /// ID to identify an entity in the world
    /// </summary>
    public abstract class Creature : IEntity, IInteractable<Creature>
    {
        /// <summary>
        /// Creature ID to identify a creature in the world
        /// </summary>
        public int ID { get; protected set; }

        /// <summary>
        /// Variable used to somewhat accurately determine the cuas eof the creature's death at the time of the creation of its corpse
        /// </summary>
        public CauseOfDeath causeOfDeath = CauseOfDeath.NONE;
        /// <summary>
        /// Damage that first educed the creature's health below zero
        /// </summary>
        public double killingBlow;
        /// <summary>
        /// ID of the one who dealt the killing blow, if applicable.
        /// </summary>
        public int killerID;

        /// <summary>
        /// Constructor for factories
        /// </summary>
        public Creature()
        {
            InteractionsDict = new Dictionary<Interactions, List<Action<Creature>>>();
            activeStatus = new List<Status.Status>();
            removedStatus = new List<Status.Status>();
        }

        /// <summary>
        /// Initializes a creature in a world and position
        /// </summary>
        /// <param name="w">World in which it'll reside</param>
        public void Init(int ID, World w, int x, int y, CreatureChromosome chromosome = default(CreatureChromosome), string name = "None", int fatherID = -1, int motherID = -1)
        {
            this.ID = ID;
            world = w;
            if (chromosome == null)
            {
                this.chromosome = new CreatureChromosome();
            }
            else
            {
                this.chromosome = chromosome;
            }
            speciesName = name;
            stats = new CreatureStats();
            SetStats();
            mind = new Mind(this, fatherID, motherID);
            this.x = x;
            this.y = y;
            timeToBeInHeat = stats.TimeBetweenHeats;
            halfMaxMobility = this.chromosome.GetFeatureMax(CreatureFeature.Mobility) / 2;

            ConfigureStateMachine();
            SetUpInteractions();
            //Console.WriteLine(mfsm.ExportToDotGraph());
        }

        /// <summary>
        /// Simulation step
        /// </summary>
        public bool Tick()
        {
            Expend();
            ManageHealth();
            CheckTemperature();
            FemaleTick();

            mind.UpdatePriorities();
            foreach (Status.Status s in activeStatus)   // Activates each status effect
                if (s.OnTick()) RemoveStatus(s, true);  // removing it when necessary

            // Action points added every tick 
            ActionPoints += stats.Metabolism * (int)Math.Ceiling((float)UniverseParametersManager.parameters.baseActionCost / (chromosome.GetFeatureMax(CreatureFeature.Metabolism) / 2));

            // Executes the state action if the creature has enough Action Points
            int cost = 0;
            while ((cost = mfsm.EvaluateCost()) <= ActionPoints)
            {
                mfsm.CurrentState.Action();
                ActionPoints -= cost;
#if DEBUG
                Console.WriteLine(GetStateInfo());
#else 
                GetStateInfo();
#endif
            }

            Clear();
            return true;
        }

        public void CycleDayNight()
        {
            //If the creature sees normally (1 = 100% of its vision) it was day and now is night.
            if (stats.CurrentVision == 1)
                stats.CurrentVision = stats.NightPerceptionPercentage; //Instead of 1 now it sees from minNightVision to 1 depending on its feature.

            //Else it was night now it is day again and begins to see normally.
            else
                stats.CurrentVision = 1;

            mind.UpdatePerception();
        }


        /// <returns> Return True if the position is in a confortable temperature</returns>
        public bool CheckTemperature(int x, int y)
        {
            double tileTemperature = world.map[x, y].temperature;

            return tileTemperature > stats.MinTemperature && tileTemperature < stats.MaxTemperature;
        }

        /// <summary>
        /// Check if the creature is in a position where is confortable with the temperature
        /// Otherwise the creature will take damage
        /// </summary>
        void CheckTemperature()
        {
            double tileTemperature = world.map[x, y].temperature;
            double difference;

            //The difference between the extreme acceptable temperature an the tile temperature is calculated.
            if (tileTemperature < stats.MinTemperature)
                difference = stats.MinTemperature - tileTemperature;
            else if (tileTemperature > stats.MaxTemperature)
                difference = tileTemperature - stats.MaxTemperature;
            //If the creature is confortable nothing happens.
            else
                return;

            //A range from 0 to 1 is calculated based on the difference of temperature and a max value for it.
            double range = Math.Min(difference / UniverseParametersManager.parameters.maxTemperatureDifference, 1);
            //The base damage of being in an area with a temperature that cannot be stand is a porcentage of the max health each tick.
            //To that, another instance of damage is added depending on how much this temperature supasses that acceptable for the creature.
            double damage = stats.MaxHealth *
                ((range * (UniverseParametersManager.parameters.maxHealthTemperatureDamage - UniverseParametersManager.parameters.minHealthTemperatureDamage)) +
                UniverseParametersManager.parameters.minHealthTemperatureDamage);
            stats.CurrHealth -= (float)damage;

            Tracker.Instance.Track(new CreatureReceiveDamage(world.tick, ID, speciesName, -1, (float)damage, DamageType.Temperature, stats.CurrHealth));

            if (causeOfDeath == CauseOfDeath.NONE && stats.CurrHealth <= 0)
            {
                causeOfDeath = CauseOfDeath.Temperature;
                killingBlow = damage;
                killerID = ID;  // TODO: -1 o esto?
            }
#if DEBUG            
            Console.WriteLine("CreatureId: " + ID + "  " + "temperature difference: " + difference + ", which dealt " + damage + " damage");
#endif

            mind.CreateDanger();
        }

        /// <summary>
        /// Clear the lists and the objectives
        /// </summary>
        void Clear()
        {
            // Clears the statuses marked for deletion
            foreach (Status.Status s in removedStatus)
                activeStatus.Remove(s);
            removedStatus.Clear();
        }

        /// <summary>
        /// Update the inHeat stat of a female.
        /// Also check if she want to mate or not
        /// </summary>
        void FemaleTick()
        {
            if (stats.Gender == Gender.Female && !stats.IsNewBorn())
            {
                if (timeToBeInHeat == 0)//Can be pregnant
                    stats.InHeat = true;
                else if (timeToBeInHeat <= -1)// Pregnant, reset timer
                {
                    timeToBeInHeat = stats.TimeBetweenHeats;
                    stats.InHeat = false;
                }
                else
                    timeToBeInHeat--;

                //if the female has to do something, she doesn't want to mate
                if (IsExhausted() || IsVeryHungry() || IsVeryThirsty()
                    || mating || !stats.InHeat)
                {
                    wantMate = false;
                }
                else wantMate = true;
            }
        }

        /// <summary>
        /// Lowers the creature's current energy, rest and hydration
        /// Amount varies depending on the creature's abilities
        /// </summary>
        void Expend()
        {
            stats.CurrHydration = Math.Max(stats.CurrHydration - stats.HydrationExpense, 0);
            stats.CurrRest = Math.Max(stats.CurrRest - stats.RestExpense, 0);
            stats.CurrEnergy = Math.Max(stats.CurrEnergy - stats.EnergyExpense, 0);
            stats.CurrAge++;
            if (stats.CurrAge >= stats.LifeSpan)
                causeOfDeath = CauseOfDeath.Longevity;
        }

        /// <summary>
        /// Attempts to regenrate the creature's health if is healthy, 
        /// if the stats are 0 then reduce his health.
        /// Checks first if it can with current energy and rest
        /// And then regenrates a percentage of the creature's max hp
        /// based on how much resources it has
        /// </summary>
        void ManageHealth()
        {

            if (stats.CurrEnergy <= 0 || stats.CurrRest <= 0 || stats.CurrHydration <= 0)
            {
                stats.CurrHealth -= 1;  // TODO: Numero magico

                DamageType dtype = DamageType.Starvation;
                if (stats.CurrEnergy <= 0)
                    dtype = DamageType.Starvation;
                else if (stats.CurrHydration <= 0)
                    dtype = DamageType.Dehydration;
                else if (stats.CurrRest <= 0)
                    dtype = DamageType.Exhaustion;

                Tracker.Instance.Track(new CreatureReceiveDamage(world.tick, ID, speciesName, -1, 1, dtype, stats.CurrHealth));

                if (causeOfDeath == CauseOfDeath.NONE && stats.CurrHealth <= 0)
                {
                    if (stats.CurrEnergy <= 0)
                        causeOfDeath = CauseOfDeath.Starved;
                    else if (stats.CurrHydration <= 0)
                        causeOfDeath = CauseOfDeath.Dehydration;
                    else if (stats.CurrRest <= 0)
                        causeOfDeath = CauseOfDeath.Exhaustion;

                    killingBlow = 1;
                    killerID = -1;
                }
            }
            else if (stats.CurrEnergy >= (stats.MaxEnergy * UniverseParametersManager.parameters.energyRegenerationThreshold) &&
                stats.CurrRest >= (stats.MaxRest * UniverseParametersManager.parameters.restRegenerationThreshold) &&
                stats.CurrHydration >= (stats.MaxHydration * UniverseParametersManager.parameters.hydrationRegenerationThreshold) &&
                CheckTemperature(x, y) && stats.CurrHealth > 0)
            {
                float pE = (stats.CurrEnergy - (stats.MaxEnergy * UniverseParametersManager.parameters.energyRegenerationThreshold)) /  // Percentage of surpassed thresholds
                    (stats.MaxEnergy - (stats.MaxEnergy * UniverseParametersManager.parameters.energyRegenerationThreshold));
                float pR = (stats.CurrRest - (stats.MaxRest * UniverseParametersManager.parameters.energyRegenerationThreshold)) /
                    (stats.MaxRest - (stats.MaxRest * UniverseParametersManager.parameters.energyRegenerationThreshold));
                float pH = (stats.CurrHydration - (stats.MaxHydration * UniverseParametersManager.parameters.energyRegenerationThreshold)) /
                    (stats.MaxHydration - (stats.MaxHydration * UniverseParametersManager.parameters.energyRegenerationThreshold));

                float medPercent = (pE + pR + pH) / 3.0f;   // Average percentage of suprassed thresholds

                stats.CurrHealth += (UniverseParametersManager.parameters.regenerationRate * stats.MaxHealth * medPercent);  // TODO: Ver si esto esta bien, ingenieria de valores
                stats.CurrHealth = Math.Min(stats.CurrHealth, stats.MaxHealth); // So it does not get over-healed
            }
        }

        #region Genetics and Taxonomy

        // Taxonomy
        public string speciesName;
        public string progenitorSpeciesName;

        // Genetics
        public CreatureChromosome chromosome { get; private set; }
        public CreatureStats stats { get; private set; }

        /// <summary>
        /// Sets the stats of the creature.
        /// </summary>
        abstract public void SetStats();
        #endregion

        #region State Machine
        public int ActionPoints { get; private set; }

        // State machine
        // Diagram: https://drive.google.com/file/d/1NLF4vdYOvJ5TqmnZLtRkrXJXqiRsnfrx/view?usp=sharing
        private Fsm mfsm;

        /// <summary>
        /// Returns the creature's current state
        /// </summary>
        public string GetState()
        {
            IState state = mfsm.CurrentState;
            while (state is CompoundState)
                state = (state as CompoundState).stateMachine.CurrentState;
            return state.ToString();
        }

        /// <summary>
        /// Returns specific information about the current state
        /// </summary>
        public string GetStateInfo()
        {
            IState state = mfsm.CurrentState;
            while (state is CompoundState)
                state = (state as CompoundState).stateMachine.CurrentState;
            return state.GetInfo();
        }

        /// <summary>
        /// Configures the creature's state machine with default states.
        /// </summary>
        void ConfigureStateMachine()
        {
            // Alive state configuration
            // States
            // Safe-state configuration
            // States
            IState wander = new Wander(this);
            IState explore = new Explore(this);
            IState goToDrink = new GoToDrink(this);
            IState drink = new Drinking(this);
            IState goToMate = new GoToMate(this);
            IState tryMate = new TryMate(this);
            IState mating = new Mating(this, UniverseParametersManager.parameters.ticksToReproduce);
            IState goToEat = new GoToEat(this);
            IState eat = new Eating(this);
            IState goToSafePlace = new GoToSafePlace(this);
            IState goToSafeTemperaturePlace = new GoToSafeTemperaturePlace(this);
            IState sleep = new Sleeping(this);
            Fsm safeFSM = new Fsm(wander);
            IState safe = new CalmState("Safe", safeFSM, this);

            // Done exploring
            ITransition doneExploringTransition = new DoneExploringTransition(this);
            safeFSM.AddTransition(explore, doneExploringTransition, wander);

            // TODO queremos esto? si lo dejamos, quitar comprobaciones en las transiciones de si tiene la habilidad o no
            //Follow Parent
            if (stats.Paternity > 0)
            {
                IState followParent = new FollowParent(this);
                ITransition followParentTransition = new FollowParentTransition(this);
                ITransition stopFollowParentTransition = new StopFollowParentTransition(this);
                safeFSM.AddTransition(wander, followParentTransition, followParent);
                safeFSM.AddTransition(followParent, stopFollowParentTransition, wander);
            }

            ITransition goToSafeTempPlaceTransition = new GoToSafeTemperaturePlaceTransition(this);
            ITransition stopGoToSafeTempPlaceTransition = new StopGoToSafeTemperaturePlaceTransition(this);
            ITransition goToSafeTempPlaceExploreTransition = new GoToSafeTemperaturePlaceExploreTransition(this);
            safeFSM.AddTransition(wander, goToSafeTempPlaceTransition, goToSafeTemperaturePlace);
            safeFSM.AddTransition(goToSafeTemperaturePlace, stopGoToSafeTempPlaceTransition, wander);
            safeFSM.AddTransition(wander, goToSafeTempPlaceExploreTransition, explore);

            // Sleeping
            ITransition goToSafePlaceTransition = new GoToSafePlaceTransition(this);
            ITransition stopGoToSafePlaceTransition = new StopGoToSafePlaceTransition(this);
            ITransition safePlaceExploreTransition = new SafePlaceExploreTransition(this);
            ITransition sleepySafeTransition = new SleepySafeTransition(this);
            ITransition sleepyTransition = new SleepyTransition(this);
            ITransition wakeTransition = new WakeTransition(this);
            safeFSM.AddTransition(wander, sleepyTransition, sleep);
            safeFSM.AddTransition(wander, safePlaceExploreTransition, explore);
            safeFSM.AddTransition(wander, goToSafePlaceTransition, goToSafePlace);
            safeFSM.AddTransition(goToSafePlace, stopGoToSafePlaceTransition, wander);
            safeFSM.AddTransition(goToSafePlace, sleepySafeTransition, sleep);
            safeFSM.AddTransition(explore, sleepyTransition, sleep);
            safeFSM.AddTransition(sleep, wakeTransition, wander);

            // Drinking
            ITransition thirstyTransition = new ThirstyTransition(this);
            ITransition drinkingTransition = new DrinkingTransition(this);
            ITransition drinkingExploreTransition = new DrinkingExploreTransition(this);
            ITransition stopDrinkingTransition = new StopDrinkingTransition(this);
            ITransition stopGoToDrinkTransition = new StopGoToDrinkTransition(this);
            safeFSM.AddTransition(wander, drinkingExploreTransition, explore);
            safeFSM.AddTransition(wander, thirstyTransition, goToDrink);
            safeFSM.AddTransition(goToDrink, stopGoToDrinkTransition, wander);
            safeFSM.AddTransition(goToDrink, drinkingTransition, drink);
            safeFSM.AddTransition(drink, stopDrinkingTransition, wander);
            safeFSM.AddTransition(sleep, drinkingExploreTransition, explore);
            safeFSM.AddTransition(sleep, thirstyTransition, goToDrink);

            // Eating
            ITransition hungerTransition = new HungerTransition(this);
            ITransition hungerExploreTransition = new HungerExploreTransition(this);
            ITransition eatingTransition = new EatingTransition(this);
            ITransition stopEatingTransition = new StopEatingTransition(this);
            ITransition stopGoToEatTransition = new StopGoToEatTransition(this);
            safeFSM.AddTransition(wander, hungerExploreTransition, explore);
            safeFSM.AddTransition(wander, hungerTransition, goToEat);
            safeFSM.AddTransition(goToEat, stopGoToEatTransition, wander);
            safeFSM.AddTransition(goToEat, eatingTransition, eat);
            safeFSM.AddTransition(eat, stopEatingTransition, wander);
            safeFSM.AddTransition(sleep, hungerExploreTransition, explore);
            safeFSM.AddTransition(sleep, hungerTransition, goToEat);

            // Mating
            ITransition mateTransition = new GoToMateTransition(this);
            ITransition tryMateTransition = new TryMateTransition(this);
            ITransition matingTransition = new MatingTransition(this);
            ITransition matingExploreTransition = new MatingExploreTransition(this);
            ITransition stopMatingTransition = new StopMatingTransition(this);
            ITransition stopGoToMateTransition = new StopGoToMateTransition(this);
            ITransition stopTryMateTransition = new StopTryMateTransition(this);

            safeFSM.AddTransition(wander, matingExploreTransition, explore);
            safeFSM.AddTransition(wander, mateTransition, goToMate);
            safeFSM.AddTransition(goToMate, stopGoToMateTransition, wander);
            safeFSM.AddTransition(goToMate, tryMateTransition, tryMate);
            safeFSM.AddTransition(tryMate, matingTransition, mating);
            safeFSM.AddTransition(tryMate, stopTryMateTransition, wander);
            safeFSM.AddTransition(wander, matingTransition, mating);
            safeFSM.AddTransition(goToDrink, matingTransition, mating);
            safeFSM.AddTransition(goToEat, matingTransition, mating);
            safeFSM.AddTransition(goToSafePlace, matingTransition, mating);
            safeFSM.AddTransition(mating, stopMatingTransition, wander);

            // Escape-state Configuration
            // States
            IState fleeing = new Fleeing(this);
            //IState hide = new Hide(this);
            Fsm escapeFSM = new Fsm(fleeing);
            // Transitions
            ITransition fleeTransition = new FleeTransition(this);
            //ITransition hideTranistion = new HideTransition(this);
            //escapeFSM.AddTransition(fleeing, hideTranistion, hide);
            //escapeFSM.AddTransition(hide, fleeTransition, fleeing);
            IState escape = new CompoundState("Escape", escapeFSM);

            // Combat-state Configuration
            // States
            IState attack = new Attacking(this);
            IState chaseEnemy = new ChaseEnemy(this);
            Fsm combatFSM = new Fsm(chaseEnemy);
            // Transitions
            ITransition attackTransition = new AttackTransition(this);
            ITransition chaseEnemyTransition = new ChaseEnemyTransition(this);
            combatFSM.AddTransition(chaseEnemy, attackTransition, attack);
            combatFSM.AddTransition(attack, chaseEnemyTransition, chaseEnemy);
            IState combat = new CompoundState("Combat", combatFSM);

            Fsm aliveFSM = new Fsm(safe);
            // Transitions
            ITransition escapeTransition = new EscapeTransition(this);
            ITransition safeTransition = new SafeTransition(this);
            ITransition combatTransition = new CombatTransition(this);
            aliveFSM.AddTransition(safe, escapeTransition, escape);
            aliveFSM.AddTransition(safe, combatTransition, combat);
            aliveFSM.AddTransition(combat, safeTransition, safe);
            aliveFSM.AddTransition(combat, escapeTransition, escape);
            aliveFSM.AddTransition(escape, safeTransition, safe);
            aliveFSM.AddTransition(escape, combatTransition, combat);

            IState alive = new CompoundState("Alive", aliveFSM);
            IState dead = new Dead(this);

            mfsm = new Fsm(alive);
            // Transitions
            ITransition dieTransition = new DieTransition(this);
            mfsm.AddTransition(alive, dieTransition, dead);
        }

        #endregion

        #region Interactions
        // Interactions that the creature can react to. Keys are the Interaction type
        // and values are the actions that the creature performs when something interacts with it.
        Dictionary<Interactions, List<Action<Creature>>> InteractionsDict;

        // Handler for interaction events
        public delegate void ReceiveInteractionHandler(Creature receiver, Creature sender, Interactions type);
        public event ReceiveInteractionHandler ReceiveInteractionEvent; // TODO: asi?

        /// <summary>
        /// Set the interactions that the creature has
        /// </summary>
        protected void SetUpInteractions()
        {
            // Attack
            AddInteraction(Interactions.attack, ReceiveDamage);
            if (this.chromosome.HasAbility(CreatureFeature.Thorns, CreatureChromosome.AbilityUnlock[CreatureFeature.Thorns]))
                AddInteraction(Interactions.attack, RetalliateDamage);

            // Poison
            AddInteraction(Interactions.poison, Poison);

            // Mate
            AddInteraction(Interactions.mate, OnMate);
            AddInteraction(Interactions.stopMate, StopMating);
        }

        // Methods to receive and respond to interactions
        /// <summary>
        /// Executes every response that this creature has to an interaction with other creature
        /// </summary>
        public void ReceiveInteraction(Creature interacter, Interactions type)
        {
            if (InteractionsDict.ContainsKey(type))
            {
                foreach (Action<Creature> response in InteractionsDict[type])
                    response(interacter);
                ReceiveInteractionEvent?.Invoke(this, interacter, type);
            }
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
        public void RemoveInteraction(Interactions type, Action<Creature> response)
        {
            if (!InteractionsDict.ContainsKey(type)) return;

            InteractionsDict[type].Remove(response);
            if (InteractionsDict[type].Count == 0)
                InteractionsDict.Remove(type);
        }

        /// <summary>
        /// Determines whether the creature has enough health to fight or if it's too weak.
        /// </summary>
        /// <returns></returns>
        public bool AbleToFight()
        {
            // Health threshold above which the creature will fight
            float threshold = 0.25f - (stats.Aggressiveness / UniverseParametersManager.parameters.combatTransitionHealthThresholdMultiplier); // TODO: A mayor agresividad mas se arriesga, revisar cifras
            return stats.CurrHealth >= stats.MaxHealth * threshold;
        }

        /// <summary>
        /// Determines whether the creature has enough health to fight or if it's too weak.
        /// </summary>
        /// <returns></returns>
        public List<Creature> CombatPack()
        {
            //If the creature has enough allies to put up a fight the gank him together.
            List<int> allies = mind.NearbyAllies();
            List<Creature> pack = new List<Creature>();
            foreach (int ally in allies)
            {
                Creature a = world.GetCreature(ally);
                if (a != null && a.AbleToFight())
                    pack.Add(a);
            }
            if (AbleToFight()) pack.Add(this);
            return pack;
        }

        public bool ShouldPackFight(List<Creature> pack, float danger)
        {
            return stats.Aggressiveness * pack.Count >= danger;
        }

        // Standard reactions to interactions

        /// <summary>
        /// Returns the taken damage
        /// </summary>
        /// <param name="dmg">Incoming damage</param>
        /// <param name="pen">Damage penetratione</param>
        private float ComputeDamage(float dmg, float pen)
        {
            float amount;
            amount = (dmg) - Math.Max((stats.Armor - pen), 0);
            amount = Math.Max(0, amount);
            return amount;
        }

        /// <summary>
        /// Action the creature will do upon being attacked
        /// </summary>
        private void ReceiveDamage(Creature interacter)
        {
            float damage = ComputeDamage(interacter.stats.Damage, interacter.stats.Perforation);
            stats.CurrHealth -= damage;

            Tracker.Instance.Track(new CreatureReceiveDamage(world.tick, ID, speciesName, interacter.ID, damage, DamageType.Attack, stats.CurrHealth));

            if (causeOfDeath == CauseOfDeath.NONE && stats.CurrHealth <= 0)
            {
                causeOfDeath = CauseOfDeath.Attack;
                killingBlow = damage;
                killerID = interacter.ID;  // TODO: -1 o esto?
            }

#if DEBUG
            Console.WriteLine(speciesName + " " + ID + " TAKES " + damage + " DMG (" + stats.CurrHealth + " HP LEFT)");
#endif

            // If the pack is aggressive enought they will fight, else nothing happens.
            Vector3Int enemyPos; Enemy(out _, out enemyPos);
            List<Creature> pack = CombatPack();
            if (ShouldPackFight(pack, PositionDanger(enemyPos.x, enemyPos.y)))
                foreach (Creature fighter in pack)
                {
                    fighter.TargetEnemy(interacter.ID, new Vector3Int(interacter.x, interacter.y, (int)interacter.creatureLayer));
                }
        }

        /// <summary>
        /// Action the creature will do if it can reflect damage back to the attacker
        /// </summary>
        private void RetalliateDamage(Creature interacter)
        {
            interacter.stats.CurrHealth -= stats.Counter;   // TODO: Ver si esto es danio bueno

            Tracker.Instance.Track(new CreatureReceiveDamage(interacter.world.tick, interacter.ID, interacter.speciesName, ID, stats.Counter, DamageType.Retalliation, interacter.stats.CurrHealth));

            if (interacter.causeOfDeath == CauseOfDeath.NONE && interacter.stats.CurrHealth <= 0)
            {
                interacter.causeOfDeath = CauseOfDeath.Retalliation;
                interacter.killingBlow = stats.Counter;
                interacter.killerID = ID;
            }
#if DEBUG
            Console.WriteLine(speciesName + " " + ID + " RETURNS " + stats.Counter + " DMG (" + interacter.stats.CurrHealth + " HP LEFT ON " + interacter.ID + ")");
#endif
        }

        /// <summary>
        /// Action the creature will do when becoming poisoned
        /// </summary>
        private void Poison(Creature interacter)
        {
            if (interacter.stats.Perforation >= stats.Armor)    // Venoms stack, no refreshing
                AddStatus(new Poison((int)(interacter.stats.Venom), interacter.stats.Venom * 0.25f, interacter.ID)); // TODO: Numero magico
        }

        /// <summary>
        /// If a female receive this interaction, check if she want to mate
        /// and send a interaction to the male
        /// </summary>
        /// <param name="interacter"> The creature that sends the interaction </param>
        private void OnMate(Creature interacter)
        {
            if (matingCreature != -1 && matingCreature != 0)
                return;
            if (wantMate && stats.Gender == Gender.Female)
            {
                wantMate = false;
                mating = true;
                matingCreature = interacter.ID;
                interacter.ReceiveInteraction(this, Interactions.mate);
            }
            else if (stats.Gender == Gender.Male)
            {
                mating = true;
                matingCreature = interacter.ID;
            }
        }

        /// <summary>
        /// If the creature was mating and has stopped, tell the mating creature to stop as well
        /// </summary>
        /// <param name="interacter"> Creature who has sent the interaction </param>
        private void StopMating(Creature interacter)
        {
            matingCreature = -1;
            mating = false;
        }

        // TODO: For the love of god please comment
        public int timeToBeInHeat { get; set; }
        public bool mating { get; set; }
        public bool wantMate { get; set; }
        public int matingCreature { get; set; }
        #endregion

        #region Creature Information

        // Stats related information
        public bool IsCarnivorous() { return stats.Diet == Diet.Carnivore; }
        public bool IsHerbivorous() { return stats.Diet == Diet.Herbivore; }

        /// <summary>
        /// Check if the creature is hunger (need to eat)
        /// </summary>
        /// <returns> True if the creature is hunger </returns>
        public bool IsHungry()
        {
            return stats.CurrEnergy <= stats.hungerThreshold * stats.MaxEnergy;
        }

        /// <summary>
        /// Check if the creature is very hunger (need to eat)
        /// </summary>
        /// <returns> True if the creature is very hunger </returns>
        public bool IsVeryHungry()
        {
            return stats.CurrEnergy <= stats.veryHungerThreshold * stats.MaxEnergy;
        }

        /// <summary>
        /// Check if the creature is thirsty (need to drink)
        /// </summary>
        /// <returns> True if the creature is thirsty </returns>
        public bool IsThirsty()
        {
            return stats.CurrHydration <= stats.thirstyThreshold * stats.MaxHydration;
        }

        /// <summary>
        /// Check if the creature is very thirsty (need to drink)
        /// </summary>
        /// <returns> True if the creature is very thirsty </returns>
        public bool IsVeryThirsty()
        {
            return stats.CurrHydration <= stats.veryThirstyThreshold * stats.MaxHydration;
        }

        /// <summary>
        /// Check if the creature is tired (need to sleep)
        /// </summary>
        /// <returns> True if the creature is tired </returns>
        public bool IsTired()
        {
            return stats.CurrRest <= stats.tiredThreshold * stats.MaxRest;
        }

        /// <summary>
        /// Check if the creature is exhausted (need to sleep)
        /// </summary>
        /// <returns> True if the creature is exhausted </returns>
        public bool IsExhausted()
        {
            return stats.CurrRest <= stats.exhaustThreshold * stats.MaxRest;
        }
        #endregion

        #region Mind
        // Memory related information
        public Mind mind { get; private set; }

        /// <summary>
        /// Check if the eating objective is not null
        /// </summary>
        /// <returns>True if the creature knows where to eat </returns>
        public bool HasEatingObjective()
        {
            return Plant() || Corpse();
        }

        /// <summary>
        /// Gives the creature an enemy to target in combat related activities.
        /// </summary>
        /// <param name="creature">Creature to consider an enemy</param>
        public void TargetEnemy(int creature, Vector3Int pos) { mind.TargetEnemy(creature, pos); }
        /// <summary>
        /// Returns true if this creature or an ally in sight has been attacked;
        /// </summary>
        public bool HasBeenAttacked() { return Enemy(); }

        /// <summary>
        /// Returns the danger level of the tile in the map on which the creature is. Danger is calculated based on Intimidation.
        /// </summary>
        public float PositionDanger(int x, int y) { return mind.PositionDanger(x, y); }
        /// <summary>
        /// Creatres an experience for the creature in the current tile that it is in.
        /// If its positive, it is a good experience, if it is negative, a bad one.
        /// </summary>
        public void CreateDanger() { mind.CreateDanger(); }
        public void CreateSafety() { mind.CreateSafety(); }
        /// <summary>
        /// Saves in memory a safe drinking spot and updates the danger levels around it.
        /// </summary>
        public void SafeWaterSource() { mind.SafeWaterSource(); }
        /// <summary>
        /// Saves in memory a safe plant to eat and updates the danger levels around it.
        /// </summary>
        public void SafeEdiblePlant() { mind.SafeEdiblePlant(); }

        /// <summary>
        /// Gets the information of the enemy the creature wants engage in combat
        /// </summary>
        /// <param name="id"> The enemy ID </param>
        /// <param name="position"> The enemy position </param>
        /// <returns> False if it has no enemy, true otherwise </returns>
        public bool Enemy(out int id, out Vector3Int position) { return mind.Enemy(out id, out position); }
        public bool Enemy() { return Enemy(out _, out _); }
        /// <summary>
        /// Gets the information of the nearest dangerous creature
        /// </summary>
        /// <param name="id"> The menace ID </param>
        /// <param name="position"> The menace position </param>
        /// <returns> False if it has no menace, true otherwise </returns>
        public bool Menace(out int id, out Vector3Int position) { return mind.Menace(out id, out position); }
        public bool Menace() { return Menace(out _, out _); }
        /// <summary>
        /// Gets the information of the closest parent to the creature
        /// </summary>
        /// <param name="id"> The parent ID </param>
        /// <param name="position"> The latest parent position that the creature remembers </param>
        /// <returns> False if it has no parent, true otherwise </returns>
        public bool Parent(out int id, out Vector3Int position) { return mind.Parent(out id, out position); }
        public bool Parent() { return Parent(out _, out _); }
        /// <summary>
        /// Gets the information of the closest prey the creature wants engage in combat.
        /// If the creature is herbivorous, it always returns non valid information.
        /// </summary>
        /// <param name="id"> The prey ID </param>
        /// <param name="position"> The prey position </param>
        /// <returns> False if it has no prey, true otherwise </returns>
        public bool Prey(out int id, out Vector3Int position) { return mind.Prey(out id, out position); }
        public bool Prey() { return Prey(out _, out _); }
        /// <summary>
        /// Gets the information of the closest ally to the creature
        /// </summary>
        /// <param name="id"> The ally ID </param>
        /// <param name="position"> The ally position </param>
        /// <returns> False if it has no ally, true otherwise </returns>
        public bool Ally(out int id, out Vector3Int position) { return mind.Ally(out id, out position); }
        public bool Ally() { return Ally(out _, out _); }
        /// <summary>
        /// Gets the information of the closest possible mate the creature has
        /// </summary>
        /// <param name="id"> The mate ID </param>
        /// <param name="position"> The mate position </param>
        /// <returns> False if it has no mate, true otherwise </returns>
        public bool Mate(out int id, out Vector3Int position) { return mind.Mate(out id, out position); }
        public bool Mate() { return Mate(out _, out _); }
        /// <summary>
        /// Gets the information of the closest corpse to the creature
        /// If the creature is herbivorous, it always returns non valid information.
        /// </summary>
        /// <param name="id"> The corpse ID </param>
        /// <param name="position"> The corpse position </param>
        /// <returns> False if it has no corpse, true otherwise </returns>
        public bool Corpse(out int id, out Vector3Int position) { return mind.Corpse(out id, out position); }
        public bool Corpse() { return Corpse(out _, out _); }
        /// <summary>
        /// Gets the information of the closest edible plant to the creature
        /// If the creature is carnivorous, it always returns non valid information.
        /// </summary>
        /// <param name="id"> The plant ID </param>
        /// <param name="position"> The plant position </param>
        /// <returns> False if it has no plant, true otherwise </returns>
        public bool Plant(out int id, out Vector3Int position) { return mind.Plant(out id, out position); }
        public bool Plant() { return Plant(out _, out _); }
        /// <summary>
        /// Gets the position of the most valid water position to the creature
        /// </summary>
        /// <returns> Null if the creature does not have or does not remember any water spot </returns>
        public Vector2Int WaterPosition() { return mind.WaterPosition(); }
        /// <summary>
        /// Gets the position of the most valid safe position to the creature
        /// </summary>
        /// <returns> Null if the creature does not have or does not remember any safe spot </returns>
        public Vector2Int SafePosition() { return mind.SafePosition(); }
        /// <summary>
        /// Gets the position of the most valid safe temperature position to the creature
        /// </summary>
        /// <returns> Null if the creature does not have or does not remember any safe temperature spot </returns>
        public Vector2Int SafeTemperaturePosition() { return mind.SafeTemperaturePosition(); }
        /// <summary>
        /// Gets a new position that the creature has not explored yet or that it does not remember that 
        /// it has explored it before
        /// </summary>
        public Vector2Int NewPosition() { return mind.NewPosition(); }

        /// <summary>
        /// 
        /// </summary>
        public int NewExploreRegion() { return mind.NewExplorePosition(); }
        #endregion

        #region World Info, Movement and Paths

        // World tile position
        public int x { get; private set; }
        public int y { get; private set; }
        // World in which the creature resides
        public World world { get; private set; }
        public enum HeightLayer { Ground, Tree = 1, Air = 2 };

        public HeightLayer creatureLayer;

        Vector3 finalPos;

        Vector3[] path;
        int pathIterator;
        double halfMaxMobility;

        public bool cornered { get; set; }  // This determines if the creature cannot flee fruther and must fight back

        /// <summary>
        /// Moves a creature a specified amount
        /// </summary>
        public void Move(int x, int y, HeightLayer z = HeightLayer.Ground)
        {
            world.entityMap[this.x, this.y].Remove(this);
            this.x += x;
            this.y += y;
            world.entityMap[this.x, this.y].Add(this);
            Vector2 pos = new Vector2();
            pos.X = this.x; pos.Y = this.y;
            world.pathPos.Add(pos);
            creatureLayer = z;
            if (!world.IsTree(x, y) && z == HeightLayer.Tree)
                creatureLayer = HeightLayer.Ground;
        }

        /// <summary>
        /// Places a creature in the designated coordinates
        /// </summary>
        public void Place(int x, int y, HeightLayer z = HeightLayer.Ground)
        {
            world.entityMap[this.x, this.y].Remove(this);
            this.x = x;
            this.y = y;
            world.entityMap[this.x, this.y].Add(this);
            Vector2 pos = new Vector2();
            pos.X = this.x; pos.Y = this.y;
            world.pathPos.Add(pos);
            creatureLayer = z;
            if (!world.IsTree(x, y) && z == HeightLayer.Tree)
                creatureLayer = HeightLayer.Ground;
        }

        /// <summary>
        /// Calculate the distance between the creature and the given entity
        /// </summary>
        /// <returns> Distance between creature and entity. intMaxValue if entity is null </returns>
        public int DistanceToObjective(IEntity entity)
        {
            if (entity == null) return int.MaxValue;
            return DistanceToObjective(entity.x, entity.y);
        }

        /// <summary>
        /// Calculate the distance between the creature and the given pos
        /// </summary>
        /// <returns> Distance between creature and pos. intMaxValue if out of the map </returns>
        public int DistanceToObjective(Vector3Int pos)
        {
            if (pos == null) return int.MaxValue;
            return DistanceToObjective(pos.x, pos.y);
        }
        public int DistanceToObjective(int ox, int oy)
        {
            if (!world.CheckBounds(ox, oy)) return int.MaxValue;
            return Math.Max(Math.Abs(ox - x), Math.Abs(oy - y));
        }
        public int DistanceToObjective(Vector2Int pos)
        {
            if (pos == null) return int.MaxValue;
            return DistanceToObjective(pos.x, pos.y);
        }

        /// <summary>
        /// Returns minimal path length for arboreal movement being more efficient than ground.
        /// </summary>
        public int GetTreeThreshold(double treeDensity)
        {
            double a = 2 * (int)HeightLayer.Tree * (treeDensity * (1 - Tree.movementPenalty) - stats.GroundSpeed / halfMaxMobility);
            double b = treeDensity * (stats.GroundSpeed / halfMaxMobility + Tree.movementPenalty - stats.ArborealSpeed / halfMaxMobility - 1);
            return (int)Math.Floor((a / b) + 0.5);
        }

        /// <summary>
        /// Returns minimal path length for flying movement being more efficient than ground/arboreal.
        /// </summary>
        int GetFlyThreshold(double treeDensity)
        {
            double a = 2 * (int)HeightLayer.Air * (stats.GroundSpeed / halfMaxMobility * (1 - treeDensity) + treeDensity * stats.AerialSpeed / halfMaxMobility);
            double b = -2 * stats.AerialSpeed / halfMaxMobility * (int)HeightLayer.Tree * treeDensity;
            double c = stats.AerialSpeed / halfMaxMobility + stats.GroundSpeed / halfMaxMobility * (treeDensity - 1) - treeDensity * stats.ArborealSpeed / halfMaxMobility;
            return (int)Math.Floor(((a + b) / c) + 0.5);
        }

        /// <summary>
        /// Sets the creature path towards the provided coordinates. Throws exception on unreacheable coordinates. Sets the path iterator to 0.
        /// </summary>
        /// <param name="z">Ending target layer for the creature.</param>
        /// <returns>The cost for moving to the first position on the path.</returns>
        public int SetPath(int x, int y, HeightLayer z = HeightLayer.Ground)
        {
            if (PathEnded())
            {
                if (!world.CanMove(x, y, z)) throw new IndexOutOfRangeException("The creature cannot reach the position (" + x + ", " + y + ", " + z + ")");
                if ((stats.AerialSpeed == -1 && z == HeightLayer.Air) || (stats.ArborealSpeed == -1 && z == HeightLayer.Tree)) throw new IndexOutOfRangeException("The creature cannot reach the position (" + x + ", " + y + ", " + z + ")");

                //If the creature is already in the air, we cannot assert that A* is doable.
                if (creatureLayer == HeightLayer.Air || (creatureLayer != HeightLayer.Air && z == HeightLayer.Air))
                {
                    path = Astar.GetAirPath(new Vector3(this.x, this.y, (int)creatureLayer), new Vector3(x, y, (int)z));
                    pathIterator = 0;
                    return GetNextCostOnPath();
                }

                path = Astar.GetPath(this, world, new Vector3(this.x, this.y, (int)creatureLayer), finalPos = (new Vector3(x, y, (int)z)), out double treeDensity); // A* to the objective
                int thres = GetFlyThreshold(treeDensity);
                if (thres > 0 && path.Length >= thres)
                {
                    path = Astar.GetAirPath(new Vector3(this.x, this.y, (int)creatureLayer), new Vector3(x, y, (int)z));// Straight line to the objective
                }
                pathIterator = 0;
            }
            return GetNextCostOnPath();
        }
        public int SetPath(Vector2Int p, HeightLayer z = HeightLayer.Ground) { return SetPath(p.x, p.y, z); }
        public int SetPath(Vector3Int p) { return SetPath(p.x, p.y, (HeightLayer)p.z); }

        public bool PathEnded()
        {
            return (path == null || pathIterator >= path.Length);
        }
        /// <summary>
        /// Returns the cost for moving to the next position on the path. Does not advance the path iterator.
        /// </summary>
        public int GetNextCostOnPath()
        {
            // TODO Hay que tener en cuenta el path sea de longuitud 0
            if (path == null || path.Length == 0 || pathIterator == path.Length) // TODO: que los estados tengan cuidado de cuando el coste que les dan es -1
                return -1;
            int speed;
            int layer = (int)path[pathIterator].Z;
            //TODO: que es esto?
            switch ((int)path[pathIterator].Z)
            {
                case 0:
                default:
                    speed = stats.GroundSpeed;
                    break;
                case 1:
                    speed = stats.ArborealSpeed;
                    break;
                case 2:
                    speed = stats.AerialSpeed;
                    break;
            }

            int x = (int)path[pathIterator].X, y = (int)path[pathIterator].Y;
            if (layer == (int)HeightLayer.Tree && (world.map[x, y].plant is Tree || world.map[x, y].plant is EdibleTree))
                return (int)(UniverseParametersManager.parameters.baseActionCost * ((chromosome.GetFeatureMax(CreatureFeature.Mobility) - speed * (2 - Tree.movementPenalty)) / halfMaxMobility));
            return (int)(UniverseParametersManager.parameters.baseActionCost * ((chromosome.GetFeatureMax(CreatureFeature.Mobility) - speed) / halfMaxMobility));
        }

        /// <summary>
        /// Returns the next position on the path. Advances the path iterator.
        /// </summary>
        /// <returns>Next position or (-1, -1, -1) on path end.</returns>
        public Vector3 GetNextPosOnPath()
        {
            //if (path == null)
            //    return new Vector3(-2,-1,-1);
            if (pathIterator >= path.Length)
            {
                path = null; return new Vector3(-1, -1, -1);
            }
            if (pathIterator == path.Length - 1 && path[pathIterator] != finalPos)
                SetPath((int)finalPos.X, (int)finalPos.Y, (HeightLayer)finalPos.Z);
            return path[pathIterator++];
        }

        /// <summary>
        /// Checks if the creature can access a given layer
        /// Based on its current layer and stats
        /// </summary>
        public bool CanReach(HeightLayer layer)
        {
            if (creatureLayer < layer)
            {
                if (layer == HeightLayer.Air) return stats.AirReach;
                if (layer == HeightLayer.Tree) return stats.TreeReach;
                return false;
            }
            else return true;
        }

        /// <summary>
        /// Returns the highest layer the creature can move through
        /// </summary>
        public HeightLayer GetHighestLayer()
        {
            if (stats.AirReach) return HeightLayer.Air;
            if (stats.TreeReach) return HeightLayer.Tree;
            return HeightLayer.Ground;
        }
        #endregion

        #region Status Effects

        // List of active status effects
        List<Status.Status> activeStatus;

        // List of status effects to be removed
        List<Status.Status> removedStatus;

        /// <summary>
        /// Adds a status to the list of active statuses
        /// </summary>
        public void AddStatus(Status.Status stat)
        {
            stat.SetOwner(this);
            activeStatus.Add(stat);
            stat.OnApply();
        }

        /// <summary>
        /// Removes a status from the list
        /// </summary>
        /// <param name="expired">If its timer ran out</param>
        public void RemoveStatus(Status.Status stat, bool expired)
        {
            if (activeStatus.Contains(stat))
            {
                removedStatus.Add(stat);
                if (expired) stat.OnExpire();
                else stat.OnRemove();
            }
        }
        #endregion

        #region Tracker
        public void BirthEventTrack()
        {
            Telemetry.Events.CreatureBirth cbEvent = new Telemetry.Events.CreatureBirth(world.tick, ID, speciesName);
            cbEvent.MaxHealth = stats.MaxHealth;
            cbEvent.HealthRegen = stats.HealthRegeneration;
            cbEvent.MaxEnergy = stats.MaxEnergy;
            cbEvent.EnergyExpense = stats.EnergyExpense;
            cbEvent.MaxHydration = stats.MaxHydration;
            cbEvent.HydrationExpense = stats.HydrationExpense;
            cbEvent.MaxRest = stats.MaxRest;
            cbEvent.RestExpense = stats.RestExpense;
            cbEvent.RestRecovery = stats.RestRecovery;
            cbEvent.MaxPerception = stats.MaxPerception;
            cbEvent.MinTemperature = stats.MinTemperature;
            cbEvent.MaxTemperature = stats.MaxTemperature;
            cbEvent.Gender = stats.Gender.ToString();
            cbEvent.Diet = stats.Diet.ToString();
            cbEvent.Damage = stats.Damage;
            cbEvent.Armor = stats.Armor;
            cbEvent.Perforation = stats.Perforation;
            cbEvent.Venom = stats.Venom;
            cbEvent.Counter = stats.Counter;
            cbEvent.GroundSpeed = stats.GroundSpeed;
            cbEvent.AerialSpeed = stats.AerialSpeed;
            cbEvent.AirReach = stats.AirReach;
            cbEvent.ArborealSpeed = stats.ArborealSpeed;
            cbEvent.TreeReach = stats.TreeReach;
            cbEvent.Camouflage = stats.Camouflage;
            cbEvent.Aggressiveness = stats.Aggressiveness;
            cbEvent.Intimidation = stats.Intimidation;
            cbEvent.Size = stats.Size;
            cbEvent.LifeSpan = stats.LifeSpan;
            cbEvent.Limbs = stats.Members;
            cbEvent.Metabolism = stats.Metabolism;
            cbEvent.Hair = stats.Hair;
            cbEvent.Knowledge = stats.Knowledge;
            cbEvent.Paternity = stats.Paternity;
            cbEvent.Upright = stats.Upright;

            Telemetry.Tracker.Instance.Track(cbEvent);
        }
        #endregion
    }
}
