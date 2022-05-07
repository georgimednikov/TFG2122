namespace EvolutionSimulation.Entities
{
    public class EdibleTree : EdiblePlant
    {
        public static float movementPenalty { get; private set; } = UniverseParametersManager.parameters.treeMovementPenalty;

        public EdibleTree()
        {
            type = PlantType.EdibleTree;
            regrowthTime = RandomGenerator.Next(70, 151); // TODO: Numeros magicos
            nutritionalValue = RandomGenerator.Next(11, 22) * UniverseParametersManager.parameters.plantNutritionMultiplier; // TODO: Numeros magicos
        }
    }
}
