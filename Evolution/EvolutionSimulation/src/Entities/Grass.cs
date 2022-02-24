namespace EvolutionSimulation.Entities
{
    public class Grass : EdiblePlant
    {
        public Grass()
        {
            regrowhtTime = RandomGenerator.Next(1, 51);
            nutritionalValue = RandomGenerator.Next(1, 6);
        }

        public Grass(int regrowhtTime, float nutritionalValue)
        {
            this.regrowhtTime = regrowhtTime;
            this.nutritionalValue = nutritionalValue;
        }
    }
}
