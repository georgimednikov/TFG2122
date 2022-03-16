﻿using System;
using EvolutionSimulation;

namespace EvolutionSimulation.Entities
{
    public class Corpse : StaticEntity, IInteractable<Creature>
    {
        /// <summary>
        /// Sets the corpse traits 
        /// </summary>
        /// <param name="w"> World in which it'll reside </param>
        /// <param name="lifeTime"> LifeTime in Ticks of the Entity </param>
        /// <param name="x"> X World position </param>
        /// <param name="y"> Y World position </param>
        /// <param name="putridStart">
        /// A value from 0 to 1 that tells when the corpse starts putrefying. 
        /// 1 means the corpse starts putrefying inmediatly, and 0 that it will never putrefy
        /// </param>
        /// <param name="poisonProb">
        /// The probability of being poisoned when interacting with the putrid corpse. 
        /// A value between 0 and 1, higher or lower values will be clamped.
        /// </param>
        public void SetTraits(int lifeTime, float putridStart, float poisonProb, int poisonDuration, float poisonTickDamage, float maxNutritionPoints)
        {
            this.lifeTime = lifeTime;
            this.poisonProb = Math.Min(1.0f, Math.Max(0.0f, poisonProb));
            this.poisonDuration = poisonDuration;
            this.poisonTickDamage = poisonTickDamage;
            this.maxNutritionPoints = maxNutritionPoints;
            // Corpse starts putrifying when the corpse reaches the 'putridStart' part of its duration
            putridTime = (int)(lifeTime * putridStart);
        }

        /// <summary>
        /// Sets the corpse traits based on a creature stats
        /// </summary>
        public void SetTraits(Creature creature)
        {
            // From an hour to two weeks of lifetime, depending on size
            lifeTime = World.ticksHour
                        + ((creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Size) * World.ticksHour * World.hoursDay * 7) - World.ticksHour)
                        * (creature.stats.Size / creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Size)); 

            // The less health, the faster the rot
            float putridStart = creature.stats.MaxHealth * UniverseParametersManager.parameters.rotStartMultiplier;   
            putridTime = (int)(lifeTime * putridStart);

            // If it is venomous it will be more risky to eat 
            if (creature.HasAbility(Genetics.CreatureFeature.Venomous, UniverseParametersManager.parameters.abilityUnlockPercentage))
                poisonProb = creature.stats.Venom / creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Venomous);
            else
                poisonProb = 0;

            poisonDuration = (int)creature.stats.Venom;
            poisonTickDamage = creature.stats.Venom * 0.25f;    // TODO: Nº magico aqui ojo

            // TODO: testiar
            maxNutritionPoints = UniverseParametersManager.parameters.corpseNutritionPointsMultiplier 
                                * creature.stats.Size / creature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Size);  
        }

        //TODO, hacer un setTraits con parametros default que se saquen del UniverseParametersManager

        public void ReceiveInteraction(Creature other, Interactions type)
        {
            if (type != Interactions.eat) return;

            if(Edible)
                other.stats.CurrEnergy += maxNutritionPoints;
            // Corpse putrefaction cause penalties on the creature eating it
            else
            {
                float remains = lifeTime / (float)putridTime;
                float prob = (float)RandomGenerator.NextDouble();
                // Putrid corpse, probability to be poisoned gets higher the more time it passes 
                // The scavenger stat reduces the probability of being poisoned
                float actualPoisonProb = Math.Min(1.0f, Math.Max(0.0f, poisonProb + 1.0f - remains));
                float actualNutritionPoints = maxNutritionPoints * remains;
                // Having the ability 'Scavenger' reduces the penalties of eating the corpse            
                if (other.HasAbility(Genetics.CreatureFeature.Scavenger, 0.4f)) // TODO: ahora está puesto el % de unlock a pelo, cambiarlo
                {
                    actualPoisonProb -= other.stats.Scavenger;
                    actualNutritionPoints += (maxNutritionPoints - actualNutritionPoints) * other.stats.Scavenger;
                }
                // Eating the corpse increases the energy of the creature 
                other.stats.CurrEnergy += actualNutritionPoints;
                // The creature can be poisoned when eating the corpse
                if (prob < actualPoisonProb)
                    other.AddStatus(new Status.Poison(poisonDuration, poisonTickDamage));
            }                      
        }

        public override void Tick()
        {
            lifeTime--;
            Edible = lifeTime > putridTime;

            Console.WriteLine("Ticks to corpse disappearance: " + lifeTime);

            if (lifeTime <= 0)
                world.Destroy(ID);
        }

        // Copse state: it is edible until it starts putrefying
        public bool Edible { get; private set; }
        // Remaining life time of this entity
        public int lifeTime { get; protected set; }

        // Probability of being poisoned when interacting
        float poisonProb;
        // Nutrition points added to creatures that eat the corpse
        float maxNutritionPoints;
        // Duration (in Ticks) of the poison that it can be applied to the creature
        int poisonDuration;
        // Poison damage applied to the creature every tick
        float poisonTickDamage;
        // Time when the corpse starts putrefying
        int putridTime;
    }
}
