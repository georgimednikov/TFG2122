using System;
using System.IO;
using Newtonsoft.Json;

namespace EvolutionSimulation
{
    public class UniverseParameters
    {
        // Time/World
        public int ticksPerHour = 50;
        public int hoursPerDay = 24;
        public int daysPerYear = 365;
        public float morningStart = 6.5f;
        public float nightStart = 20;

        //Plant
        public int grassHp = 10;
        public int bushHp = 20;
        public int eTreeHp = 50;

        // Creature
        public float abilityUnlockPercentage = 0.4f; //The percentage of an ability that has to be had in order to unlock it
        public int minHealth = 10; //Minimum amount of health
        public int healthGainMultiplier = 2; //Health gained per point of constitution
        public float healthRegeneration = 0.1f; // How much health is regenerated each tick if the creature is healthy
        public int maxLimbs = 10;
        public int minRestExpense = 1; // How much does rest go down every tick
        public int maxRestExpense = 5;
        public int resourceAmount = 100; //Max amount of every resource
        public int minPerception = 5;
        public int maxPerception = 10;
        public float minLifeSpan = 0.5f; // Minimum years alive
        public float exhaustToSleepRatio = 3; //The creature has to spend sleepToExhaustRatio hours awake per hour asleep
        public float hoursTilExhaustion = 28; // Hours until the creature falls over exhausted
        public float perceptionWithoutNightVision = 0.6f; //Percentage of the max Perception lost at night
        public float minPerceptionWithNightVision = 0.7f; //Percentage of the max Perception lost at night
        public float minMobilityMedium = 0.6f; //When moving through a special medium the slowest speed possible is its mobility * (0.6 - 1.0) depending on proficiency
        public float mobilityPenalty = 0.7f; //The more evolved the animal is to move on a medium different than the ground the worse it moves in relation to the ground
        public float maxSpeed = 1.5f;
        public float hornIntimidationMultiplier = 1.5f;
        public float hairTemperatureMultiplier = 0.2f;//The temperature multiplier is the creature has the hair ability
        public float restRegenerationThreshold = 0.7f;  // Threshold at which rest must be for the creature to be able to regenerate health
        public float energyRegenerationThreshold = 0.85f;   // Threshold at which energy must be for the creature to be able to regenerate health
        public float hydrationRegenerationThreshold = 0.85f;    // Threshold at which hysration must be for the creature to be able to regenerate health
        public float regenerationRate = 0.01f;  // Perceentage of maximum health whichthe creature regenerates each tick
        public float hoursTilStarvation = 124f;  // Maximum time until the creature reached 0 energy
        public float thirstToHungerRatio = 3f;  // Rate at wich thisrts increases in relation to hunger
        public float maxTemperatureAggressivenessPercentage = 0.5f; //Max percentage of the creature's aggressiveness that is used as danger in tiles with extreme temperature
        public float maxTemperatureDifference = 0.2f; //How many degrees over the limit influence the damage until it reaches the limit.
        public double minHealthTemperatureDamage = 0.01; //Minimum max health damage for being in extreme temperatures.
        public double maxHealthTemperatureDamage = 0.02; //Maximum max health damage for being in extreme temperatures.
        public float venomDamageMultiplier = 0.25f;

        // CreatureStats
        public float newbornStatMultiplier = 0.33f; //Starting multiplier of newborns
        public float adulthoodThreshold = 0.25f; //After which percentage of lifespan the creature has his stats not dimished by age
        public float tiredThreshold = 0.40f; //After which percentage of currRest the creature should sleep with low priority
        public float exhaustThreshold = 0.15f; //After which percentage of currRest the creature should sleep with high priority and some stats are dimished
        public float hungryThreshold = 0.40f; //After which percentage of currEnergy the creature should eat with low priority
        public float veryHungryThreshold = 0.15f; //After which percentage of currEnergy the creature should eat with high priority
        public float thirstyThreshold = 0.40f; //After which percentage of currHydration the creature should eat with low priority
        public float veryThirstyThreshold = 0.15f; //After which percentage of currHydration the creature should eat with high priority

        // Trees
        public float treeMovementPenalty = 0.7f;

        // Memory
        public int knowledgeTickMultiplier = 500;
        //public float perceptionToRadiusMultiplier = 0.15f;
        public float aggressivenessToRadiusMultiplier = 0.33f;

        // States
        public int baseActionCost = 1000;
        public int venomCostMultiplier = 100;
        public float chaseCostMultiplier = 0.4f;
        public float fleeingCostMultiplier = 0.75f;
        public int drinkingCostMultiplier = 10;
        public int drinkingMultiplier = 5;
        public int eatingCostMultiplier = 10;
        public int sleepingCostMultiplier = 10;
        public float mutationChance = 0.1f;
        public int adjacentLength = 1;
        public float actionPerceptionPercentage = 0.75f;
        public float sleepingExpenseReduction = 0.1f;

        // Transitions
        public int fleeingTransitionMultiplier = 4;
        public int hidingTransitionMultiplier = 4;
        public float stopEatingTransitionEnergyMultiplier = 1;
        public float combatTransitionHealthThresholdMultiplier = 50;
        public float maxMenaceIntimidationMultiplierBasedOnMissingHealth = 2;
        public float safeTransitionAggressivenessThreshold = 1;
        public float experienceMaxAggresivenessMultiplier = 0.2f; //Percentage of the max aggressiveness used to create experiences
        public float safePrefferedOverClosestResourceRatio = 1.25f; //Acceptable max distance of the closest safe resource compared to the closest

        // Corpse
        public float rotStartMultiplier = 0.01f;
        public float corpseNutritionPointsMultiplier = 80f;
    }

    public static class UniverseParametersManager
    {
        public static void ReadJSON()
        {
            string jsonParameters = UserInfo.DataDirectory + "UniverseParameters.json";
            if (!File.Exists(jsonParameters))
                throw new Exception("Cannot find JSON with universe parameters with name: " + UserInfo.DataDirectory + "UniverseParameters.json");
            string file = File.ReadAllText(jsonParameters);
            parameters = JsonConvert.DeserializeObject<UniverseParameters>(file);
            Validator.Validate(parameters);
        }

        public static void SetDefaultParameters()
        {
           parameters = new UniverseParameters();
        }

        public static void WriteDefaultParameters()
        {
            UniverseParameters export = new UniverseParameters();
            string species = JsonConvert.SerializeObject(export, Formatting.Indented);
            File.WriteAllText(UserInfo.DataDirectory + "UniverseParameters.json", species);
        }

        public static UniverseParameters parameters;
    }
}
