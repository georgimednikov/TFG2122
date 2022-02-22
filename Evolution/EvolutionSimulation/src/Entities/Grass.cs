using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation
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
