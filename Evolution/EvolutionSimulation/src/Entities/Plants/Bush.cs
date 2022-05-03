namespace EvolutionSimulation.Entities
{
    public class Bush : EdiblePlant
    {
        public Bush()
        {
            type = PlantType.Bush;
            regrowthTime = RandomGenerator.Next(20, 101); // TODO: Numeros magicos
            nutritionalValue = RandomGenerator.Next(5, 11) * UniverseParametersManager.parameters.plantNutritionMultiplier; // TODO: Numeros magicos
        }
    }
}
