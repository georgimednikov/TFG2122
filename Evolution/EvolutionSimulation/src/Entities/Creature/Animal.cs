using EvolutionSimulation.Genetics;
using System;

namespace EvolutionSimulation.Entities
{
    public class Animal : Creature
    {
        public override void SetStats()
        {
            float abilityUnlock = UniverseParametersManager.parameters.abilityUnlockPercentage;

            int minHealth = UniverseParametersManager.parameters.minHealth;
            int healthValue = UniverseParametersManager.parameters.healthGainMultiplier;
            int maxLimbs = UniverseParametersManager.parameters.maxLimbs;
            int minRestExpense = UniverseParametersManager.parameters.minRestExpense;
            int maxRestExpense = UniverseParametersManager.parameters.maxRestExpense;
            //int minEnergy = 50;
            //int sizeToEnergyRatio = 2; //The creature gains 1 point of max energy for every sizeToEnergyRatio of Size
            int resourceAmount = UniverseParametersManager.parameters.resourceAmount; //Max amount of /*energy/*/sleep/hydration
            int minPerception = UniverseParametersManager.parameters.minPerception;
            int maxPerception = UniverseParametersManager.parameters.maxPerception;
            float minLifeSpan = UniverseParametersManager.parameters.minLifeSpan; // Minimum years alive
            float exhaustToSleepRatio = UniverseParametersManager.parameters.exhaustToSleepRatio; //The creature has to spend sleepToExhaustRatio hours awake per hour asleep
            float perceptionWithoutNightVision = UniverseParametersManager.parameters.perceptionWithoutNightVision; //Percentage of perception at night when the creature does not have night vision.
            float minPerceptionWithNightVision = UniverseParametersManager.parameters.minPerceptionWithNightVision; //Minimum percentage of perception at night when the creature has night vision.
            float minMobilityMedium = UniverseParametersManager.parameters.minMobilityMedium; //The slowest speed possible when moving through a special tile is its mobility * (0.6 - 1.0) depending on proficiency
            float mobilityPenalty = UniverseParametersManager.parameters.mobilityPenalty; //The more evolved the animal is to move on a medium different than the ground the worse it moves in relation to the ground
                                                                                          //A ground creature moves fast on the ground, but cannot move throught the air/trees
                                                                                          //An arboreal creature moves fast through trees, but arborealSpeed * mobilityPenalty on the ground
                                                                                          //An aerial creature moves fast in the air, but arborealSpeed = aerialSpeed * mobilityPenalty and groundSpeed = arborealSpeed * mobilityPenalty

            //TODO: Poner esto en el cromosoma, tiempo de embarazo, tiempo entre celos, tiempo en celo
            float timeBetweenHeats = UniverseParametersManager.parameters.yearsBetweenHeats; //time in years between two heats or give birth and the next heat 

            stats.TimeBetweenHeats = world.YearToTick(timeBetweenHeats);

            //Longevity is in years so we parse it to ticks and add a minimun value 
            stats.LifeSpan = world.YearToTick(chromosome.GetFeature(CreatureFeature.Longevity) + minLifeSpan);

            //Multipliers
            stats.HealthRegeneration = UniverseParametersManager.parameters.healthRegeneration;
            stats.MaxSpeed = UniverseParametersManager.parameters.maxSpeed;

            stats.Gender = chromosome.GetGender();

            stats.Diet = (Diet)SetStatInRange(CreatureFeature.Diet, (int)Diet.Count);

            //Minimum health plus bonus health
            stats.MaxHealth = chromosome.GetFeature(CreatureFeature.Constitution) * healthValue + minHealth;
            // Setting CurrHealth is unnecessary
            stats.Damage = chromosome.GetFeature(CreatureFeature.Strength);
            stats.Armor = chromosome.GetFeature(CreatureFeature.Fortitude);
            stats.Perforation = chromosome.GetFeature(CreatureFeature.Piercing);

            //See mobilityPenalty commentary
            bool wings = chromosome.HasAbility(CreatureFeature.Wings, CreatureChromosome.AbilityUnlock[CreatureFeature.Wings]);
            bool arboreal = chromosome.HasAbility(CreatureFeature.Arboreal, CreatureChromosome.AbilityUnlock[CreatureFeature.Arboreal]);
            stats.Upright = chromosome.HasAbility(CreatureFeature.Upright, CreatureChromosome.AbilityUnlock[CreatureFeature.Upright]);
            int speed = Math.Max(10,chromosome.GetFeature(CreatureFeature.Mobility));
            stats.AirReach = wings;
            stats.TreeReach = wings || arboreal || stats.Upright;

            stats.AerialSpeed = stats.ArborealSpeed = -1;
            if (wings)
            {
                stats.AerialSpeed = Math.Max((int)((speed * (minMobilityMedium + (1 - minMobilityMedium) * chromosome.GetFeaturePercentage(CreatureFeature.Wings))) - (chromosome.GetFeaturePercentage(CreatureFeature.Size) * stats.MaxSpeed)), 0);
                stats.ArborealSpeed = Math.Max((int)((stats.AerialSpeed * mobilityPenalty) - (chromosome.GetFeaturePercentage(CreatureFeature.Size) * stats.MaxSpeed) / 2f), 0);
                stats.GroundSpeed = (int)(stats.ArborealSpeed * mobilityPenalty);
            }
            if (stats.TreeReach)//TODO igual es arboreal, pero no estoy seguro
            {
                stats.ArborealSpeed = Math.Max((int)((speed * (minMobilityMedium + (1 - minMobilityMedium) * chromosome.GetFeaturePercentage(CreatureFeature.Arboreal))) - (chromosome.GetFeaturePercentage(CreatureFeature.Size) * stats.MaxSpeed) / 2f), 0);
                stats.GroundSpeed = (int)(stats.ArborealSpeed * mobilityPenalty);
            }
            if (!wings && !arboreal)
            {
                stats.GroundSpeed = speed;
            }

            //Physique related stats
            stats.Size = chromosome.GetFeature(CreatureFeature.Size);

            if (!chromosome.HasAbility(CreatureFeature.Scavenger, CreatureChromosome.AbilityUnlock[CreatureFeature.Scavenger])) stats.Scavenger = 0;
            else stats.Scavenger = chromosome.GetFeaturePercentage(CreatureFeature.Scavenger);
            if (!chromosome.HasAbility(CreatureFeature.Venomous, CreatureChromosome.AbilityUnlock[CreatureFeature.Venomous])) stats.Venom = 0;
            else stats.Venom = chromosome.GetFeature(CreatureFeature.Venomous);
            if (!chromosome.HasAbility(CreatureFeature.Thorns, CreatureChromosome.AbilityUnlock[CreatureFeature.Thorns])) stats.Counter = 0;
            else stats.Counter = chromosome.GetFeature(CreatureFeature.Thorns);

            stats.Members = SetStatInRange(CreatureFeature.Members, maxLimbs + 1);//its not inclusive [0-11)

            stats.Metabolism = chromosome.GetFeature(CreatureFeature.Metabolism);
            
            stats.IdealTemperature = (chromosome.GetFeature(CreatureFeature.IdealTemperature) 
                / (double)chromosome.GetFeatureMax(CreatureFeature.IdealTemperature));
            double tempInRange = (chromosome.GetFeature(CreatureFeature.TemperatureRange) 
                / (double)chromosome.GetFeatureMax(CreatureFeature.TemperatureRange)) * 0.5f; // TODO: Numero magico
            stats.MinTemperature = stats.IdealTemperature - tempInRange;
            stats.MaxTemperature = stats.IdealTemperature + tempInRange;

            stats.MaxEnergy = resourceAmount; // minEnergy + stats.Size / sizeToEnergyRatio; TODO: en teoria es el mismo valor todos los recursos, cambia el gasto
            stats.CurrEnergy = stats.MaxEnergy;
            stats.EnergyExpense = (stats.MaxEnergy / (UniverseParametersManager.parameters.hoursTilStarvation * World.ticksHour)) *
            (stats.Metabolism / (float)chromosome.GetFeatureMax(CreatureFeature.Metabolism) * (stats.Members / 2f) +
            (stats.Venom / 2f + stats.Counter / 2f) * 0.5f);
            //(stats.MaxEnergy / (8 * World.ticksHour)) * ((float)Math.Pow(ratio, 2) / (float)Math.Pow(0.5, 2));

            stats.MaxHydration = resourceAmount;
            stats.CurrHydration = stats.MaxHydration;
            stats.HydrationExpense = (stats.EnergyExpense * UniverseParametersManager.parameters.thirstToHungerRatio);

            stats.MaxRest = resourceAmount;
            stats.CurrRest = stats.MaxRest;
            float re = stats.MaxRest / (UniverseParametersManager.parameters.hoursTilExhaustion * UniverseParametersManager.parameters.ticksPerHour);
            stats.RestExpense = re / 2f +   // TODO: Numeros arcanos
                (re / 2f * (1 - chromosome.GetFeaturePercentage(CreatureFeature.Resistance)));
            //    minRestExpense + (maxRestExpense - minRestExpense) * 
            //    (1 - (float)chromosome.GetFeature(CreatureFeature.Resistance) / chromosome.GetFeatureMax(CreatureFeature.Resistance));
            stats.RestRecovery = stats.RestExpense * exhaustToSleepRatio;

            //Environment related stats
            stats.Camouflage = chromosome.GetFeature(CreatureFeature.Camouflage);
            stats.Aggressiveness = chromosome.GetFeature(CreatureFeature.Aggressiveness);
           
            //If the creature does not have the feature night vision then its perception will be the lowest posible,
            //So instead of Perception * 1 it will be Perception * minNightVision
            if (!chromosome.HasAbility(CreatureFeature.NightVision, CreatureChromosome.AbilityUnlock[CreatureFeature.NightVision]))
                stats.NightPerceptionPercentage = perceptionWithoutNightVision;

            //Else it is calculated what percentage of the ability the creature has unlocked, removing the minimum value needed to have the ability per se,
            //and then depending on that percentage the creature has a NightPenalty that goes from minNightVision to 1.
            else
            {
                int maxNightVisionGene = chromosome.GetFeatureMax(CreatureFeature.NightVision);
                int offset = (int)(abilityUnlock * maxNightVisionGene);
                float percentageOfNightVision = (float)(chromosome.GetFeature(CreatureFeature.NightVision) - offset) / (chromosome.GetFeatureMax(CreatureFeature.NightVision) - offset);
                stats.NightPerceptionPercentage = minPerceptionWithNightVision + (1 - minPerceptionWithNightVision) * percentageOfNightVision;
            }

            //Value that multiplies perception when it is being gotten
            stats.CurrentVision = world.day ? 1 : stats.NightPerceptionPercentage;
            stats.ActionPerceptionPercentage = 1;

            int maxPerceptionGene = chromosome.GetFeatureMax(CreatureFeature.Perception);
            float range = Math.Max(1.0f / maxPerceptionGene, (float)chromosome.GetFeature(CreatureFeature.Perception) / maxPerceptionGene);

            stats.Perception = (int)(minPerception + (maxPerception - minPerception) * range);
            stats.MaxPerception = minPerception + (maxPerception - minPerception) * 1;

            //If the creature can see in the dark, that penalty is reduced the better sight it has
            if (chromosome.HasAbility(CreatureFeature.NightVision, CreatureChromosome.AbilityUnlock[CreatureFeature.NightVision]))
                stats.CurrentVision *= 1 - chromosome.GetFeaturePercentage(CreatureFeature.NightVision);


            //Behaviour related stats
            stats.Knowledge = chromosome.GetFeature(CreatureFeature.Knowledge);

            if (!chromosome.HasAbility(CreatureFeature.Paternity, CreatureChromosome.AbilityUnlock[CreatureFeature.Paternity])) stats.Paternity = 0;
            else stats.Paternity = chromosome.GetFeature(CreatureFeature.Paternity);

            ModifyStatsByAbilities();
        }

