using System;
using System.Collections.Generic;
using EvolutionSimulation.FSM;
using EvolutionSimulation.FSM.Creature.States;
using EvolutionSimulation.FSM.Creature.Transitions;
using EvolutionSimulation.Genetics;
using EvolutionSimulation.Entities.Status;
using System.Numerics;

namespace EvolutionSimulation.Entities
{
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
            //TODO: Los parametros de abajo no se donde ponerlos xd
            //speciesName = "None";
            SetStats();
            mind = new Mind(this, fatherID, motherID);
            this.x = x;
            this.y = y;
            timeToBeInHeat = stats.TimeBetweenHeats;

            ConfigureStateMachine();
            // Attack
            AddInteraction(Interactions.attack, ReceiveDamage);
            if (HasAbility(CreatureFeature.Thorns, CreatureChromosome.AbilityUnlock[CreatureFeature.Thorns]))
                AddInteraction(Interactions.attack, RetalliateDamage);

            // Poison
            AddInteraction(Interactions.poison, Poison);

            // Mate
            AddInteraction(Interactions.mate, OnMate);
            AddInteraction(Interactions.stopMate, StopMating);

            //Console.WriteLine(mfsm.ExportToDotGraph());
        }

        /// <summary>
        /// Simulation step
        /// </summary>
        public void Tick()
        {
            Expend();
            Regen();
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
                Console.WriteLine(GetStateInfo());
            }

            Clear();
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
            stats.CurrHydration -= stats.HydrationExpense;
            stats.CurrRest -= stats.RestExpense;
            stats.CurrEnergy -= stats.EnergyExpense;
        }

        /// <summary>
        /// Attempts to regenrate the creature's health
        /// Checks first if it can with current energy and rest
        /// And then regenrates a percentage of the creature's max hp
        /// </summary>
        void Regen()
        {
            if (stats.CurrEnergy >= (stats.MaxEnergy * UniverseParametersManager.parameters.energyRegenerationThreshold) &&
                stats.CurrRest >= (stats.MaxRest * UniverseParametersManager.parameters.restRegenerationThreshold) &&
                stats.CurrHydration >= (stats.MaxHydration * UniverseParametersManager.parameters.hydrationRegenerationThreshold))
            {
                stats.CurrHealth += (UniverseParametersManager.parameters.regenerationRate * stats.MaxHealth);  // TODO: Ver si esto esta bien, ingenieria de valores
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
        /// Configures the creature's state machine with the given states.
        /// TODO: We are forcefully cramming these states down the FSM's throat
        /// </summary>
        void ConfigureStateMachine()
        {
            // Alive state configuration
            // States
            // Safe-state configuration
            // States
            IState wander = new Wander(this);
            IState explore = new Explore(this); // TODO: este estado
            IState goToDrink = new GoToDrink(this);
            IState drink = new Drinking(this);
            IState goToMate = new GoToMate(this);
            IState tryMate = new TryMate(this);
            IState mating = new Mating(this, 100);//TODO que 100 lo coja del cromosoma, es el tiempo que tardan en reproducirse
            IState goToEat = new GoToEat(this);
            IState eat = new Eating(this);
            IState goToSafePlace = new GoToSafePlace(this);
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
            safeFSM.AddTransition(mating, stopMatingTransition, wander);
            safeFSM.AddTransition(goToDrink, matingTransition, mating);
            safeFSM.AddTransition(goToEat, matingTransition, mating);
            safeFSM.AddTransition(goToSafePlace, matingTransition, mating);

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
        // TODO: Quitamos la entrada del diccionario si no quedan reacciones?
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
            amount = Math.Min(amount, stats.CurrHealth);
            return amount;
        }

        /// <summary>
        /// Action the creature will do upon being attacked
        /// </summary>
        private void ReceiveDamage(Creature interacter)
        {
            stats.CurrHealth -= ComputeDamage(interacter.stats.Damage, interacter.stats.Perforation);

            //If the creature has enough allies to put up a fight the gank him together.
            List<int> allies = mind.NearbyAllies();
            List<Creature> fighters = new List<Creature>();
            foreach (int ally in allies)
            {
                Creature a = world.GetCreature(ally);
                if (a != null && a.AbleToFight())
                    fighters.Add(a);
            }

            // If the pack is aggressive enought they will fight, else nothing happens.
            Vector2Int enemyPos; Enemy(out _, out enemyPos);
            if (stats.Aggressiveness * (fighters.Count + (AbleToFight() ? 1 : 0)) >= PositionDanger(enemyPos.x, enemyPos.y))
                foreach (Creature fighter in fighters)
                {
                    fighter.TargetEnemy(interacter.ID);
                }
        }

        /// <summary>
        /// Action the creature will do if it can reflect damage back to the attacker
        /// </summary>
        private void RetalliateDamage(Creature interacter)
        {
            interacter.stats.CurrHealth -= stats.Counter;   // TODO: Ver si esto es danio bueno
            Console.WriteLine(speciesName + " RETURNS " + stats.Counter + " DMG");
        }

        /// <summary>
        /// Action the creature will do when becoming poisoned
        /// </summary>
        private void Poison(Creature interacter)
        {
            if (interacter.stats.Perforation >= stats.Armor)    // Venoms stack, no refreshing
                AddStatus(new Poison((int)(interacter.stats.Venom), interacter.stats.Venom * 0.25f));
        }

        /// <summary>
        /// If a female receive this interaction, check if she want to mate
        /// and send a interaction to the male
        /// </summary>
        /// <param name="interacter"> The creature that sends the interaction </param>
        private void OnMate(Creature interacter)
        {
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
            Creature mate = world.GetCreature(matingCreature);
            if (mate != null)
            {
                mate.ReceiveInteraction(this, Interactions.stopMate);
            }
            matingCreature = -1;
            mating = false;
        }


        public  int timeToBeInHeat { get;  set; }
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

        /// <summary>
        /// Returns if an ability is unlocked
        /// </summary>
        /// <param name="unlock">Skill percentage when skill is unlocked</param>
        public bool HasAbility(CreatureFeature feat, float unlock)
        {
            float f = chromosome.GetFeature(feat);
            float mF = chromosome.GetFeatureMax(feat);
            return unlock <= f / mF;
        }
        #endregion

        #region Mind
        // Memory related information
        Mind mind;

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
        public void TargetEnemy(int creature) { mind.TargetEnemy(creature); }
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
        public bool Enemy(out int id, out Vector2Int position) { return mind.Enemy(out id, out position); }
        public bool Enemy() { return Enemy(out _, out _); }
        /// <summary>
        /// Gets the information of the nearest dangerous creature
        /// </summary>
        /// <param name="id"> The menace ID </param>
        /// <param name="position"> The menace position </param>
        /// <returns> False if it has no menace, true otherwise </returns>
        public bool Menace(out int id, out Vector2Int position) { return mind.Menace(out id, out position); }
        public bool Menace() { return Menace(out _, out _); }
        /// <summary>
        /// Gets the information of the closest parent to the creature
        /// </summary>
        /// <param name="id"> The parent ID </param>
        /// <param name="position"> The latest parent position that the creature remembers </param>
        /// <returns> False if it has no parent, true otherwise </returns>
        public bool Parent(out int id, out Vector2Int position) { return mind.Parent(out id, out position); }
        public bool Parent() { return Parent(out _, out _); }
        /// <summary>
        /// Gets the information of the closest prey the creature wants engage in combat
        /// </summary>
        /// <param name="id"> The prey ID </param>
        /// <param name="position"> The prey position </param>
        /// <returns> False if it has no prey, true otherwise </returns>
        public bool Prey(out int id, out Vector2Int position) { return mind.Prey(out id, out position); }
        public bool Prey() { return Prey(out _, out _); }
        /// <summary>
        /// Gets the information of the closest ally to the creature
        /// </summary>
        /// <param name="id"> The ally ID </param>
        /// <param name="position"> The ally position </param>
        /// <returns> False if it has no ally, true otherwise </returns>
        public bool Ally(out int id, out Vector2Int position) { return mind.Ally(out id, out position); }
        public bool Ally() { return Ally(out _, out _); }
        /// <summary>
        /// Gets the information of the closest possible mate the creature has
        /// </summary>
        /// <param name="id"> The mate ID </param>
        /// <param name="position"> The mate position </param>
        /// <returns> False if it has no mate, true otherwise </returns>
        public bool Mate(out int id, out Vector2Int position) { return mind.Mate(out id, out position); }
        public bool Mate() { return Mate(out _, out _); }
        /// <summary>
        /// Gets the information of the closest corpse to the creature
        /// If the creature is herbivorous, it always returns non valid information.
        /// </summary>
        /// <param name="id"> The corpse ID </param>
        /// <param name="position"> The corpse position </param>
        /// <returns> False if it has no corpse, true otherwise </returns>
        public bool Corpse(out int id, out Vector2Int position) { return mind.Corpse(out id, out position); }
        public bool Corpse() { return Corpse(out _, out _); }
        /// <summary>
        /// Gets the information of the closest edible plant to the creature
        /// If the creature is carnivorous, it always returns non valid information.
        /// </summary>
        /// <param name="id"> The plant ID </param>
        /// <param name="position"> The plant position </param>
        /// <returns> False if it has no plant, true otherwise </returns>
        public bool Plant(out int id, out Vector2Int position) { return mind.Plant(out id, out position); }
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
        /// Gets a new position that the creature has not explored yet or that it does not remember that 
        /// it has explored it before
        /// </summary>
        public Vector2Int NewPosition() { return mind.NewPosition(); }
        #endregion

        #region World Info, Movement and Paths

        // World tile position
        public int x { get; private set; }
        public int y { get; private set; }
        // World in which the creature resides
        public World world { get; private set; }
        public enum HeightLayer { Ground, Tree = 1, Air = 2 };

        public HeightLayer creatureLayer;

        Vector3[] path;
        int pathIterator;

        public bool cornered { get; set; }  // This determines if the creature cannot flee fruther and must fight back

        /// <summary>
        /// Moves a creature a specified amount
        /// </summary>
        public void Move(int x, int y, HeightLayer z = HeightLayer.Ground)
        {
            this.x += x;
            this.y += y;
            if (world.isTree(x, y))
                creatureLayer = z;
            else if (creatureLayer != HeightLayer.Air)
                creatureLayer = HeightLayer.Ground;
        }

        /// <summary>
        /// Places a creature in the designated coordinates
        /// </summary>
        public void Place(int x, int y, HeightLayer z = HeightLayer.Ground)
        {
            this.x = x;
            this.y = y;
            if (world.isTree(x, y))
                creatureLayer = z;
            else if (creatureLayer != HeightLayer.Air)
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
        public int DistanceToObjective(Vector2Int pos)
        {
            if (pos == null) return int.MaxValue;
            return DistanceToObjective(pos.x, pos.y);
        }
        public int DistanceToObjective(int ox, int oy)
        {
            if (!world.checkBounds(ox, oy)) return int.MaxValue;
            return Math.Max(Math.Abs(ox - x), Math.Abs(oy - y));
        }

        /// <summary>
        /// Returns minimal path length for arboreal movement being more efficient than ground.
        /// </summary>
        public int GetTreeThreshold(double treeDensity)
        {
            double a = 2 * (int)HeightLayer.Tree * (treeDensity * (1 - Tree.movementPenalty) - stats.GroundSpeed / (chromosome.GetFeatureMax(CreatureFeature.Mobility) / 2));
            double b = treeDensity * (stats.GroundSpeed / (chromosome.GetFeatureMax(CreatureFeature.Mobility) / 2) + Tree.movementPenalty - stats.ArborealSpeed / (chromosome.GetFeatureMax(CreatureFeature.Mobility) / 2) - 1);
            return (int)Math.Floor((a / b) + 0.5);
        }

        /// <summary>
        /// Returns minimal path length for flying movement being more efficient than ground/arboreal.
        /// </summary>
        int GetFlyThreshold(double treeDensity)
        {
            double a = 2 * (int)HeightLayer.Air * (stats.GroundSpeed / (chromosome.GetFeatureMax(CreatureFeature.Mobility) / 2) * (1 - treeDensity) + treeDensity * stats.AerialSpeed / (chromosome.GetFeatureMax(CreatureFeature.Mobility) / 2));
            double b = -2 * stats.AerialSpeed / (chromosome.GetFeatureMax(CreatureFeature.Mobility) / 2) * (int)HeightLayer.Tree * treeDensity;
            double c = stats.AerialSpeed / (chromosome.GetFeatureMax(CreatureFeature.Mobility) / 2) + stats.GroundSpeed / (chromosome.GetFeatureMax(CreatureFeature.Mobility) / 2) * (treeDensity - 1) - treeDensity * stats.ArborealSpeed / (chromosome.GetFeatureMax(CreatureFeature.Mobility) / 2);
            return (int)Math.Floor(((a + b) / c) + 0.5);
        }

        /// <summary>
        /// Sets the creature path towards the provided coordinates. Throws exception on unreacheable coordinates. Sets the path iterator to 0.
        /// </summary>
        /// <param name="z">Ending target layer for the creature.</param>
        /// <returns>The cost for moving to the first position on the path.</returns>
        public int SetPath(int x, int y, HeightLayer z = HeightLayer.Ground)
        {
            if (!world.canMove(x, y, z)) throw new IndexOutOfRangeException("The creature cannot reach the position (" + x + ", " + y + ", " + z + ")");
            if ((stats.AerialSpeed == -1 && z == HeightLayer.Air) || (stats.ArborealSpeed == -1 && z == HeightLayer.Tree)) throw new IndexOutOfRangeException("The creature cannot reach the position (" + x + ", " + y + ", " + z + ")");
            path = Astar.GetPath(this, world, new Vector3(this.x, this.y, (int)creatureLayer), new Vector3(x, y, (int)z), out double treeDensity); // A* to the objective
            int thres = GetFlyThreshold(treeDensity);
            if (thres > 0 && path.Length >= thres)
                path = Astar.GetAirPath(new Vector3(this.x, this.y, (int)creatureLayer), new Vector3(x, y, (int)z));// Straight line to the objective
            pathIterator = 0;
            return GetNextCostOnPath();
        }
        public int SetPath(Vector2Int p, HeightLayer z = HeightLayer.Ground) { return SetPath(p.x, p.y, z); }

        /// <summary>
        /// Returns the cost for moving to the next position on the path. Does not advance the path iterator.
        /// </summary>
        public int GetNextCostOnPath()
        {
            // TODO Hay que tener en cuenta el path sea de longuitud 0
            if (path == null || path.Length == 0 || pathIterator == path.Length) // TODO: que los estados tengan cuidado de cuando el coste que les dan es -1
                return -1;

            int x = (int)path[pathIterator].X, y = (int)path[pathIterator].Y;
            int speed;
            switch ((int)path[pathIterator].Z)
            {
                case 0:
                default:
                    speed = stats.GroundSpeed;
                    break;
                case 1:
                    speed = stats.GroundSpeed;
                    break;
                case 2:
                    speed = stats.GroundSpeed;
                    break;
            }
            if (world.map[x, y].plant is Tree || world.map[x, y].plant is EdibleTree)
                return (int)(UniverseParametersManager.parameters.baseActionCost * ((chromosome.GetFeatureMax(CreatureFeature.Mobility) - speed * (2 - Tree.movementPenalty)) / (chromosome.GetFeatureMax(CreatureFeature.Mobility) / 2)));
            return (int)(UniverseParametersManager.parameters.baseActionCost * ((chromosome.GetFeatureMax(CreatureFeature.Mobility) - speed) / (chromosome.GetFeatureMax(CreatureFeature.Mobility) / 2)));
        }

        /// <summary>
        /// Returns the next position on the path. Advances the path iterator.
        /// </summary>
        /// <returns>Next position or (-1, -1, -1) on path end.</returns>
        public Vector3 GetNextPosOnPath()
        {
            if (pathIterator >= path.Length) { path = null; return new Vector3(-1, -1, -1); }

            return path[pathIterator++];
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
    }
}
