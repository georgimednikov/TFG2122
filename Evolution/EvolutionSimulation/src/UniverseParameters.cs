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
        public float minLifeSpan = 0.5f; // Minimum years alive
        public float exhaustToSleepRatio = 3; //The creature has to spend sleepToExhaustRatio hours awake per hour asleep
        public float nightPerceptionPenalty = 0.4f; //Percentage of the max Perception lost at night
        public float minMobilityMedium = 0.6f; //When moving through a special medium the slowest speed possible is its mobility * (0.6 - 1.0) depending on proficiency
        public float mobilityPenalty = 0.7f; //The more evolved the animal is to move on a medium different than the ground the worse it moves in relation to the ground
        public float maxSpeed = 1.5f;
        public float hornIntimidationMultiplier = 1.5f;

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

        // States
        public int baseActionCost = 1000;
        public int venomCostMultiplier = 100;
        public float chaseCostMultiplier = 0.4f;
        public float fleeingCostMultiplier = 0.75f;
        public int drinkingCostMultiplier = 10;
        public int eatingCostMultiplier = 10;
        public int sleepingCostMultiplier = 10;

        // Transitions
        public int fleeingTransitionMultiplier = 4;
        public int hidingTransitionMultiplier = 4;
        public float stopEatingTransitionEnergyMultiplier = 1;
        //public float combatTransitionAggressivenessThreshold = 0.5f;
        public float combatTransitionHealthThresholdMultiplier = 50;
        public float escapeTransitionAggressivenessThreshold = 0.5f;
        public float escapeTransitionHealthThresholdMultiplier = 50;
        public float safeTransitionAggressivenessThreshold = 1;

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
