using System;
using EvolutionSimulation.Entities;

namespace EvolutionSimulation
{
    class Corpse : IEntity, IInteractable<Creature>
    {
        public enum CorpseState
        {
            Edible,
            Putrid
        }
        public Corpse()
        {
            rnd = new Random();
        }

        /// <summary>
        /// Initializes the corpse.
        /// <param name="corpseDuration"> 
        /// The number of ticks that the corpses stays
        /// </param>
        /// <param name="putridStart">
        /// The % of the corpse duration when the corpse starts putrefying
        /// </param>
        /// <param name="putridPoisonProb">
        /// The probability of being poisoned when interacting with the putrid corpse. 
        /// A value between 0 and 100 (%), higher or lower values will be clamped.
        /// </param>
        /// <param name="maxNutritionPoints">
        /// Nutrition points acquired when consuming the corpse.
        /// They get lower the more putrid the corpse gets
        /// TODO: Hacer que sea menos puntos de nutricion cuando se este pudriendo
        /// </param>
        public void Init(int corpseDuration,  float putridStart, int poisonProb, int maxNutritionPoints)
        {
            this.corpseTimeLeft = corpseDuration;
            this.poisonProb = poisonProb;
            this.nutritionPoints = maxNutritionPoints;
            poisonProb = 0;
            // Corpse starts putrefying when the corpse consumed putridStart% of its duration
            putridTime = (int)(corpseDuration * putridStart);
        }

        public void OnInteract(Creature other)
        {
            int prob = RandomGenerator.Next(0, 100);
            if(state == CorpseState.Putrid)
            {
                // Putrid corpse, probability to be poisoned gets higher the more time it passes 
                int actualPoisonProb = poisonProb + (int)(putridTime / (float)corpseTimeLeft);
                if (prob < actualPoisonProb)
                    Console.WriteLine("The creature is poisoned");
            }
            // TODO: Considerar la habilidad carroniero
            // TODO: Aniadir puntos de nutricion a la creatura
        }

        public void Tick()
        {
            corpseTimeLeft--;
            state = corpseTimeLeft > putridTime ? CorpseState.Edible : CorpseState.Putrid;

            if (corpseTimeLeft == 0)
            {
                Console.WriteLine("Desaparece");
                EntityManager.Instance.Delete(this);
            }
        }

        Random rnd;
        int corpseTimeLeft;
        int poisonProb;
        int nutritionPoints;

        // States
        CorpseState state;
        int putridTime;
    }
}
