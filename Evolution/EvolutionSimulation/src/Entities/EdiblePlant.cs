namespace EvolutionSimulation.Entities
{
    public class EdiblePlant : Plant
    {
        public bool eaten { get; protected set; } = false;
        protected int regrowthTime;
        protected int startRegrowthTime = 0;
        protected float nutritionalValue;

        override public void Tick()
        {
            if (eaten)
                eaten = startRegrowthTime++ >= regrowthTime;            
        }

        public float Eat()
        {
            startRegrowthTime = 0;
            eaten = true;
            return nutritionalValue;
        }

    }
}
