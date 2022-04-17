using System;

namespace EvolutionSimulation.Entities
{
    public class EdiblePlant : Plant
    {
        public bool eaten { get; protected set; } = false;
        protected int regrowthTime;
        protected float nutritionalValue;
        override public void Tick()
        {
            if (eaten)  // If it is eaten, it remains so until it is fully grown back
                eaten = curHp < maxHp;

            // It regrows steadily even while not fully eaten
            curHp = Math.Min(curHp + (1 / (float)regrowthTime) * maxHp, maxHp);
        }

        /// <summary>
        /// if the plant has been eaten, it doesn't give nutritional value
        /// </summary>
        /// <returns> Return the nutritional value</returns>
        public void ReceiveInteraction(Creature other, Interactions type)
        {
            if (type != Interactions.eat || eaten)
            {
                Console.WriteLine("TRIED EATING EATEN PLANT");
            }

            float dealt = Math.Min(other.stats.Damage, curHp);
            other.stats.CurrEnergy += (dealt / maxHp) * nutritionalValue;
            curHp -= dealt;

            if(curHp <= 0) {
                eaten = true;
            }
        }
    }
}
