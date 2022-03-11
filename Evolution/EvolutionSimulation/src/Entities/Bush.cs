namespace EvolutionSimulation.Entities
{
    public class Bush : EdiblePlant
    {
        public Bush()
        {
            type = PlantType.Bush;
            regrowthTime = RandomGenerator.Next(1, 101);
            nutritionalValue = RandomGenerator.Next(5, 11);
        }

        public Bush(int regrowhtTime, float nutritionalValue)
        {
            type = PlantType.Bush;
            this.regrowthTime = regrowhtTime;
            this.nutritionalValue = nutritionalValue;
        }
    }
}
