using EvolutionSimulation.Genetics;
using System;

namespace EvolutionSimulation.Entities
{
    public class Animal : Creature
    {
        public override void SetStats()
        {
            //The percentage of an ability that has to be had in order to unlock it
            float abilityUnlock = 0.4f;

            int minHealth = 10; //Minimum amount of health
            int healthValue = 2; //Health gained per point of constitution
            int maxMembers = 11;//its not inclusive [0-11)
            int minRestExpense = 1;
            int maxRestExpense = 5;
            int minEnergy = 50;
            int resourceAmount = 100; //Max amount of sleep/hydratation
            int sizeToEnergyRatio = 2; //The creature gains 1 point of max energy for every sizeToEnergyRatio of Size
            float exhaustToSleepRatio = 3; //The creature has to spend sleepToExhaustRatio hours awake per hour asleep
            float nightPerceptionPenalty = 0.4f; //Percentage of the max Perception lost at night
            float mobilityPenalty = 0.7f; //The more evolved the animal is to move on a medium different than the ground the worse it moves in relation to the ground
                                          //A ground creature moves fast on the ground, but cannot move throught the air/trees
                                          //An arboreal creature moves fast through trees, but arborealSpeed * mobilityPenalty on the ground
                                          //An aerial creature moves fast in the air, but arborealSpeed = aerialSpeed * mobilityPenalty and groundSpeed = arborealSpeed * mobilityPenalty

            stats.LifeSpan = chromosome.GetFeature(CreatureFeature.Longevity);

            ////Multipliers
            stats.HealthRegeneration = 0.1f;
            stats.MaxSpeed = 1.5f;

            stats.Gender = chromosome.GetGender();

            stats.Diet = (Diet)SetStatInRange(CreatureFeature.Diet, (int)Diet.Count);

            //Minimum health plus bonus health
            stats.MaxHealth = chromosome.GetFeature(CreatureFeature.Constitution) * healthValue + minHealth;
            // Setting CurrHealth is unnecessary
            stats.Damage = chromosome.GetFeature(CreatureFeature.Strength);
            stats.Armor = chromosome.GetFeature(CreatureFeature.Fortitude);
            stats.Perforation = chromosome.GetFeature(CreatureFeature.Piercing);

            //See mobilityPenalty commentary
            bool wings = HasAbility(CreatureFeature.Wings, abilityUnlock);
            bool arboreal = HasAbility(CreatureFeature.Arboreal, abilityUnlock);
            bool upright = HasAbility(CreatureFeature.Upright, abilityUnlock);
            int speed = chromosome.GetFeature(CreatureFeature.Mobility);
            stats.AirReach = wings;
            stats.TreeReach = wings || arboreal || upright;

            stats.AerialSpeed = stats.ArborealSpeed = -1;
            if (wings)
            {
                stats.AerialSpeed = speed * (chromosome.GetFeature(CreatureFeature.Wings) * chromosome.GetFeatureMax(CreatureFeature.Wings));
                stats.ArborealSpeed = (int)(stats.AerialSpeed * mobilityPenalty);
                stats.GroundSpeed = (int)(stats.ArborealSpeed * mobilityPenalty);
            }
            if (arboreal)
            {
                stats.ArborealSpeed = speed * (chromosome.GetFeature(CreatureFeature.Arboreal) * chromosome.GetFeatureMax(CreatureFeature.Arboreal));
                stats.GroundSpeed = (int)(stats.ArborealSpeed * mobilityPenalty);
            }
            if (!wings && !arboreal)
            {
                stats.GroundSpeed = speed;
            }

            //Physique related stats
            stats.Size = chromosome.GetFeature(CreatureFeature.Size);

            
            stats.Members = SetStatInRange(CreatureFeature.Members, maxMembers);

            stats.Metabolism = chromosome.GetFeature(CreatureFeature.Metabolism);
            stats.IdealTemperature = chromosome.GetFeature(CreatureFeature.IdealTemperature);
            stats.MinTemperature = stats.IdealTemperature - chromosome.GetFeature(CreatureFeature.TemperatureRange);
            stats.MaxTemperature = stats.IdealTemperature + chromosome.GetFeature(CreatureFeature.TemperatureRange);

            stats.MaxEnergy = minEnergy + stats.Size / sizeToEnergyRatio;
            stats.CurrEnergy = stats.MaxEnergy;
            stats.EnergyExpense = 1 + stats.Metabolism / chromosome.GetFeatureMax(CreatureFeature.Metabolism);
            stats.MaxHydration = resourceAmount;
            stats.CurrHydration = stats.MaxHydration;
            stats.HydrationExpense = stats.EnergyExpense;

            stats.MaxRest = resourceAmount;
            stats.CurrRest = stats.MaxRest;
            stats.RestExpense = minRestExpense + (maxRestExpense - minRestExpense) * (1 - chromosome.GetFeature(CreatureFeature.Resistence) / chromosome.GetFeatureMax(CreatureFeature.Resistence));
            stats.RestRecovery = stats.RestExpense * exhaustToSleepRatio;

            //Environment related stats
            stats.Camouflage = chromosome.GetFeature(CreatureFeature.Camouflage);
            stats.Aggressiveness = chromosome.GetFeature(CreatureFeature.Aggressiveness);
            stats.Perception = chromosome.GetFeature(CreatureFeature.Perception);

            //A percentage equal to nightPerceptionPenalty of the max perception is lost at night
            stats.NightDebuff = chromosome.GetFeatureMax(CreatureFeature.Perception) * nightPerceptionPenalty;
            //If the creature can see in the dark, that penalty is reduced the better sight it has
            if (HasAbility(CreatureFeature.NightVision, abilityUnlock))
                stats.NightDebuff *= 1 - (chromosome.GetFeature(CreatureFeature.NightVision) / chromosome.GetFeatureMax(CreatureFeature.NightVision));


            ////Behaviour related stats
            stats.Knowledge = chromosome.GetFeature(CreatureFeature.Knowledge);
            stats.Paternity = chromosome.GetFeature(CreatureFeature.Paternity);

            if (!HasAbility(CreatureFeature.Scavenger, abilityUnlock)) stats.Scavenger = -1;
            else stats.Scavenger = chromosome.GetFeature(CreatureFeature.Scavenger) / chromosome.GetFeatureMax(CreatureFeature.Scavenger);
            if (!HasAbility(CreatureFeature.Venomous, abilityUnlock)) stats.Venom = -1;
            else stats.Venom = chromosome.GetFeature(CreatureFeature.Venomous);
            if (!HasAbility(CreatureFeature.Thorns, abilityUnlock)) stats.Counter = -1;
            else stats.Counter = chromosome.GetFeature(CreatureFeature.Thorns);
            if (!HasAbility(CreatureFeature.Mimic, abilityUnlock)) stats.Intimidation = -1;
            else stats.Intimidation = chromosome.GetFeature(CreatureFeature.Mimic);
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


        private bool HasAbility(CreatureFeature feat, float unlock)
        {
            return unlock < chromosome.GetFeature(feat) / chromosome.GetFeatureMax(feat);
        }
    }
}
