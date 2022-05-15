namespace EvolutionSimulation.Entities
{
    public class Grass : EdiblePlant
    {
        public Grass()
        {
            type = PlantType.Grass;
            regrowthTime = (int)(UniverseParametersManager.parameters.grassHoursTillGrowth * UniverseParametersManager.parameters.ticksPerHour);
            nutritionalValue = UniverseParametersManager.parameters.grassNutritionalValue;
        }
    }
}