        /// <summary>
        /// Modify differents stats depending on the abilities
        /// </summary>
        /// <param name="abilityUnlock"></param>
        private void ModifyStatsByAbilities()
        {
            //Hair. Better with low temperatures and worse with high temperatures
            stats.Hair = chromosome.HasAbility(CreatureFeature.Hair, CreatureChromosome.AbilityUnlock[CreatureFeature.Hair]);
            if (stats.Hair)
            {
                int hairValue = chromosome.GetFeature(CreatureFeature.Hair);
                double maxHairValue = chromosome.GetFeatureMax(CreatureFeature.Hair);
                double increase = (hairValue / maxHairValue) * UniverseParametersManager.parameters.hairTemperatureMultiplier;
                stats.MinTemperature -= increase * 2;
                stats.MinTemperature = Math.Max(stats.MinTemperature, 0);
                stats.MaxTemperature += increase;
                stats.MaxTemperature = Math.Min(stats.MaxTemperature, 1);
            }

            //UpRight increase the perception at most 1.5
            if (chromosome.HasAbility(CreatureFeature.Upright, CreatureChromosome.AbilityUnlock[CreatureFeature.Upright]))
            {
                float increase = 1.0f + chromosome.GetFeaturePercentage(CreatureFeature.Upright) / 2.0f;

                stats.Perception = (int)(stats.Perception * increase);
                increase = 1.0f + 1.0f / 2.0f;
                stats.MaxPerception = (int)(stats.MaxPerception * increase);
            }

            //Intimidation has to be calculed here because of modifyStatByAge
            float intimidation = stats.Size / 2; 

            //Horns. Increase damage and intimidation
            if (chromosome.HasAbility(CreatureFeature.Horns, CreatureChromosome.AbilityUnlock[CreatureFeature.Horns]))
            {
                int hornsValue = chromosome.GetFeature(CreatureFeature.Horns);
                stats.Damage += hornsValue;
                intimidation += hornsValue * UniverseParametersManager.parameters.hornIntimidationMultiplier;
            }


            float increaseIntimidation = 0;
            //Mimic increase the intimidation at most twice
            if (chromosome.HasAbility(CreatureFeature.Mimic, CreatureChromosome.AbilityUnlock[CreatureFeature.Mimic]))
            {
                increaseIntimidation = 1.0f + chromosome.GetFeaturePercentage(CreatureFeature.Mimic);
            }

            stats.Intimidation = (int)(intimidation * increaseIntimidation);
        }

