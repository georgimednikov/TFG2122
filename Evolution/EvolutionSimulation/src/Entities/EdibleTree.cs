namespace EvolutionSimulation.Entities
{
    public class EdibleTree : EdiblePlant
    {
        public static float movementPenalty { get; private set; } = UniverseParametersManager.parameters.treeMovementPenalty;

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
