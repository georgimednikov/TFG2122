namespace EvolutionSimulation.Entities
{
    public class EdibleTree : EdiblePlant
    {
        public static float movementPenalty { get; private set; } = UniverseParametersManager.parameters.treeMovementPenalty;

        public EdibleTree()
        {
            type = PlantType.EdibleTree;
            regrowthTime = (int)(UniverseParametersManager.parameters.eTreeHoursTillGrowth * UniverseParametersManager.parameters.ticksPerHour);
            nutritionalValue = UniverseParametersManager.parameters.eTreeNutritionalValue;
        }
    }
}
