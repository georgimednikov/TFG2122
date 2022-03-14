namespace EvolutionSimulation.Entities
{
    public class EdibleTree : EdiblePlant
    {
        public static float movementPenalty { get; private set; } = UniverseParametersManager.parameters.treeMovementPenalty;

        public EdibleTree()
        {
            type = PlantType.EdibleTree;
            regrowthTime = RandomGenerator.Next(50, 101);
            nutritionalValue = RandomGenerator.Next(5, 11);
        }

        public EdibleTree(int regrowhtTime, float nutritionalValue)
        {
            type = PlantType.EdibleTree;
            this.regrowthTime = regrowhtTime;
            this.nutritionalValue = nutritionalValue;
        }
    }
}
