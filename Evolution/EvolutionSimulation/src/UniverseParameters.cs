using System;
using System.IO;
using Newtonsoft.Json;

namespace EvolutionSimulation
{
    public class UniverseParameters
    {
        // Time/World
        public int ticksPerHour = 50; // Number of ticks per hour of simulation
        public int hoursPerDay = 24; // Number of hours per day of simulation
        public int daysPerYear = 365; // Number of days per year of simulation
        public float morningStart = 6.5f; // Dawn hour
        public float nightStart = 20; // Nigthfall hour

        //Plant
        public int grassHp = 10; // Grass "health", it represents the amount of times it can be consumed before expiring
        public int bushHp = 20; // Bush "health", it represents the amount of times it can be consumed before expiring
        public int eTreeHp = 50; // EdibleTree "health", it represents the amount of times it can be consumed before expiring
        public float grassNutritionalValue = 2.8f; // Amount of food the grass gives
        public float bushNutritionValue = 6.4f;  // Amount of food bushes give
        public float eTreeNutritionalValue = 13.2f;  // Amount of food edible trees give
        public float grassHoursTillGrowth = 24f; // Hours needed for grass to be regenerated. Default: One day
        public float bushHoursTillGrowth = 120f; // Hours needed for bushes to be regenerated. Default: Five days
        public float eTreeHoursTillGrowth = 360f; // Hours needed for edible trees to be regenerated. Default: Fifteen days

        // Creature
        public float abilityUnlockPercentage = 0.4f; //The percentage of an ability that has to be had in order to unlock it
        public int minHealth = 10; //Minimum amount of health
        public int healthGainMultiplier = 2; //Health gained per point of constitution
        public float healthRegeneration = 0.1f; // How much health is regenerated each tick if the creature is healthy
        public int maxLimbs = 10; // Maximum amount of limbs a creature can have
        public int resourceAmount = 100; //Max amount of every resource
        public int minPerception = 5; // The minimum radius of perception a creature can have
        public int maxPerception = 10; // The maximum radius of perceptiona creature can have
        public float minLifeSpan = 0.5f; // Minimum number of years a creature can live
        public float exhaustToSleepRatio = 3; //The creature has to spend sleepToExhaustRatio hours awake per hour asleep
        public float hoursTilExhaustion = 28; // Hours until the creature falls over exhausted
        public float perceptionWithoutNightVision = 0.6f; //Percentage of the perception lost at night
        public float minPerceptionWithNightVision = 0.7f; //Percentage of the perception lost at night
        public float minMobilityMedium = 0.6f; //When moving through a special medium the slowest speed possible is its mobility * (0.6 - 1.0) depending on proficiency
        public float mobilityPenalty = 0.7f; //The more evolved the animal is to move on a medium different than the ground the worse it moves in relation to the ground
        public float yearsBetweenHeats = 0.1f;  // How many years between each heat period
        public int maxChildNumber = 5; // The maximum number of child that can be conceived in a sexual intercourse
        public int ticksToReproduce = 20; // How many ticks the creatures must be reproducing for to be able to bear offspring
        public float maxSpeed = 1.5f; // Speed multiplier that defines the maximum speed of the creature TODO: no se si es esto
        public float hornIntimidationMultiplier = 1.5f; // Intimidation multiplier if the creature has horns
        public float hairTemperatureMultiplier = 0.2f; // The temperature multiplier if the creature has the hair ability
        public float restRegenerationThreshold = 0.7f;  // Threshold at which rest must be for the creature to be able to regenerate health
        public float energyRegenerationThreshold = 0.85f;   // Threshold at which energy must be for the creature to be able to regenerate health
        public float hydrationRegenerationThreshold = 0.85f;    // Threshold at which hysration must be for the creature to be able to regenerate health
        public float regenerationRate = 0.01f;  // Percentage of maximum health whichthe creature regenerates each tick
        public float hoursTilStarvation = 124f;  // Maximum time until the creature reached 0 energy
        public float thirstToHungerRatio = 3f;  // Rate at wich thisrts increases in relation to hunger
        public float maxTemperatureDifference = 0.2f; //How many degrees over the limit influence the damage until it reaches the limit.
        public double minHealthTemperatureDamage = 0.01; //Minimum max health damage for being in extreme temperatures.
        public double maxHealthTemperatureDamage = 0.02; //Maximum max health damage for being in extreme temperatures.
        public float venomDamageMultiplier = 0.25f; // Venom damage multiplier applied to every type of venom TODO: no se si es esto 
        public float omnivorousNutritionMultiplier = 0.7f; // How effective food sources are for omnivores.

