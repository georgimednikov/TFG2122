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
    /// A creature with attributes and behavior
    /// </summary>
    public abstract class Creature : IEntity, IInteractable<Creature>
    {

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
        public void Init(World w, int x, int y, CreatureChromosome chromosome = default(CreatureChromosome), string name = "None")
        {
            world = w;

            if(chromosome == null)
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
            this.x = x;
            this.y = y;
            timeToBeInHeat = stats.TimeBetweenHeats;
            memory = new Memory(this, world);

            ConfigureStateMachine();
            // Attack
            AddInteraction(Interactions.attack, ReceiveDamage);
            if (HasAbility(CreatureFeature.Thorns, 0.65f))
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
            // Bodily functions
            Expend();
            Regen();

            FemaleTick();

            memory.Update();
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
            }
            Clear();
        }

        public void CycleDayNight()
        {
            //If the creature sees normally (1 = 100% of its vision) it was day and now is night.
            if (stats.CurrentVision == 1)
                stats.CurrentVision = stats.NightPenalty; //Instead of 1 now it sees from minNightVision to 1 depending on its feature.

            //Else it was night now it is day again and begins to see normally.
            else
                stats.CurrentVision = 1;

            memory.UpdatePerceptionRadius();
        }

        /// <summary>
        /// When a creature dies calls this method to notify its children
        /// that the creature has died
        /// </summary>
        /// <param name="parent"> The parent of the creature that has died</param>
        // TODO si quien muere no sabe donde esta (no lo recuerda), no hacer nada. hay que modificar memory
        // lo mismo al reves, si muere el padre pero no sabe donde esta la madre, no cambiar la referencia
        public void ParentDead(Creature parent)
        {
            if (parent == father)
            {
                father = null;
            }
            else
            {
                mother = null;
            }
            
            //change reference to follow
            parentToFollow = parent == father ? mother : father;
        }

        /// <summary>
        /// Clear the lists and the objectives
        /// </summary>
        void Clear()
        {
            hasBeenHit = false; // TODO: Reset flags en general

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
            if(stats.CurrEnergy >= (stats.MaxEnergy * UniverseParametersManager.parameters.energyRegenerationThreshold) &&
                stats.CurrRest >= (stats.MaxRest * UniverseParametersManager.parameters.restRegenerationThreshold) &&
                stats.CurrHydration >= (stats.MaxHydration * UniverseParametersManager.parameters.hydrationRegenerationThreshold)) {
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

        // State related attributes

        public bool hasBeenHit;

        public Creature matingCreature;

        /// <summary>
        /// Time in ticks to be in heat (a female)
        /// </summary>
        public int timeToBeInHeat;
        /// <summary>
        /// If a female want to mate, its false if she has needs like
        /// sleep or eat or is mating
        /// </summary>
        public bool wantMate = false;
        /// <summary>
        /// If a creatures is mating
        /// </summary>
        public bool mating;

        /// <summary>
        /// Returns the creature's current state
        /// </summary>
        public string GetState()
        {
            return mfsm.CurrentState.ToString();
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
            IState hide = new Hide(this);
            Fsm escapeFSM = new Fsm(fleeing);
            // Transitions
            ITransition fleeTransition = new FleeTransition(this);
            ITransition hideTranistion = new HideTransition(this);
            escapeFSM.AddTransition(fleeing, hideTranistion, hide);
            escapeFSM.AddTransition(hide, fleeTransition, fleeing);
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

        // Methods to receive and respond to interactions
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

        // Standard reactions to interactions

        /// <summary>
        /// Returns the taken damage
        /// </summary>
        /// <param name="dmg">Incoming damage</param>
        /// <param name="pen">Damage penetratione</param>
        public float ComputeDamage(float dmg, float pen)
        {
            float amount = 0;
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
            //TODO: Cambiar target aunque normalmente sera el mas cercano asi que tampoco es tan urgente
            hasBeenHit = true;
        }

        /// <summary>
        /// Action the creature will do if it can reflect damage back to the attacker
        /// </summary>
        private void RetalliateDamage(Creature interacter)
        {
            interacter.stats.CurrHealth -= stats.Counter;   // TODO: Ver si esto es danio bueno
            Console.WriteLine(speciesName + "(" + x + "," + y + ") devuelve " + stats.Counter + " de daño");
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
                matingCreature = interacter;
                interacter.ReceiveInteraction(this, Interactions.mate);
            }
            else if (stats.Gender == Gender.Male)
            {
                mating = true;
                matingCreature = interacter;
            }
        }

        /// <summary>
        /// If the creature was mating and has stopped, tell the mating creature to stop as well
        /// </summary>
        /// <param name="interacter"> Creature who has sent the interaction </param>
        private void StopMating(Creature interacter)
        {
            if (matingCreature != null)
            {
                matingCreature.ReceiveInteraction(this, Interactions.stopMate);
                matingCreature = null;
                mating = false;
            }
        }
        #endregion

        #region Creature Information

        // Stats related information

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


        #region Memory
        // Memory related information
        public Memory memory;

        /// <summary>
        /// Check if the eating objective is not null
        /// </summary>
        /// <returns>True if the creature knows where to eat </returns>
        public bool HasEatingObjective()
        {
            // Hervibore and not plant objective
            if (stats.Diet == Diet.Herbivore && memory.ClosestFruitPosition() == null)
                return true;
            // Carnivore and not corpse objective
            if (stats.Diet == Diet.Carnivore && memory.ClosestCorpsePosition() == null)
                return true;
            // Omnivore and not plant and corpse objective
            if (stats.Diet == Diet.Omnivore && memory.ClosestCorpsePosition() == null && memory.ClosestFruitPosition() == null)
                return true;

            return false;
        }
        /// <summary>
        /// Check if the creature can eat a rotten corpse as an alternative to a good food source.
        /// </summary>
        /// <returns>True if the creature knows where to eat </returns>
        public bool CanEatRottenCorpse()
        {
            if (stats.Diet == Diet.Herbivore)
                return false;

            return GetClosestRottenCorpsePosition() != null;
        }

        /// <summary>
        /// Returns the danger level of the tile in the map on which the creature is. Danger is calculated based on Intimidation.
        /// </summary>
        public float GetDanger() { return memory.GetPositionDanger(x, y); }
        /// <summary>
        /// Creatres an experience for the creature in the current tile that it is in.
        /// If its positive, it is a good experience, if it is negative, a bad one.
        /// </summary>
        public void CreateExperience(float exp) { memory.CreateExperience(x, y, exp); }
        /// <summary>
        /// Saves in memory a safe drinking spot and updates the danger levels around it.
        /// </summary>
        public void SafeWaterSpotFound(float exp) { memory.SafeWaterSpotFound(exp); }
        /// <summary>
        /// Saves in memory a safe plant to eat and updates the danger levels around it.
        /// </summary>
        public void SafePlantFound(float exp) { memory.SafePlantFound(exp); }
        /// <summary>
        /// Returns the position of the closest safe water source.
        /// </summary>
        public Vector2Int GetSafeWaterPosition() { return memory.SafeWaterPosition(); }
        /// <summary>
        /// Returns the position of the closest safe edible plant.
        /// </summary>
        public Vector2Int GetSafeFruitPosition() { return memory.SafeFruitPosition(); }


        /// <summary>
        /// Returns the position of the closest ally the creature remembers.
        /// </summary>
        public Vector2Int GetClosestAllyPosition() { return memory.ClosestAllyPosition(); }
        /// <summary>
        /// Returns the position of the closest possible mate the creature remembers.
        /// </summary>
        public Vector2Int GetClosestPossibleMatePosition() { return memory.ClosestPossibleMatePosition(); }
        /// <summary>
        /// Returns the position of the closest not allied creature the creature remembers.
        /// </summary>
        public Vector2Int GetClosestCreaturePosition() { return memory.ClosestCreaturePosition(); }
        /// <summary>
        /// Returns the position of the closest rechable creature the creature remembers.
        /// </summary>
        public Vector2Int GetClosestCreatureReachablePosition() { return memory.ClosestCreatureReachablePosition(); }
        /// <summary>
        /// Returns the position of the closest corpse the creature remembers.
        /// </summary>
        public Vector2Int GetClosestCorpsePosition() { return memory.ClosestCorpsePosition(); }
        /// <summary>
        /// Returns the position of the closest corpse the creature remembers.
        /// </summary>
        public Vector2Int GetClosestRottenCorpsePosition() { return memory.ClosestRottenCorpsePosition(); }
        /// <summary>
        /// Returns the position of the closest edible plant the creature remembers.
        /// </summary>
        public Vector2Int GetClosestFruitPosition() { return memory.ClosestFruitPosition(); }
        /// <summary>
        /// Returns the position of the closest mass of water the creature remembers.
        /// </summary>
        public Vector2Int GetClosestWaterPosition() { return memory.ClosestWaterPosition(); }
        /// <summary>
        /// Returns the position of the closest safe place the creature remembers.
        /// </summary>
        public Vector2Int GetClosestSafePlacePosition() { return memory.ClosestSafePlacePosition(); }
        /// <summary>
        /// Returns the position of a random place the creature barely remembers or does not remember at all.
        /// </summary>
        public Vector2Int GetUndiscoveredPlacePosition() { return memory.UndiscoveredPlacePosition(); }

        public Creature GetClosestAlly() { return memory.ClosestAlly(); }
        public Creature GetClosestPossibleMate() { return memory.ClosestPossibleMate(); }
        public Creature GetClosestCreature() { return memory.ClosestCreature(); }
        public Creature GetClosestCreatureReachable() { return memory.ClosestCreatureReachable(); }
        public Corpse GetClosestCorpse() { return memory.ClosestCorpse(); }
        public EdiblePlant GetClosestFruit() { return memory.ClosestFruit(); }
        #endregion

        // Parents
        public Creature father { get; set; }
        public Creature mother { get; set; }
        public Creature parentToFollow { get; set; }
        //Childs
        public List<Creature> childs;
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

            int x1, y1;
            x1 = Math.Abs(x - entity.x);
            y1 = Math.Abs(y - entity.y);

            return (int)Math.Sqrt(Math.Pow(x1, 2) + Math.Pow(y1, 2));
        }

        /// <summary>
        /// Calculate the distance between the creature and the given pos
        /// </summary>
        /// <returns> Distance between creature and pos. intMaxValue if out of the map </returns>
        public int DistanceToObjective(Vector2Int pos)
        {
            if (!world.checkBounds(pos.x, pos.y)) return int.MaxValue;

            int x1, y1;
            x1 = Math.Abs(x - pos.x);
            y1 = Math.Abs(y - pos.y);

            return (int)Math.Sqrt(Math.Pow(x1, 2) + Math.Pow(y1, 2));
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
