using System;
using System.Collections.Generic;
using EvolutionSimulation.FSM;
using EvolutionSimulation.FSM.Creature.States;
using EvolutionSimulation.FSM.Creature.Transitions;
using EvolutionSimulation.Genetics;
using System.Numerics;
using EvolutionSimulation.Entities.Status;

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
            seenSameSpeciesCreatures = new List<Creature>();
            otherSeenCreatures = new List<Creature>();
            seenEntities = new List<StableEntity>();
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
            //speciesName = "None";
            SetStats();
            this.x = x;
            this.y = y;
            timeToBeInHeat = stats.TimeBetweenHeats;
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

            Console.WriteLine(mfsm.ExportToDotGraph());
        }

        /// <summary>
        /// Simulation step
        /// </summary>
        public void Tick()
        {
            //toDie.value = (stats.CurrAge++ >= stats.LifeSpan);
            //toSleep.value = (stats.CurrRest <= 0.1 * stats.MaxRest);
            //toWake.value = (stats.CurrRest >= stats.MaxRest);

            stats.CurrRest -= stats.RestExpense;
            mfsm.ObtainActionPoints(stats.Metabolism);

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
                if (stats.CurrEnergy < stats.veryHungerThreshold * stats.MaxEnergy
                    || stats.CurrRest < stats.exhaustThreshold * stats.MaxRest
                    || stats.CurrHydration < stats.veryThirstyThreshold * stats.MaxHydration
                    || mating || !stats.InHeat)
                {
                    wantMate = false;
                }
                else wantMate = true;
            }

            Perceive();
            foreach (Status.Status s in activeStatus)   // Activates each status effect
                if (s.OnTick()) RemoveStatus(s, true);  // removing it when necessary
            ProcessInput();

            do { mfsm.Evaluate(); } // While the creature can keep performing actions
            while (mfsm.Execute());// Maintains the evaluation - execution action

            Clear();
        }

        /// <summary>
        /// Clear the lists and the objectives
        /// </summary>
        void Clear()
        {
            nearestEnemy = null;
            nearestAlly = null;
            nearestMate = null;
            nearestCorpse = null;
            nearestEdiblePlant = null;


            hasBeenHit = false; // TODO: Reset flags en general
            seenSameSpeciesCreatures.Clear();
            otherSeenCreatures.Clear();
            seenEntities.Clear();

            // Clears the statuses marked for deletion
            foreach (Status.Status s in removedStatus)
                activeStatus.Remove(s);
            removedStatus.Clear();
        }



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
            IState safe = new CompoundState("Safe", safeFSM);

            // Drinking
            ITransition thirstyTransition = new ThirstyTransition(this);
            ITransition drinkingTransition = new DrinkingTransition(this);
            ITransition drinkingExploreTransition = new DrinkingExploreTransition(this);
            ITransition stopDrinkingTransition = new StopDrinkingTransition(this);
            ITransition stopGoToDrinkTransition = new StopGoToDrinkTransition(this);
            safeFSM.AddTransition(wander, thirstyTransition, goToDrink);
            safeFSM.AddTransition(wander, drinkingExploreTransition, explore);
            safeFSM.AddTransition(goToDrink, drinkingTransition, drink);
            safeFSM.AddTransition(goToDrink, stopGoToDrinkTransition, wander);
            safeFSM.AddTransition(drink, stopDrinkingTransition, wander);

            // Mating
            ITransition mateTransition = new GoToMateTransition(this);
            ITransition tryMateTransition = new TryMateTransition(this);
            ITransition matingTransition = new MatingTransition(this);
            ITransition stopMatingTransition = new StopMatingTransition(this);
            ITransition stopGoToMateTransition = new StopGoToMateTransition(this);
            ITransition stopTryMateTransition = new StopTryMateTransition(this);
            safeFSM.AddTransition(wander, mateTransition, goToMate);
            safeFSM.AddTransition(goToMate, tryMateTransition, tryMate);
            safeFSM.AddTransition(goToMate, stopGoToMateTransition, wander);
            safeFSM.AddTransition(tryMate, matingTransition, mating);
            safeFSM.AddTransition(tryMate, stopTryMateTransition, wander);
            safeFSM.AddTransition(mating, stopMatingTransition, wander);

            // Eating
            ITransition hungerTransition = new HungerTransition(this);
            ITransition hungerExploreTransition = new HungerExploreTransition(this);
            ITransition eatingTransition = new EatingTransition(this);
            ITransition stopEatingTransition = new StopEatingTransition(this);
            ITransition stopGoToEatTransition = new StopGoToEatTransition(this);
            safeFSM.AddTransition(wander, hungerTransition, goToEat);
            safeFSM.AddTransition(wander, hungerExploreTransition, explore);
            safeFSM.AddTransition(goToEat, stopGoToEatTransition, wander);
            safeFSM.AddTransition(goToEat, eatingTransition, eat);
            safeFSM.AddTransition(eat, stopEatingTransition, wander);

            // Sleeping
            ITransition goToSafePlaceTransition = new GoToSafePlaceTransition(this);
            ITransition sleepySafeTransition = new SleepySafeTransition(this);
            ITransition sleepyTransition = new SleepyTransition(this);
            ITransition wakeTransition = new WakeTransition(this);
            safeFSM.AddTransition(wander, goToSafePlaceTransition, goToSafePlace);
            //safeFSM.AddTransition(goToSafePlace, ?, wander);
            safeFSM.AddTransition(goToSafePlace, sleepySafeTransition, sleep);
            safeFSM.AddTransition(wander, sleepyTransition, sleep);
            safeFSM.AddTransition(sleep, wakeTransition, wander);

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
            //
            IState alive = new CompoundState("Alive", aliveFSM);
            IState dead = new Dead(this);
            //
            mfsm = new Fsm(alive);
            // Transitions
            ITransition dieTransition = new DieTransition(this);
            mfsm.AddTransition(alive, dieTransition, dead);
        }

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
            List<Creature> seenCreatures = world.PerceiveCreatures(this, x, y, perceptionRadius);
            seenEntities = world.PerceiveEntities(this, x, y, perceptionRadius);
            seenCreatures.Sort(new Utils.SortByDistance(this));   // TODO, no hacer new todo el rato
            seenEntities.Sort(new Utils.SortByDistanceSEntities(this));   // TODO, no hacer new todo el rato
            foreach (Creature c in seenCreatures)
            {
                if (c.speciesName == speciesName || c.progenitorSpeciesName == speciesName || c.speciesName == progenitorSpeciesName)
                    seenSameSpeciesCreatures.Add(c);
                else
                    otherSeenCreatures.Add(c);
            }
        }

        /// <summary>
        /// With all the entities that the creature has perceive, 
        /// select the most important (the nearest enemy, ally, food...)
        /// The transitions will use this information to change a different state
        /// </summary>
        void ProcessInput()
        {
            // Nearest ally
            if (seenSameSpeciesCreatures.Count != 0)
                nearestAlly = seenSameSpeciesCreatures[0];
            // Find the nearest mate
            foreach (Creature c in seenSameSpeciesCreatures)
            {
                if (stats.Gender == Gender.Male && c.wantMate)
                {
                    nearestMate = c;
                    break;
                }
                else if (stats.Gender == Gender.Female && c.stats.Gender == Gender.Male)
                {
                    nearestMate = c;
                    break;
                }
            }

            if (otherSeenCreatures.Count != 0)
                nearestEnemy = otherSeenCreatures[0];

            //Find the nearest edible plant and corpse
            foreach (StableEntity c in seenEntities)
            {
                if (nearestEdiblePlant == null && c as EdiblePlant != null)
                {
                    nearestEdiblePlant = (EdiblePlant)c;
                    //Check if the plant has not been eaten
                    if (nearestEdiblePlant.eaten) nearestEdiblePlant = null;
                }
                else if (nearestCorpse == null && c as Corpse != null)
                {
                    nearestCorpse = (Corpse)c;
                }
                if (nearestCorpse != null && nearestEdiblePlant != null)
                    break;

            }
        }

        /// <summary>
        /// Executes every response that this creature has to an interaction with other creature
        /// </summary>
        public void ReceiveInteraction(Creature interacter, Interactions type)
        {
            if (InteractionsDict.ContainsKey(type))
                foreach (Action<Creature> response in InteractionsDict[type])
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


        /// <summary>
        /// Returns the taken damage
        /// </summary>
        /// <param name="dmg">Incoming damage</param>
        /// <param name="pen">Damage penetratione</param>
        public float ComputeDamage(float dmg, float pen)
        {
            float amount = 0;
            amount = (dmg) - (stats.Armor - pen);
            amount = Math.Max(0, amount);
            amount = Math.Min(amount, stats.CurrHealth);
            return amount;
        }


        /// <summary>
        /// Check if the eating objective is not null
        /// </summary>
        /// <returns>True if the creature knows where to eat </returns>
        public bool HasEatingObjective()
        {
            // Hervibore and not plant objective
            if (stats.Diet == Diet.Herbivore && nearestEdiblePlant == null)
                return true;
            // Carnivore and not corpse objective
            if (stats.Diet == Diet.Carnivore && nearestCorpse == null)
                return true;
            // Omnivore and not plant and corpse objective
            if (stats.Diet == Diet.Omnivore && nearestCorpse == null && nearestEdiblePlant == null)
                return true;

            return false;
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
        /// Check if the creature is hunger (need to eat)
        /// </summary>
        /// <returns> True if the creature is hunger </returns>
        public bool IsHungry()
        {
            return stats.CurrEnergy > stats.hungerThreshold * stats.MaxEnergy;
        }

        /// <summary>
        /// Check if the creature is very hunger (need to eat)
        /// </summary>
        /// <returns> True if the creature is very hunger </returns>
        public bool IsVeryHungry()
        {
            return stats.CurrEnergy > stats.veryHungerThreshold * stats.MaxEnergy;
        }


        /// <summary>
        /// Check if the creature is thirsty (need to drink)
        /// </summary>
        /// <returns> True if the creature is thirsty </returns>
        public bool IsThirsty()
        {
            return stats.CurrHydration > stats.thirstyThreshold * stats.MaxHydration;
        }

        /// <summary>
        /// Check if the creature is very thirsty (need to drink)
        /// </summary>
        /// <returns> True if the creature is very thirsty </returns>
        public bool IsVeryThirsty()
        {
            return stats.CurrHydration > stats.veryThirstyThreshold * stats.MaxHydration;
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
        /// Action the creature will do upon being attacked
        /// </summary>
        private void ReceiveDamage(Creature interacter)
        {
            stats.CurrHealth -= ComputeDamage(interacter.stats.Damage, interacter.stats.Perforation);

            nearestEnemy = interacter;
            //objectivePos = new Vector2(interacter.x, interacter.y);
            hasBeenHit = true;
        }

        /// <summary>
        /// Action the creature will do if it can reflect damage back to the attacker
        /// </summary>
        private void RetalliateDamage(Creature interacter)
        {
            interacter.stats.CurrHealth -= stats.Counter;   // TODO: Ver si esto es danio bueno
            Console.WriteLine("Criatura de " + x + ", " + y + " devuelve " + stats.Counter + " de daño!");
        }

        /// <summary>
        /// Action the creature will do when becoming poisoned
        /// </summary>
        private void Poison(Creature interacter)
        {
            if (interacter.stats.Perforation >= stats.Armor)
                AddStatus(new Poison(5 + (int)interacter.stats.Venom, interacter.stats.Venom));
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

        public enum HeightLayer { Ground, Tree = 1, Air = 2 };

        public HeightLayer creatureLayer;

        Vector3[] path;
        int pathIterator;

        /// <summary>
        /// Returns minimal path length for arboreal movement being more efficient than ground.
        /// </summary>
        public int getTreeThreshold(double treeDensity)
        {
            double a = 2 * (int)HeightLayer.Tree * (treeDensity * (1 - Tree.movementPenalty) - stats.GroundSpeed / 100f);
            double b = treeDensity * (stats.GroundSpeed / 100f + Tree.movementPenalty - stats.ArborealSpeed / 100f - 1);
            return (int)Math.Floor((a / b) + 0.5);
        }

        /// <summary>
        /// Returns minimal path length for flying movement being more efficient than ground/arboreal.
        /// </summary>
        int getFlyThreshold(double treeDensity)
        {
            double a = 2 * (int)HeightLayer.Air * (stats.GroundSpeed / 100f * (1 - treeDensity) + treeDensity * stats.AerialSpeed / 100f);
            double b = -2 * stats.AerialSpeed / 100f * (int)HeightLayer.Tree * treeDensity;
            double c = stats.AerialSpeed / 100f + stats.GroundSpeed / 100f * (treeDensity - 1) - treeDensity * stats.ArborealSpeed / 100f;
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
            int thres = getFlyThreshold(treeDensity);
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
                return (int)(1000 * ((200f - speed * (2 - Tree.movementPenalty)) / 100f));
            return (int)(1000 * ((200f - speed) / 100f));
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

        /// <summary>
        /// Sets the stats of the creature.
        /// </summary>
        abstract public void SetStats();

        #region Status Effects
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

        #region Attributes
        // World tile position
        public int x { get; private set; }
        public int y { get; private set; }
        // World in which the creature resides
        public World world { get; private set; }

        // Genetic
        public string speciesName;
        public string progenitorSpeciesName;
        public CreatureChromosome chromosome { get; private set; }
        public CreatureStats stats { get; private set; }

        // List of creatures seen at this moment by this creature
        public List<Creature> seenSameSpeciesCreatures { get; private set; }
        public List<Creature> otherSeenCreatures { get; private set; }
        // List of entities seen at this moment by this creature
        public List<StableEntity> seenEntities { get; private set; }

        public int actionPoints;

        // State machine
        // Diagram: https://drive.google.com/file/d/1NLF4vdYOvJ5TqmnZLtRkrXJXqiRsnfrx/view?usp=sharing
        private Fsm mfsm;


        public Creature nearestEnemy;
        public Creature nearestAlly;
        public Creature nearestMate;
        public Creature matingCreature;
        public Corpse nearestCorpse;
        public EdiblePlant nearestEdiblePlant;
        //water place
        //safe place

        public Action arrivalAction;

        public bool hasBeenHit;

        /// <summary>
        /// Time in ticks to be in heat (a female)
        /// </summary>
        public int timeToBeInHeat;
        /// <summary>
        /// If a female want to mate, its false if she has needs like
        /// sleep or eat or is mating
        /// </summary>
        //TODO igual si estas en el estado mating y te atacan o muere la criatura con la que estas relacionandote esto hay que ponerlo a false
        public bool wantMate = false;
        /// <summary>
        /// If a creatures is mating
        /// </summary>
        public bool mating;

        // Interactions that the creature can react to. Keys are the Interaction type
        // and values are the actions that the creature performs when something interacts with it.
        Dictionary<Interactions, List<Action<Creature>>> InteractionsDict;

        // List of active status effects
        List<Status.Status> activeStatus;

        // List of status effects to be removed
        List<Status.Status> removedStatus;
        #endregion
    }
}
