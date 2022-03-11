using System;

namespace EvolutionSimulation.Entities
{
    /// <summary>
    /// Creature stats, affected by the inescapable passage of time
    /// </summary>

    public class CreatureStats
    {
        #region Stats
        public Genetics.Gender Gender
        {
            get { return baseStats.Gender; }
            set { baseStats.Gender = value; }
        }

        //Nutrition related stats
        public Genetics.Diet Diet
        {
            get { return baseStats.Diet; }
            set { baseStats.Diet = value; }
        }

        //From 0 (normal chance of getting poisoned) to 1 (cannot get poisoned)
        public float Scavenger
        {
            get { return baseStats.Scavenger; }
            set { baseStats.Scavenger = value; }
        }

        //Health and damage related stats
        public float MaxHealth
        {
            get { return ModifyStatByAge(baseStats.MaxHealth); }
            set { baseStats.MaxHealth = value; /* If maxHealth changes, currHealth changes the difference */ CurrHealth += MaxHealth - CurrHealth; }
        }
        public float CurrHealth
        {
            get { return baseStats.CurrHealth; }
            set { baseStats.CurrHealth = value; }
        }
        public int Damage
        {
            get { /* Minimum damage is 1 */ return (int)Math.Ceiling(ModifyStatByAge(baseStats.Damage)); }
            set { baseStats.Damage = value; }
        }
        public int Armor
        {
            get { return (int)ModifyStatByAge(baseStats.Armor); }
            set { baseStats.Armor = value; }
        }
        public int Perforation
        {
            get { return (int)ModifyStatByAge(baseStats.Perforation); }
            set { baseStats.Perforation = value; }
        }
        public float Venom
        {
            get { return ModifyStatByAge(baseStats.Venom); }
            set { baseStats.Venom = value; }
        }

        // Reflected damage (spikes)
        public float Counter
        {
            get { return ModifyStatByAge(baseStats.Counter); }
            set { baseStats.Counter = value; }
        }

        //Mobility related stats
        public int AerialSpeed
        {
            get { return baseStats.AerialSpeed; }
            set { baseStats.AerialSpeed = value; }
        }
        public int ArborealSpeed
        {
            get { return baseStats.ArborealSpeed; }
            set { baseStats.ArborealSpeed = value; }
        }
        public int GroundSpeed
        {
            get { return baseStats.GroundSpeed; }
            set { baseStats.GroundSpeed = value; }
        }

        //Reaches
        public bool AirReach
        {
            get { return baseStats.AirReach; }
            set { baseStats.AirReach = value; }
        }
        public bool TreeReach
        {
            get { return baseStats.TreeReach; }
            set { baseStats.TreeReach = value; }
        }

        //Energy related stats
        public float MaxEnergy
        {
            get { return baseStats.MaxEnergy; }
            set { baseStats.MaxEnergy = value; }
        }
        public float CurrEnergy
        {
            get { return baseStats.CurrEnergy; }
            set { baseStats.CurrEnergy = value; }
        }
        public float EnergyExpense
        {
            get { return baseStats.EnergyExpense; }
            set { baseStats.EnergyExpense = value; }
        }

        //Hydration related stats
        public float MaxHydration
        {
            get { return baseStats.MaxHydration; }
            set { baseStats.MaxHydration = value; }
        }
        public float CurrHydration
        {
            get { return baseStats.CurrHydration; }
            set { baseStats.CurrHydration = value; }
        }
        public float HydrationExpense
        {
            get { return baseStats.HydrationExpense; }
            set { baseStats.HydrationExpense = value; }
        }

        //Rest related stats
        public float MaxRest
        {
            get { return baseStats.MaxRest; }
            set { baseStats.MaxRest = value; }
        }
        public float CurrRest
        {
            get { return baseStats.CurrRest; }
            set { baseStats.CurrRest = value; }
        }
        public float RestRecovery
        {
            get { return baseStats.RestRecovery; }
            set { baseStats.RestRecovery = value; }
        }
        public float RestExpense
        {
            get { return baseStats.RestExpense; }
            set { baseStats.RestExpense = value; }
        }

        //Environment related stats
        public int Camouflage
        {
            get { return baseStats.Camouflage; }
            set { baseStats.Camouflage = value; }
        }
        public int Aggressiveness
        {
            get { return (int)ModifyStatByAge(baseStats.Aggressiveness); }
            set { baseStats.Aggressiveness = value; }
        }
        public int Intimidation
        {
            get { return (int)ModifyStatByAge(baseStats.Intimidation); }
            set { baseStats.Intimidation = value; }
        }
        public int Perception
        {
            get { return (int)(baseStats.Perception * CurrentVision); }
            set { baseStats.Perception = value; }
        }
        public float CurrentVision
        {
            get { return baseStats.CurrentVision; }
            set { baseStats.CurrentVision = value; }
        }
        /// <summary>
        /// Auxiliary variable to set the night penalty when it is dark.
        /// </summary>
        public float NightPenalty { get; set; }

        //Physique related stats
        public int Size
        {
            get { return (int)ModifyStatByAge(baseStats.Size); }
            set { baseStats.Size = value; }
        }
        public int LifeSpan
        {
            get { return baseStats.LifeSpan; }
            set { baseStats.LifeSpan = value; }
        }

