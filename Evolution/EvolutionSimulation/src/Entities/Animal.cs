using EvolutionSimulation.Genetics;

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
            int maxMembers = 10;
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

            ////Multipliers
            stats.healthRegeneration = 0.1f;
            stats.maxSpeed = 1.5f;

            stats.gender = chromosome.GetGender();

            //The max value is divided in ranges based on the amount of diets and then a diet is assigned based on the range it fall in
            //Calculating the max value +1 to include the 0 since the range is inclusive.
            stats.diet = (Diet)(chromosome.GetFeature(CreatureFeature.Diet) / (chromosome.GetFeatureMax(CreatureFeature.Diet) + 1 / (int)Diet.Count));
            if (stats.diet >= Diet.Count) stats.diet = Diet.Count;

            //Minimum health plus bonus health
            stats.maxHealth = chromosome.GetFeature(CreatureFeature.Constitution) * healthValue + minHealth;
            stats.currHealth = stats.maxHealth;

            stats.damage = chromosome.GetFeature(CreatureFeature.Strength);
            stats.armor = chromosome.GetFeature(CreatureFeature.Fortitude);
            stats.perforation = chromosome.GetFeature(CreatureFeature.Piercing);

            //See mobilityPenalty commentary
            bool wings = HasAbility(CreatureFeature.Wings, abilityUnlock);
            bool arboreal = HasAbility(CreatureFeature.Arboreal, abilityUnlock);
            bool upright = HasAbility(CreatureFeature.Upright, abilityUnlock);
            int speed = chromosome.GetFeature(CreatureFeature.Mobility);
            stats.airReach = wings;
            stats.treeReach = wings || arboreal || upright;
            if (wings)
            {
                stats.aerialSpeed = speed * (chromosome.GetFeature(CreatureFeature.Wings) * chromosome.GetFeatureMax(CreatureFeature.Wings));
                stats.arborealSpeed = (int)(stats.aerialSpeed * mobilityPenalty);
                stats.groundSpeed = (int)(stats.arborealSpeed * mobilityPenalty);
            }
            else if (arboreal)
            {
                stats.aerialSpeed = -1;
                stats.arborealSpeed = speed * (chromosome.GetFeature(CreatureFeature.Arboreal) * chromosome.GetFeatureMax(CreatureFeature.Arboreal));
                stats.groundSpeed = (int)(stats.arborealSpeed * mobilityPenalty);
            }
            else
            {
                stats.aerialSpeed = -1;
                stats.arborealSpeed = -1;
                stats.groundSpeed = speed;
            }

            ////Physique related stats
            stats.size = chromosome.GetFeature(CreatureFeature.Size);
            stats.lifeSpan = chromosome.GetFeature(CreatureFeature.Longevity);

            //Calculating the max value +1 to include the 0 since the range is inclusive.
            stats.members = chromosome.GetFeature(CreatureFeature.Members) / (chromosome.GetFeatureMax(CreatureFeature.Members + 1) / maxMembers);

            stats.metabolism = chromosome.GetFeature(CreatureFeature.Metabolism);
            stats.idealTemperature = chromosome.GetFeature(CreatureFeature.IdealTemperature);
            stats.minTemperature = stats.idealTemperature - chromosome.GetFeature(CreatureFeature.TemperatureRange);
            stats.maxTemperature = stats.idealTemperature + chromosome.GetFeature(CreatureFeature.TemperatureRange);

            stats.maxEnergy = minEnergy + stats.size / sizeToEnergyRatio;
            stats.currEnergy = stats.maxEnergy;
            stats.energyExpense = 1 + stats.metabolism / chromosome.GetFeatureMax(CreatureFeature.Metabolism);

            stats.maxHydratation = resourceAmount;
            stats.currHydratation = stats.maxHydratation;
            stats.hydratationExpense = stats.energyExpense;

            stats.maxRest = resourceAmount;
            stats.currRest = stats.maxRest;
            stats.restExpense = minRestExpense + (maxRestExpense - minRestExpense) * (1 - chromosome.GetFeature(CreatureFeature.Resistence) / chromosome.GetFeatureMax(CreatureFeature.Resistence));
            stats.restRecovery = stats.restExpense * exhaustToSleepRatio;

            //Environment related stats
            stats.camouflage = chromosome.GetFeature(CreatureFeature.Camouflage);
            stats.aggressiveness = chromosome.GetFeature(CreatureFeature.Aggressiveness);
            stats.perception = chromosome.GetFeature(CreatureFeature.Perception);

            //A percentage equal to nightPerceptionPenalty of the max perception is lost at night
            stats.nightDebuff = chromosome.GetFeatureMax(CreatureFeature.Perception) * nightPerceptionPenalty;
            //If the creature can see in the dark, that penalty is reduced the better sight it has
            if (HasAbility(CreatureFeature.NightVision, abilityUnlock))
                stats.nightDebuff *= 1 - (chromosome.GetFeature(CreatureFeature.NightVision) / chromosome.GetFeatureMax(CreatureFeature.NightVision));


            ////Behaviour related stats
            stats.knowledge = chromosome.GetFeature(CreatureFeature.Knowledge);
            stats.paternity = chromosome.GetFeature(CreatureFeature.Paternity);

            if (!HasAbility(CreatureFeature.Scavenger, abilityUnlock)) stats.scavenger = -1;
            else stats.scavenger = chromosome.GetFeature(CreatureFeature.Scavenger) / chromosome.GetFeatureMax(CreatureFeature.Scavenger);
            if (!HasAbility(CreatureFeature.Venomous, abilityUnlock)) stats.venom = -1;
            else stats.venom = chromosome.GetFeature(CreatureFeature.Venomous);
            if (!HasAbility(CreatureFeature.Thorns, abilityUnlock)) stats.counter = -1;
            else stats.counter = chromosome.GetFeature(CreatureFeature.Thorns);
            if (!HasAbility(CreatureFeature.Mimic, abilityUnlock)) stats.intimidation = -1;
            else stats.intimidation = chromosome.GetFeature(CreatureFeature.Mimic);
        }

        private bool HasAbility(CreatureFeature feat, float unlock)
        {
            return unlock < chromosome.GetFeature(feat) / chromosome.GetFeatureMax(feat);
        }
    }
}