        /// <summary>
        /// Calculate a number that represent the stat of a given feature
        /// It's calculated dividing the maxValue of the feature in ranges by max
        /// It's not inclusive, the values are in the range [0,max)
        /// </summary>
        private int SetStatInRange(CreatureFeature feature, int max)
        {
            if (max < 0) return -1;
            //+1 to avoid a number greater than the max value
            int maxstat = chromosome.GetFeatureMax(feature) + 1;
            if (maxstat <= 0) return -1;
            int statCh = chromosome.GetFeature(feature);
            int stat = (int)(statCh / (maxstat / ((float)max)));
            return stat;
        }

        public override void SetStats(CreatureBaseStats baseStats)
        {
            stats.TimeBetweenHeats = baseStats.TimeBetweenHeats;
            stats.LifeSpan = baseStats.LifeSpan;
            stats.HealthRegeneration = baseStats.HealthRegeneration;
            stats.MaxSpeed = baseStats.MaxSpeed;
            stats.Gender = baseStats.Gender;
            stats.Diet = baseStats.Diet;
            stats.MaxHealth = baseStats.MaxHealth;
            stats.Damage = baseStats.Damage;
            stats.Armor = baseStats.Armor;
            stats.Perforation = baseStats.Perforation;
            stats.Upright = baseStats.Upright;
            stats.AirReach = baseStats.AirReach;
            stats.TreeReach = baseStats.TreeReach;
            stats.AerialSpeed = baseStats.AerialSpeed;
            stats.ArborealSpeed = baseStats.ArborealSpeed;
            stats.GroundSpeed = baseStats.GroundSpeed;
            stats.Size = baseStats.Size;
            stats.Scavenger = baseStats.Scavenger;
            stats.Venom = baseStats.Venom;
            stats.Counter = baseStats.Counter;
            stats.Members = baseStats.Members;
            stats.Metabolism = baseStats.Metabolism;
            stats.IdealTemperature = baseStats.IdealTemperature;
            stats.MinTemperature = baseStats.MinTemperature;
            stats.MaxTemperature = baseStats.MaxTemperature;
            stats.MaxEnergy = baseStats.MaxEnergy; 
            stats.CurrEnergy = stats.MaxEnergy;
            stats.EnergyExpense = baseStats.EnergyExpense;
            stats.MaxHydration = baseStats.MaxHydration;
            stats.CurrHydration = stats.MaxHydration;
            stats.HydrationExpense = baseStats.HydrationExpense;
            stats.MaxRest = baseStats.MaxRest;
            stats.CurrRest = stats.MaxRest;
            stats.RestExpense = baseStats.RestExpense;
            stats.RestRecovery = baseStats.RestRecovery;
            stats.Camouflage = baseStats.Camouflage;
            stats.Aggressiveness = baseStats.Aggressiveness;
            stats.Perception = baseStats.Perception;
            stats.MaxPerception = baseStats.MaxPerception;
            stats.NightPerceptionPercentage = stats.NightPerceptionPercentage;
            stats.CurrentVision = world.day ? 1 : stats.NightPerceptionPercentage;
            stats.ActionPerceptionPercentage = 1;
            stats.Knowledge = baseStats.Knowledge;
            stats.Paternity = baseStats.Paternity;
            stats.Hair = baseStats.Hair;
            stats.Intimidation = baseStats.Intimidation;
        }
    }
}
