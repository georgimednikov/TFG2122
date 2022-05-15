namespace EvolutionSimulation.Entities
{
    public class Bush : EdiblePlant
    {
        public Bush()
        {
            type = PlantType.Bush;
            regrowthTime = (int)(UniverseParametersManager.parameters.bushHoursTillGrowth * UniverseParametersManager.parameters.ticksPerHour);
            nutritionalValue = UniverseParametersManager.parameters.bushNutritionValue;
        }
    }
}
