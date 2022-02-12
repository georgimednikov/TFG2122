using System;
using EvolutionSimulation.Entities;

namespace EvolutionSimulation
{
    public class Corpse : StableEntity, IInteractable<Creature>
    {
        public enum CorpseState
        {
            Edible,
            Putrid
        }

        /// <summary>
        /// Initializes the Corpse
        /// </summary>
        /// <param name="w"> World in which it'll reside </param>
        /// <param name="lifeTime"> LifeTime in Ticks of the Entity </param>
        /// <param name="x"> X World position </param>
        /// <param name="y"> Y World position </param>
        /// <param name="putridStart">
        /// The % of the corpse duration when the corpse starts putrefying
        /// </param>
        /// <param name="poisonProb">
        /// The probability of being poisoned when interacting with the putrid corpse. 
        /// A value between 0 and 100 (%), higher or lower values will be clamped.
        /// </param>
        /// <param name="maxNutritionPoints">
        /// Nutrition points acquired when consuming the corpse.
        /// They get lower the more putrid the corpse gets
        /// TODO: Hacer que sea menos puntos de nutricion cuando se este pudriendo
        /// </param>
        public void Init(World w, int lifeTime, int x, int y,  float putridStart, int poisonProb, int maxNutritionPoints)
        {
            base.Init(w, lifeTime, x, y);
            this.poisonProb = poisonProb;
            this.nutritionPoints = maxNutritionPoints;
            // Corpse starts putrefying when the corpse consumed putridStart% of its duration
            putridTime = (int)(lifeTime * putridStart);
        }

        public void OnInteract(Creature other)
        {
            int prob = RandomGenerator.Next(0, 100);
            if(state == CorpseState.Putrid)
            {
                // Putrid corpse, probability to be poisoned gets higher the more time it passes 
                int actualPoisonProb = (int)(poisonProb + (putridTime / (float)lifeTime));
                if (prob < actualPoisonProb)
                    Console.WriteLine("The creature is poisoned");
            }
            // TODO: Considerar la habilidad carroniero
            // TODO: Aniadir puntos de nutricion a la creatura
        }

        override protected void Update()
        {
            state = lifeTime > putridTime ? CorpseState.Edible : CorpseState.Putrid;

            if (state == CorpseState.Putrid)
                nutritionPoints -= putridTime / (float)lifeTime;

            if (lifeTime <= 0)
            {
                Console.WriteLine("Desaparece");
                world.Destroy(this);
            }
        }

        // Probability of being poisoned when interacting
        float poisonProb;
        // Nutrition points added to creatures that eat the corpse
        float nutritionPoints;

        // Current Corpse State
        CorpseState state;
        // Time when the corpse starts putrefying
        int putridTime;
    }
}
