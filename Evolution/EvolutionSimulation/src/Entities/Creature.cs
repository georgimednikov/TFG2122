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
            memory = new Memory(this, world);
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
            
            if(stats.Gender == Gender.Female && !stats.IsNewBorn())
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

            memory.Update();
            foreach (Status.Status s in activeStatus)   // Activates each status effect
                if (s.OnTick()) RemoveStatus(s, true);  // removing it when necessary

            do { mfsm.Evaluate(); } // While the creature can keep performing actions
            while (mfsm.Execute());// Maintains the evaluation - execution action

            Clear();
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
            IState mating = new Mating(this,100);//TODO que 100 lo coja del cromosoma, es el tiempo que tardan en reproducirse
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
            escapeFSM.AddTransition(fleeing, hideTranistion , hide);
            escapeFSM.AddTransition(hide, fleeTransition , fleeing);
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
        public void Move(int x, int y, int z = 0)
        {
            this.x += x;
            this.y += y;
            if (world.isTree(x, y))
                this.creatureLayer = (HeightLayer)z;
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

        /// <summary>
        /// Check if the eating objective is not null
        /// </summary>
        /// <returns>True if the creature knows where to eat </returns>
        public bool HasEatingObjective()
        {
            // Hervibore and not plant objective
            if (stats.Diet == Diet.Herbivore && memory.closestFruit == null)
                return true;
            // Carnivore and not corpse objective
            if (stats.Diet == Diet.Carnivore && memory.closestCorpse == null)
                return true;
            // Omnivore and not plant and corpse objective
            if (stats.Diet == Diet.Omnivore && memory.closestCorpse == null && memory.closestFruit == null)
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
        public bool IsHunger()
        {
            return stats.CurrEnergy > stats.hungerThreshold * stats.MaxEnergy;
        }

        /// <summary>
        /// Check if the creature is very hunger (need to eat)
        /// </summary>
        /// <returns> True if the creature is very hunger </returns>
        public bool IsVeryHunger()
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
            //TODO: Cambiar target aunque normalmente sera el mas cercano asi que tampoco es tan urgente
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
            if(interacter.stats.Perforation >= stats.Armor)
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
        /// Returns if an ability is unlocked
        /// </summary>
        /// <param name="unlock">Skill percentage when skill is unlocked</param>
        public bool HasAbility(CreatureFeature feat, float unlock)
        {
            float f = chromosome.GetFeature(feat);
            float mF = chromosome.GetFeatureMax(feat);
            return unlock <= f / mF;
        }

        public enum HeightLayer { Ground, Tree, Air };

        public HeightLayer creatureLayer;

        int treeHeight = 1, flightHeight = 2;
        Vector3[] path;
        int pathIterator;

        public int getTreeThreshold(double treeDensity)
        {
            double a = 2 * treeHeight * (treeDensity * (1 - Tree.movementPenalty) - stats.GroundSpeed / 100f);
            double b = treeDensity * (stats.GroundSpeed / 100f + Tree.movementPenalty - stats.ArborealSpeed / 100f - 1);
            return (int)Math.Floor((a / b) + 0.5);
        }

        int getFlyThreshold(double treeDensity)
        {
            double a = 2 * flightHeight * (stats.GroundSpeed / 100f * (1 - treeDensity) + treeDensity * stats.AerialSpeed / 100f);
            double b = -2 * stats.AerialSpeed / 100f * treeHeight * treeDensity;
            double c = stats.AerialSpeed / 100f + stats.GroundSpeed / 100f * (treeDensity - 1) - treeDensity * stats.ArborealSpeed / 100f;
            return (int)Math.Floor(((a + b) / c) + 0.5);
        }

        public int SetPath(int x, int y, int z = 0)
        {
            if (!world.canMove(x, y, z)) throw new IndexOutOfRangeException("The creature cannot reach the position (" + x + ", " + y + ", " + z + ")");
            double treeDensity = 0;
            path = Astar.GetPath(this, world, new Vector3(this.x, this.y, (int)creatureLayer), new Vector3(x, y, z), out treeDensity); // A*
            int thres = getFlyThreshold(treeDensity);
            if (thres > 0 && path.Length >= thres)
                path = Astar.GetAirPath(new Vector3(this.x, this.y, (int)creatureLayer), new Vector3(x, y, z));// A* pero con todo gratis

            //Console.Clear();
            //for (int i = 0; i < path.Length; ++i)
            //{
            //    //for (int j = 0; j < path[i].X; ++j)
            //    //{
            //    //    if (world.isTree(j, (int)path[i].Y)) Console.BackgroundColor = ConsoleColor.Green;
            //    //    else Console.BackgroundColor = ConsoleColor.Black; 
            //    //    Console.Write(" ");
            //    //}
            //    if (world.isTree((int)path[i].X, (int)path[i].Y)) Console.BackgroundColor = ConsoleColor.Green;
            //    else Console.BackgroundColor = ConsoleColor.Black;
            //    Console.SetCursorPosition((int)path[i].X, (int)path[i].Y);

            //    if (path[i].Z == 0) Console.Write("x");
            //    else Console.Write("a");
            //}
            //Console.BackgroundColor = ConsoleColor.Black;
            //Console.WriteLine();
            //if (path[path.Length - 1].X != 31 || path[path.Length - 1].Y != 31)
            //    Console.WriteLine("No se puede");
            return GetNextCostOnPath();
        }

        //TODO:Remove
        public void MakeFly()
        {
            stats.ArborealSpeed = 199;
            stats.AerialSpeed = -1;
            stats.GroundSpeed = 100;
        }

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
        Memory memory;

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

        public int actionPoints;

        // State machine
        // Diagram: https://drive.google.com/file/d/1NLF4vdYOvJ5TqmnZLtRkrXJXqiRsnfrx/view?usp=sharing
        private Fsm mfsm;


        public Action arrivalAction;

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

        public Creature GetClosestAlly() { return memory.closestAlly; }
        public Creature GetClosestPossibleMate() { return memory.closestPossibleMate; }
        public Creature GetClosestCreature() { return memory.closestCreature; }
        public Creature GetClosestCreatureReachable() { return memory.closestCreatureReachable; }
        public Corpse GetClosestCorpse() { return memory.closestCorpse; }
        public Tuple<int, int> GetClosestFruit() { return memory.closestFruit; }
        public Tuple<int, int> GetClosestWater() { return memory.closestWater; }
    }










    public class CreatureStats
    {
        //TODO valores pls dejad de ser randoms
        private float startMultiplier = 0.33f; //Starting multiplier of newborns
        private float adulthoodThreshold = 0.25f; //After which percentage of lifespan the creature has his stats not dimished by age

        public float tiredThreshold = 0.40f; //After which percentage of currRest the creature should sleep with low priority
        //After which percentage of currRest the creature should sleep with high priority and some stats are dimished
        public float exhaustThreshold = 0.15f;

        public float hungerThreshold = 0.40f; //After which percentage of currEnergy the creature should eat with low priority
        //After which percentage of currEnergy the creature should eat with high priority
        public float veryHungerThreshold = 0.15f;

        public float thirstyThreshold = 0.40f; //After which percentage of currHydration the creature should eat with low priority
        //After which percentage of currHydration the creature should eat with high priority
        public float veryThirstyThreshold = 0.15f;

        /// <summary>
        /// Modifies the given stat based on age
        /// </summary>
        float ModifyStatByAge(float stat)
        {
            return stat * Math.Min(1.0f, (1 - startMultiplier) / (LifeSpan * adulthoodThreshold) * currAge + startMultiplier);
        }

        public bool IsNewBorn() { return LifeSpan * adulthoodThreshold < currAge; }

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
        public int Camouflage { get; set; }// TODO: que dependa de la edad pero al reves
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
        //public float Hair { get; set; }

        //Behaviour related stats
        public int Knowledge { get; set; }
        public int Paternity { get; set; }

        //Multipliers
        public float HealthRegeneration { get; set; }
        public float MaxSpeed { get; set; }

        //Reproduction stats
        public int TimeBetweenHeats { get; set; }
        public bool InHeat { get; set; }

        public bool Upright { get; set; }
        public bool Hair { get; set; }
    }

}
