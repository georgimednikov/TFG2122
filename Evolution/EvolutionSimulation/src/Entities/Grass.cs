namespace EvolutionSimulation.Entities
{
    public class Grass : EdiblePlant
    {
        public Grass()
        {
            type = PlantType.Grass;
            regrowthTime = RandomGenerator.Next(1, 51);
            nutritionalValue = RandomGenerator.Next(1, 6);
        }

        public Grass(int regrowhtTime, float nutritionalValue)
        {
            type = PlantType.Grass;
            this.regrowthTime = regrowhtTime;
            this.nutritionalValue = nutritionalValue;
        }
    }
}
