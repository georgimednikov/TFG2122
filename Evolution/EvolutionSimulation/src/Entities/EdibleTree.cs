namespace EvolutionSimulation.Entities
{
    public class EdibleTree : EdiblePlant
    {
        public static float movementPenalty { get; private set; } = 0.7f; // TODO: poner en config

        public EdibleTree()
        {
            regrowthTime = RandomGenerator.Next(50, 101);
            nutritionalValue = RandomGenerator.Next(5, 11);
        }

        public EdibleTree(int regrowhtTime, float nutritionalValue)
        {
            this.regrowthTime = regrowhtTime;
            this.nutritionalValue = nutritionalValue;
        }
    }
}
