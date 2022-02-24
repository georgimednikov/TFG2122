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

        public float Eat()
        {
            startRegrowthTime = 0;
            eaten = true;
            return nutritionalValue;
        }

    }
}
