namespace EvolutionSimulation.Entities
{
    public class Grass : EdiblePlant
    {
        public Grass()
        {
            type = PlantType.Grass;
            regrowthTime = RandomGenerator.Next(1, 51); // TODO: Numeros magicos
            nutritionalValue = RandomGenerator.Next(1, 6) * UniverseParametersManager.parameters.plantNutritionMultiplier; // TODO: Numeros magicos
        }
    }
}
