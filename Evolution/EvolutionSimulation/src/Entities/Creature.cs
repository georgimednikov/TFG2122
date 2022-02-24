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
            // TODO: Esto puede estar en el estado Alive, y en el Execute ejecutar
            // el action del estado Padre.
            stats.CurrRest -= stats.RestExpense;
            mfsm.ObtainActionPoints(stats.Metabolism);         
            
            if(stats.Gender == Gender.Female && !stats.IsNewBorn())
            {
                if (timeToBeInHeat == 0)//Can be pregnant
                    stats.InHeat = true;
                //TODO: cuando se quede embarazada, poner timeToBeInHeat a -1
                else if (timeToBeInHeat <= -1)// Pregnant, reset timer
                {
                    timeToBeInHeat = stats.TimeBetweenHeats;
                    stats.InHeat = false;
                }
                else
                    timeToBeInHeat--;
            }

            Perceive();
            foreach (Status.Status s in activeStatus)   // Activates each status effect
                if (s.OnTick()) RemoveStatus(s, true);  // removing it when necessary

            ProcessInput();

            // TomarDecision(); (Asignar Criatura Objetivo) -> Trigger Transicion -> Cambio de estado
            do { mfsm.Evaluate(); } // While the creature can keep performing actions
            while (mfsm.Execute());// Maintains the evaluation - execution action
                                   // Creatura 1 ->  Ataca -> Creatura 2       Desde Creatura1 : Creatura2.Interact(Creatura 1, attack);
                                   //                                          Desde Creatura2 : Creatura1.Interact(Creatura2, attack);
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
            nearestPlant = null;


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
        /// With all the entities that the creature has perceive, 
        /// select the most important (the nearest enemy, ally, food...)
        /// The transitions will use this information to change a different state
        /// </summary>
        void ProcessInput()
        {
            // Find the nearest ally and mate
            if (seenSameSpeciesCreatures.Count != 0)
                nearestAlly = seenSameSpeciesCreatures[0];
            foreach (Creature c in seenSameSpeciesCreatures)
            {
                if (c.stats.InHeat)
                {
                    nearestMate = c;
                    break;
                }
            }

            //Find the nearest plant and corpse
            foreach (StableEntity c in seenEntities)
            {
                if (nearestPlant == null && c as Plant != null)
                {
                    nearestPlant = (Plant)c;
                }
                else if (nearestCorpse == null && c as Corpse != null)
                {
                    nearestCorpse = (Corpse)c;
                }
                if (nearestCorpse != null && nearestPlant != null)
                    break;
                
            }
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
            IState dead = new Dead(this);
            IState alive = new Alive(this);

            IState safe = new Safe(this);
            IState combat = new Combat(this);
            IState escape = new Escape(this);

            //Safe
            IState wander = new Wander(this);
            IState explore = new Explore(this);
            IState goToDrink = new GoToDrink(this);
            IState drink = new Drinking(this);
            IState goToMate = new GoToMate(this);
            IState tryMate = new TryMate(this);
            IState mating = new Mating(this,100);//TODO que 100 lo coja del cromosoma, es el tiempo que tardan en reproducirse
            IState goToEat = new GoToEat(this);
            IState eat = new Eating(this);
            IState goToSafePlace = new GoToSafePlace(this);
            IState sleep = new Sleeping(this);

            //Escape
            IState fleeing = new Fleeing(this);
            IState hide = new Hide(this);

            //Attack
            IState attack = new Attacking(this);
            IState chaseEnemy = new ChaseEnemy(this);

            mfsm = new StatelessFsm(safe);

            // Substates
            mfsm.AddSubstate(alive, safe);
            mfsm.AddSubstate(alive, combat);
            mfsm.AddSubstate(alive, escape);
            
            // Safe substates
            mfsm.AddSubstate(safe, wander);
            mfsm.AddSubstate(safe, explore);
            mfsm.AddSubstate(safe, goToDrink);
            mfsm.AddSubstate(safe, drink);
            mfsm.AddSubstate(safe, goToMate);
            mfsm.AddSubstate(safe, tryMate);
            mfsm.AddSubstate(safe, mating);
            mfsm.AddSubstate(safe, goToEat);
            mfsm.AddSubstate(safe, eat);
            mfsm.AddSubstate(safe, goToSafePlace);
            mfsm.AddSubstate(safe, sleep);

            // Combat substates
            mfsm.AddSubstate(combat, attack);
            mfsm.AddSubstate(combat, chaseEnemy);

            // Escape substates
            mfsm.AddSubstate(escape, fleeing);
            mfsm.AddSubstate(escape, hide);
           

            // Transitions
            ITransition dieTransition = new DieTransition(this);
            ITransition escapeTransition = new EscapeTransition(this);
            ITransition safeTransition = new SafeTransition(this);
            ITransition combatTransition = new CombatTransition(this);

            ITransition thirstyTransition = new ThirstyTransition(this);
            ITransition drinkingTransition = new DrinkingTransition(this);
            ITransition stopDrinkingTransition = new StopDrinkingTransition(this);

            ITransition mateTransition = new MateTransition(this);
            ITransition tryMateTransition = new TryMateTransition(this);
            ITransition matingTransition = new MatingTransition(this);
            ITransition stopMatingTransition = new StopMatingTransition(this);

            ITransition hungerTransition = new HungerTransition(this);
            ITransition eatingTransition = new EatingTransition(this);
            ITransition stopEatingTransition = new StopEatingTransition(this);

            ITransition goToSafePlaceTransition = new GoToSafePlaceTransition(this);
            ITransition sleepySafeTransition = new SleepySafeTransition(this);
            ITransition sleepyTransition = new SleepyTransition(this);
            ITransition wakeTransition = new WakeTransition(this);
            //ITransition mateTransition = new MateTransition(this);

            mfsm.AddTransition(alive, dieTransition, dead);

            mfsm.AddTransition(safe, escapeTransition, escape);
            mfsm.AddTransition(safe, combatTransition, combat);

            mfsm.AddTransition(combat, safeTransition, safe);
            mfsm.AddTransition(combat, escapeTransition, escape);

            mfsm.AddTransition(escape, safeTransition, safe);
            mfsm.AddTransition(escape, combatTransition, combat);
            

            mfsm.AddTransition(wander, thirstyTransition, goToDrink);
            mfsm.AddTransition(goToDrink, drinkingTransition, drink);
            //mfsm.AddTransition(goToDrink, ?, wander);
            mfsm.AddTransition(drink, stopDrinkingTransition, wander);

            mfsm.AddTransition(wander, mateTransition, goToMate);
            //mfsm.AddTransition(goToMate, ?, wander);
            mfsm.AddTransition(goToMate, tryMateTransition, tryMate);
            //mfsm.AddTransition(tryMate, ?, wander);
            mfsm.AddTransition(tryMate, matingTransition, mating);
            mfsm.AddTransition(mating, stopMatingTransition, wander);

            mfsm.AddTransition(wander, hungerTransition, goToEat);
            //mfsm.AddTransition(goToEat, ?, wander);
            mfsm.AddTransition(goToEat, eatingTransition, eat);
            mfsm.AddTransition(eat, stopEatingTransition, wander);

            mfsm.AddTransition(wander, thirstyTransition, goToDrink);
            mfsm.AddTransition(goToDrink, drinkingTransition, drink);

            mfsm.AddTransition(wander, goToSafePlaceTransition, goToSafePlace);
            //mfsm.AddTransition(goToSafePlace, ?, wander);
            mfsm.AddTransition(goToSafePlace, sleepySafeTransition, sleep);
            mfsm.AddTransition(wander, sleepyTransition, sleep);
            mfsm.AddTransition(sleep, wakeTransition, wander);
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
            List<Creature> seenCreatures = world.PerceiveCreatures(this, x, y, perceptionRadius);
            seenEntities = world.PerceiveEntities(this, x, y, perceptionRadius);
            seenCreatures.Sort(new Utils.SortByDistance(this));   // TODO, no hacer new todo el rato
            seenEntities.Sort(new Utils.SortByDistanceSEntities(this));   // TODO, no hacer new todo el rato
            foreach(Creature c in seenCreatures)
            {                
                if (c.speciesName == speciesName || c.progenitorSpeciesName == speciesName || c.speciesName == progenitorSpeciesName)
                    seenSameSpeciesCreatures.Add(c);
                else
                    otherSeenCreatures.Add(c);
            }
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
            if(interacter.stats.Perforation >= stats.Armor)
                AddStatus(new Poison(5 + (int)interacter.stats.Venom, interacter.stats.Venom));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interacter"> male </param>
        private void OnMate(Creature interacter)
        {
            if (!stats.InHeat && !mating) return;  // TODO: consentir o no, si tienes hambre no copular por ejemplo
            mating = true;
            interacter.mating = true;
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
        private StatelessFsm mfsm;


        public Creature nearestEnemy; 
        public Creature nearestAlly; 
        public Creature nearestMate; 
        public Corpse nearestCorpse; 
        public Plant nearestPlant; 
        //water place
        //safe place

        public Action arrivalAction;

        public bool hasBeenHit;

        protected int timeToBeInHeat;
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

    public class CreatureStats
    {
        private float startMultiplier = 0.33f; //Starting multiplier of newborns
        private float adulthoodThreshold = 0.25f; //After which percentage of lifespan the creature has his stats not dimished by age

        public float tiredThreshold = 0.40f; //After which percentage of currRest the creature should sleep with low priority
        //After which percentage of currRest the creature should sleep with high priority and some stats are dimished
        public float exhaustThreshold = 0.20f; 

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

        //Behaviour related stats
        public int Knowledge { get; set; }
        public int Paternity { get; set; }

        //Multipliers
        public float HealthRegeneration { get; set; }
        public float MaxSpeed { get; set; }

        //Reproduction stats
        public int TimeBetweenHeats { get; set; }
        public bool InHeat { get; set; }
    }

}
