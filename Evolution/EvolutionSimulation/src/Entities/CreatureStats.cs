using System;

namespace EvolutionSimulation.Entities
{
    [Serializable]
    public class CreatureStats
    {
        #region Stats
        public Genetics.Gender Gender { get; set; }

        //Nutrition related stats
        public Genetics.Diet Diet { get; set; }

        //From 0 (normal chance of getting poisoned) to 1 (cannot get poisoned)
        public float Scavenger { get; set; } 

        //Health and damage related stats
        float maxHealth;
        public float MaxHealth
        {
            get { return ModifyStatByAge(maxHealth); }
            set { maxHealth = value; /* If maxHealth changes, currHealth changes the difference */ CurrHealth += MaxHealth - CurrHealth; }
        }
        public float CurrHealth { get; set; }

        int damage;
        public int Damage { get { /* Minimum damage is 1 */ return (int)Math.Ceiling(ModifyStatByAge(damage)); } set { damage = value; } }
        
        int armor;
        public int Armor { get { return (int)ModifyStatByAge(armor); } set { armor = value; } }

        int perforation;
        public int Perforation { get { return (int)ModifyStatByAge(perforation); } set { perforation = value; } }

        float venom;
        public float Venom { get { return ModifyStatByAge(venom); } set { venom = value; } }

        // Reflected damage (spikes)
        float counter;
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
        public float MaxEnergy
        {
            get { return maxEnergy; }
            set { maxEnergy = value; }
        }
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
        public int CurrAge
        {
            get { return currAge; }
            set { float oldMaxH = MaxHealth; currAge = value; CurrHealth += MaxHealth - oldMaxH; }
        }
        public int Members { get; set; }//limbs
        public int Metabolism { get; set; }
        public float MinTemperature { get; set; }
        public float MaxTemperature { get; set; }
        public float IdealTemperature { get; set; }
        public bool Hair { get; set; }
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
        public bool IsNewBorn() { return LifeSpan * adulthoodThreshold < currAge; }

        /// <summary>
        /// Modifies the given stat based on age
        /// </summary>
        float ModifyStatByAge(float stat)
        {
            return stat * Math.Min(1.0f, (1 - startMultiplier) / (LifeSpan * adulthoodThreshold) * currAge + startMultiplier);
        }
        #endregion
    }
}