        // CreatureStats
        public float newbornStatMultiplier = 0.33f; //Starting multiplier of newborns
        public float adulthoodThreshold = 0.20f; //After which percentage of lifespan the creature has his stats not dimished by age
        public float tiredThreshold = 0.45f; //After which percentage of currRest the creature should sleep with low priority
        public float exhaustThreshold = 0.12f; //After which percentage of currRest the creature should sleep with high priority and some stats are dimished
        public float hungryThreshold = 0.55f; //After which percentage of currEnergy the creature should eat with low priority
        public float veryHungryThreshold = 0.15f; //After which percentage of currEnergy the creature should eat with high priority
        public float thirstyThreshold = 0.60f; //After which percentage of currHydration the creature should eat with low priority
        public float veryThirstyThreshold = 0.20f; //After which percentage of currHydration the creature should eat with high priority

        // Trees
        public float treeMovementPenalty = 0.7f; // Mobility penalty applied to non-arborean creatures that move through trees

        // Memory
        public int knowledgeTickMultiplier = 500; // Amount of ticks that a creature can remember things like water positions. It depends also on its knowledge.
        public float aggressivenessToRadiusMultiplier = 0.33f; // Multiplier of the radius of perception that a creature can perceive as dangerous
        public int maxResourcesRemembered = 5; // Maximum number of resources that a creature can remeber
        public int maxPositionsRemembered = 10; // Maximum number of positions that a creature can remember

        // States
        public int baseActionCost = 1000; // Base cost of any action
        public int venomCostMultiplier = 100; // Aditional cost of a venomous attack 
        public float chaseCostMultiplier = 0.4f;
        public float fleeingCostMultiplier = 0.75f; // Modifier of the cost of moving when one creature flees from another TODO: hay un numero mágico en flee
        public int drinkingCostMultiplier = 10; // Cost modifier of drinking
        public int drinkingMultiplier = 10; // Modifier of the amount of hidratation that is recovered when drinking
        public int eatingCostMultiplier = 10; // Cost modifier of eating
        public int sleepingCostMultiplier = 10; // Cost modifier of sleeping
        public float mutationChance = 0.1f; // Chance of a mutation in new creatures 
        public int adjacentLength = 1; // Tiles min that a creature will always perceive and will be able to interact 
        public float actionPerceptionPercentage = 0.75f; // Perception modification when a creature is performing an action
        public float sleepingExpenseReduction = 0.1f; // The reduction of every expense (energy, hydration, rest) when a creature is sleeping

        // Transitions
        public int fleeingTransitionMultiplier = 4; // Multiplier to calculate how aggresive is a creature while fleeing
        public float stopEatingTransitionEnergyPercentage = 1; // Percentage of the max energy to stop eating
        public float combatTransitionHealthThresholdMultiplier = 50; // Multiplier to calculate if a creature goes to attack when it has los health
        public float maxMenaceIntimidationMultiplierBasedOnMissingHealth = 2; // Multiplier to calculate the intimidation of a creature when it has low health
        public float experienceMaxAggresivenessMultiplier = 0.2f; // Percentage of the max aggressiveness used to create experiences
        public int maxDistanceToStartFollowParent = 10; // Distance to start following the parent
        public int maxDistanceToStopFollowParent = 3; // Distance to stop following the parent
        
        // Corpse
        public float rotStartMultiplier = 0.01f; // Time relative to a corpse "life time"when the corpse starts putrefying
        public float corpseNutritionPointsMultiplier = 80f; // Base nutrition points modification to a corpse

        //Taxonomy
        public float percentageSimilaritySpecies = 0.9f;// If 2 creatures's chromose similarity are under this percentage, it means their are two species

    }

    public static class UniverseParametersManager
    {
        public static void ReadJSON()
        {
            ReadJSON(UserInfo.UniverseParametersFile());
        }

        public static void ReadJSON(string uniParamsFile)
        {
            if (uniParamsFile != null)
            {
                parameters = JsonReader.Deserialize<UniverseParameters>(uniParamsFile);
                Validator.Validate(parameters);
            }
            else
                SetDefaultParameters();

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