        public int CurrAge
        {
            get { return baseStats.CurrAge; }
            set { float oldMaxH = MaxHealth; baseStats.CurrAge = value; CurrHealth += MaxHealth - oldMaxH; }
        }
        public int Members //limbs
        {
            get { return baseStats.Members; }
            set { baseStats.Members = value; }
        }
        public int Metabolism
        {
            get { return baseStats.Metabolism; }
            set { baseStats.Metabolism = value; }
        }
        public float MinTemperature
        {
            get { return baseStats.MinTemperature; }
            set { baseStats.MinTemperature = value; }
        }
        public float MaxTemperature
        {
            get { return baseStats.MaxTemperature; }
            set { baseStats.MaxTemperature = value; }
        }
        public float IdealTemperature
        {
            get { return baseStats.IdealTemperature; }
            set { baseStats.IdealTemperature = value; }
        }
        public bool Hair
        {
            get { return baseStats.Hair; }
            set { baseStats.Hair = value; }
        }

        //Behaviour related stats
        public int Knowledge
        {
            get { return baseStats.Knowledge; }
            set { baseStats.Knowledge = value; }
        }
        public int Paternity
        {
            get { return baseStats.Paternity; }
            set { baseStats.Paternity = value; }
        }

        //Multipliers
        public float HealthRegeneration
        {
            get { return baseStats.HealthRegeneration; }
            set { baseStats.HealthRegeneration = value; }
        }
        public float MaxSpeed
        {
            get { return baseStats.MaxSpeed; }
            set { baseStats.MaxSpeed = value; }
        }

        //Reproduction stats
        public int TimeBetweenHeats
        {
            get { return baseStats.TimeBetweenHeats; }
            set { baseStats.TimeBetweenHeats = value; }
        }
        public bool InHeat
        {
            get { return baseStats.InHeat; }
            set { baseStats.InHeat = value; }
        }

        public bool Upright
        {
            get { return baseStats.Upright; }
            set { baseStats.Upright = value; }
        }

        // TODO: es redundante tener los baseStats publicos?
        public CreatureBaseStats GetBaseStats()
        {
            return baseStats;
        }
        CreatureBaseStats baseStats;
        #endregion

        #region Thresholds and Modifiers
        //After which percentage of currRest the creature should sleep with low priority
        public float tiredThreshold = UniverseParametersManager.parameters.tiredThreshold;

        //After which percentage of currRest the creature should sleep with high priority and some stats are dimished
        public float exhaustThreshold = UniverseParametersManager.parameters.exhaustThreshold;

        //After which percentage of currEnergy the creature should eat with low priority
        public float hungerThreshold = UniverseParametersManager.parameters.hungryThreshold;

        //After which percentage of currEnergy the creature should eat with high priority
        public float veryHungerThreshold = UniverseParametersManager.parameters.veryHungryThreshold;

        //After which percentage of currHydration the creature should eat with low priority
        public float thirstyThreshold = UniverseParametersManager.parameters.thirstyThreshold;

        //After which percentage of currHydration the creature should eat with high priority
        public float veryThirstyThreshold = UniverseParametersManager.parameters.veryThirstyThreshold;

        //Starting multiplier of newborns
        float startMultiplier = UniverseParametersManager.parameters.newbornStatMultiplier;

        //After which percentage of lifespan the creature has his stats not dimished by age
        float adulthoodThreshold = UniverseParametersManager.parameters.adulthoodThreshold;
        #endregion

        #region Methods
        public bool IsNewBorn() { return LifeSpan * adulthoodThreshold < CurrAge; }

        /// <summary>
        /// Modifies the given stat based on age
        /// </summary>
        float ModifyStatByAge(float stat)
        {
            return stat * Math.Min(1.0f, (1 - startMultiplier) / (LifeSpan * adulthoodThreshold) * CurrAge + startMultiplier);
        }
        #endregion
    }

    /// <summary>
    /// Struct that has the creature stats.
    /// It can be (de)serialized.
    /// </summary>
    public struct CreatureBaseStats
    {
        public Genetics.Gender Gender;

        //Nutrition related stats
        public Genetics.Diet Diet;

        //From 0 (normal chance of getting poisoned) to 1 (cannot get poisoned)
        public float Scavenger;

        //Health and damage related stats
        public float MaxHealth;
        public float CurrHealth;
        public int Damage;

        public int Armor;
        public int Perforation;

        public float Venom;

        // Reflected damage (spikes)
        public float Counter;

        //Mobility related stats
        public int AerialSpeed;
        public int ArborealSpeed;
        public int GroundSpeed;

        //Reaches
        public bool AirReach;
        public bool TreeReach;

        //Energy related stats
        public float MaxEnergy;
        public float CurrEnergy;
        public float EnergyExpense;

        //Hydration related stats
        public float MaxHydration;
        public float CurrHydration;
        public float HydrationExpense;

        //Rest related stats
        public float MaxRest;
        public float CurrRest;
        public float RestRecovery;
        public float RestExpense;

        //Environment related stats
        public int Camouflage;
        public int Aggressiveness;
        public int Intimidation;
        public int Perception;
        public float CurrentVision;

        //Physique related stats
        public int Size;
        public int LifeSpan;

        public int CurrAge;
        public int Members; //limbs
        public int Metabolism;
        public float MinTemperature;
        public float MaxTemperature;
        public float IdealTemperature;
        public bool Hair;

        //Behaviour related stats
        public int Knowledge;
        public int Paternity;

        //Multipliers
        public float HealthRegeneration;
        public float MaxSpeed;

        //Reproduction stats
        public int TimeBetweenHeats;
        public bool InHeat;

        public bool Upright;
    }
}
