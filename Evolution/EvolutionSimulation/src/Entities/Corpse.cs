using System;

namespace EvolutionSimulation.Entities
{
    public class Corpse : StaticEntity, IInteractable<Creature>
    {
        /// <summary>
        /// Initializes the Corpse
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
        public void Init(World w, int lifeTime, int x, int y,  float putridStart, float poisonProb, int poisonDuration, float poisonTickDamage, float maxNutritionPoints, float hp)
        {
            Init(w, x, y, hp);

            this.lifeTime = lifeTime;

            // Clamp
            this.poisonProb = Math.Min(1.0f, Math.Max(0.0f, poisonProb));
            this.poisonDuration = poisonDuration;
            this.poisonTickDamage = poisonTickDamage;

            this.maxNutritionPoints = maxNutritionPoints;
            // Corpse starts putrifying when the corpse reaches the 'putridStart' part of its duration
            putridTime = (int)(lifeTime * putridStart);
            Console.WriteLine("Corpse created at " + x + ", " + y + ".");
        }

        /// <summary>
        /// Handles the eating interaction
        /// </summary>
        public void ReceiveInteraction(Creature other, Interactions type)
        {
            if (type != Interactions.eat) return;

            if (Edible) // The creature bites the corpse, getting fed relative to the bite taken
            {
                float dealt = Math.Min(other.stats.Damage, curHp);
                other.stats.CurrEnergy += (dealt / maxHp) * maxNutritionPoints;
                curHp -= dealt;
            }
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
                if (other.HasAbility(Genetics.CreatureFeature.Scavenger, 
                    Genetics.CreatureChromosome.AbilityUnlock[Genetics.CreatureFeature.Scavenger])) // TODO: ahora está puesto el % de unlock a pelo, cambiarlo
                {
                    actualPoisonProb -= other.stats.Scavenger;
                    actualNutritionPoints += (maxNutritionPoints - actualNutritionPoints) * other.stats.Scavenger;
                }
                // Eating the corpse increases the energy of the creature 
                float dealt = Math.Min(other.stats.Damage, curHp);
                other.stats.CurrEnergy += (dealt / maxHp) * actualNutritionPoints;
                curHp -= dealt;
                // The creature can be poisoned when eating the corpse
                if (prob < actualPoisonProb && dealt > 0)   // In case another creature tried eating an empty corpse
                    other.AddStatus(new Status.Poison(poisonDuration, poisonTickDamage));
            }                      
        }

        /// <summary>
        /// Destroys the corpse when it reaches the end of its lifetime
        /// or if it is completely eaten
        /// </summary>
        override public void Tick()
        {
            lifeTime--;
            Edible = lifeTime > putridTime;

            if (lifeTime <= 0 || curHp <= 0)
                world.Destroy(this);
            else Console.WriteLine("Ticks to corpse disappearance: " + lifeTime);
        }

        // Copse state: it is edible until it starts putrefying
        public bool Edible { get; private set; }
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
        // Remaining life time of this entity
        public int lifeTime { get; protected set; }
    }
}
