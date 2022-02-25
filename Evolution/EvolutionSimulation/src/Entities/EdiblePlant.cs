namespace EvolutionSimulation.Entities
{
    public class EdiblePlant : Plant
    {
        public bool eaten { get; protected set; } = false;
        protected int regrowhtTime;
        protected int startRegrowthTime = 0;
        protected float nutritionalValue;

        override public void Tick()
        {
            if (eaten)
                eaten = startRegrowthTime++ >= regrowhtTime;            
        }

        /// <summary>
        /// if the plant has been eaten, it doesn't give nutritional value
        /// </summary>
        /// <returns> Return the nutritional value</returns>
        public float Eat()
        {
            if (eaten) return 0;

            startRegrowthTime = 0;
            eaten = true;
            return nutritionalValue;
        }

    }
}
